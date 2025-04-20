using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenerator 
{
    [Generator]
    public class EnumLikeGenerator : IIncrementalGenerator
    {
        private const string EnumLikeAttributeName = "GeneratorAttributes.EnumLikeAttribute";
        private const string GeneratedValueAttributeName = "GeneratorAttributes.GeneratedEnumValueAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find classes decorated with [EnumLike]
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null)!;

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));

            // Add IsExternalInit definition for potential record usage internally or by generated code
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "IsExternalInit.g.cs",
                SourceText.From(IsExternalInitSource, Encoding.UTF8)));
        }

        // Quick syntax check: is it a class with attributes?
        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0;

        // Semantic check: does the class have the [EnumLike] attribute?
        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (classSymbol != null)
            {
                foreach (var attributeData in classSymbol.GetAttributes())
                {
                    if (attributeData.AttributeClass?.ToDisplayString() == EnumLikeAttributeName)
                    {
                        // Basic check: Ensure the class is partial
                        if (!classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                        {
                            // Optional: Report a diagnostic error here if not partial
                            // context.ReportDiagnostic(Diagnostic.Create(...));
                            return null; // Don't generate for non-partial classes
                        }
                        return classDeclarationSyntax;
                    }
                }
            }
            return null;
        }

        // --- Main Generation Logic ---
        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty) return;

            // Prevent generating for duplicate partial declarations
            IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

            foreach (var classSyntax in distinctClasses)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol) continue;

                string className = classSymbol.Name;
                string classNamespace = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : classSymbol.ContainingNamespace.ToDisplayString();
                string? underlyingValueTypeName = null; // Determine this from the first valid field

                var membersToGenerate = new List<EnumMemberInfo>();

                // Find relevant private static fields
                foreach (var member in classSymbol.GetMembers())
                {
                    if (member is IFieldSymbol fieldSymbol &&
                        fieldSymbol.IsStatic &&
                        fieldSymbol.IsReadOnly && // Must be readonly
                        fieldSymbol.DeclaredAccessibility == Accessibility.Private &&
                        fieldSymbol.Name.StartsWith("_"))
                    {
                        // Check for [GeneratedEnumValue] attribute
                        bool hasAttribute = fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == GeneratedValueAttributeName);

                        if (hasAttribute)
                        {
                            string privateName = fieldSymbol.Name;
                            string publicName = Capitalize(privateName.Substring(1)); // Remove "_" and capitalize
                            string fieldTypeName = fieldSymbol.Type.ToDisplayString();

                            // Determine the underlying value type from the first valid field found
                            if (underlyingValueTypeName == null)
                            {
                                underlyingValueTypeName = fieldTypeName;
                            }
                            else if (underlyingValueTypeName != fieldTypeName)
                            {
                                // Optional: Report diagnostic error - all fields must have the same type
                                continue; // Skip inconsistent types for now
                            }

                            // How to get the value? This is tricky for readonly fields initialized elsewhere.
                            // Simplification: Assume the field is initialized inline with a constant value.
                            // A robust generator would need to find the initializer syntax.
                            var fieldSyntax = fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as VariableDeclaratorSyntax;
                            string? valueLiteral = fieldSyntax?.Initializer?.Value?.ToString();

                            if (valueLiteral != null)
                            {
                                membersToGenerate.Add(new EnumMemberInfo(publicName, valueLiteral));
                            }
                            else
                            {
                                // Optional: Report diagnostic - couldn't find initializer value
                            }
                        }
                    }
                }

                // Only generate if we found members and determined the value type
                if (!membersToGenerate.Any() || underlyingValueTypeName == null)
                {
                    continue;
                }

                // --- Generate Code ---
                var sourceBuilder = new StringBuilder();
                sourceBuilder.AppendLine("// <auto-generated/>");
                sourceBuilder.AppendLine($"// Generator: {nameof(EnumLikeGenerator)}");
                sourceBuilder.AppendLine("#nullable enable");
                sourceBuilder.AppendLine("using System;");
                sourceBuilder.AppendLine("using System.Collections.Generic;");
                sourceBuilder.AppendLine("using System.Linq;");
                sourceBuilder.AppendLine();

                if (!string.IsNullOrEmpty(classNamespace))
                {
                    sourceBuilder.AppendLine($"namespace {classNamespace}");
                    sourceBuilder.AppendLine("{");
                }

                string indent = string.IsNullOrEmpty(classNamespace) ? "" : "    ";

                // Extend the existing partial class
                sourceBuilder.AppendLine($"{indent}/// <summary>");
                sourceBuilder.AppendLine($"{indent}/// Generated members for EnumLike class {className}.");
                sourceBuilder.AppendLine($"{indent}/// </summary>");
                sourceBuilder.AppendLine($"{indent}public sealed partial class {className} : IEquatable<{className}>"); // Assume sealed for simplicity
                sourceBuilder.AppendLine($"{indent}{{");

                string innerIndent = indent + "    ";

                // Static list & Instances (similar to previous generator)
                sourceBuilder.AppendLine($"{innerIndent}// Private list populated by the constructor");
                sourceBuilder.AppendLine($"{innerIndent}private static readonly System.Collections.Generic.List<{className}> _list = new System.Collections.Generic.List<{className}>();");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine($"{innerIndent}// Public static readonly instances");
                foreach (var member in membersToGenerate)
                {
                    // Call the private constructor we are about to generate
                    sourceBuilder.AppendLine($"{innerIndent}public static readonly {className} {member.PublicName} = new {className}({member.ValueLiteral}, nameof({member.PublicName}));");
                }
                sourceBuilder.AppendLine();

                // Value and Name properties
                sourceBuilder.AppendLine($"{innerIndent}/// <summary>Gets the underlying value that defines this instance.</summary>");
                sourceBuilder.AppendLine($"{innerIndent}public {underlyingValueTypeName} Value {{ get; }}");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine($"{innerIndent}/// <summary>Gets the name associated with this instance.</summary>");
                sourceBuilder.AppendLine($"{innerIndent}public string Name {{ get; }}");
                sourceBuilder.AppendLine();

                // Private Constructor
                sourceBuilder.AppendLine($"{innerIndent}// Private constructor used by static initializers");
                sourceBuilder.AppendLine($"{innerIndent}private {className}({underlyingValueTypeName} value, string name)");
                sourceBuilder.AppendLine($"{innerIndent}{{");
                sourceBuilder.AppendLine($"{innerIndent}    this.Value = value;");
                sourceBuilder.AppendLine($"{innerIndent}    this.Name = name ?? throw new ArgumentNullException(nameof(name));");
                sourceBuilder.AppendLine($"{innerIndent}    _list.Add(this); // Add instance to the static list");
                sourceBuilder.AppendLine($"{innerIndent}}}");
                sourceBuilder.AppendLine();

                // Static List(), FromValue(), FromName() methods (needed as no base class)
                // (Code is identical to previous generator's generated code for these)
                sourceBuilder.AppendLine($"{innerIndent}// Static utility methods");
                sourceBuilder.AppendLine($"{innerIndent}public static System.Collections.Generic.IReadOnlyCollection<{className}> List() => _list.AsReadOnly();");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine($"{innerIndent}public static {className} FromValue({underlyingValueTypeName} value)");
                sourceBuilder.AppendLine($"{innerIndent}{{");
                sourceBuilder.AppendLine($"{innerIndent}    var matchingItem = _list.FirstOrDefault(item => EqualityComparer<{underlyingValueTypeName}>.Default.Equals(item.Value, value));");
                sourceBuilder.AppendLine($"{innerIndent}    if (matchingItem == null) throw new ArgumentException($\"'{{value}}' is not a valid value for {className}.\", nameof(value));");
                sourceBuilder.AppendLine($"{innerIndent}    return matchingItem;");
                sourceBuilder.AppendLine($"{innerIndent}}}");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine($"{innerIndent}public static {className} FromName(string name, bool ignoreCase = false)");
                sourceBuilder.AppendLine($"{innerIndent}{{");
                sourceBuilder.AppendLine($"{innerIndent}    var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;");
                sourceBuilder.AppendLine($"{innerIndent}    var matchingItem = _list.FirstOrDefault(item => string.Equals(item.Name, name, comparison));");
                sourceBuilder.AppendLine($"{innerIndent}    if (matchingItem == null) throw new ArgumentException($\"'{{name}}' is not a valid name for {className}.\", nameof(name));");
                sourceBuilder.AppendLine($"{innerIndent}    return matchingItem;");
                sourceBuilder.AppendLine($"{innerIndent}}}");
                sourceBuilder.AppendLine();

                // IEquatable<T>, Equals, GetHashCode, Operators, ToString (identical to previous generator)
                sourceBuilder.AppendLine($"{innerIndent}// Equality implementations");
                sourceBuilder.AppendLine($"{innerIndent}public bool Equals({className}? other) {{ if (other is null) return false; if (Object.ReferenceEquals(this, other)) return true; return EqualityComparer<{underlyingValueTypeName}>.Default.Equals(this.Value, other.Value); }}");
                sourceBuilder.AppendLine($"{innerIndent}public override bool Equals(object? obj) {{ if (obj == null || GetType() != obj.GetType()) return false; return this.Equals(({className})obj); }}");
                sourceBuilder.AppendLine($"{innerIndent}public override int GetHashCode() => EqualityComparer<{underlyingValueTypeName}>.Default.GetHashCode(this.Value);");
                sourceBuilder.AppendLine($"{innerIndent}public static bool operator ==({className}? left, {className}? right) {{ if (left is null) return right is null; return left.Equals(right); }}");
                sourceBuilder.AppendLine($"{innerIndent}public static bool operator !=({className}? left, {className}? right) => !(left == right);");
                sourceBuilder.AppendLine($"{innerIndent}public override string ToString() => this.Name;");

                sourceBuilder.AppendLine($"{indent}}}"); // End partial class

                if (!string.IsNullOrEmpty(classNamespace))
                {
                    sourceBuilder.AppendLine("}"); // End namespace
                }

                context.AddSource($"{className}.EnumLike.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        // --- Helper Methods ---
        private static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        // Helper record
        private record EnumMemberInfo(string PublicName, string ValueLiteral);

        // Source for IsExternalInit
        private const string IsExternalInitSource = @"
// <auto-generated/>
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}";
    }
}
