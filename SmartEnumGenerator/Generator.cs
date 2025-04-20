using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SmartEnumGenerator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        // Define the fully qualified names of the attributes
        private const string GenerateSmartEnumAttributeName = "SmartEnumGeneratorAttributes.GenerateSmartEnumAttribute";
        private const string SmartEnumPropsAttributeName = "SmartEnumGeneratorAttributes.SmartEnumPropsAttribute";
        private const string BaseSmartEnumNamespace = "SmartEnumBase"; // Namespace of the base class

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // --- Step 1: Filter Syntax Nodes ---
            // Find all enum declarations that might have the [GenerateSmartEnum] attribute
            IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // Quick filter for enums
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // Get enum if attribute matches
                .Where(static m => m is not null)!; // Filter out nulls

            // --- Step 2: Combine with Compilation ---
            // Get the semantic model for the filtered enums and combine with attribute data
            IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(enumDeclarations.Collect());

            // --- Step 3: Generate Code ---
            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        // Quick syntax check: is it an enum declaration?
        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is EnumDeclarationSyntax eds && eds.AttributeLists.Count > 0;

        // Semantic check: does the enum actually have the [GenerateSmartEnum] attribute?
        private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

            // Loop through all attributes on the enum
            foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol attributeSymbol)
                    {
                        INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                        string fullName = attributeContainingTypeSymbol.ToDisplayString();

                        // Is the attribute the [GenerateSmartEnum] attribute?
                        if (fullName == GenerateSmartEnumAttributeName)
                        {
                            return enumDeclarationSyntax;
                        }
                    }
                }
            }
            // No matching attribute found
            return null;
        }

        // --- Main Generation Logic ---
        private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
        {
            if (enums.IsDefaultOrEmpty) return; // Nothing to do

            // Get distinct enums (might be multiple partial definitions) - not applicable here as we target enums
            IEnumerable<EnumDeclarationSyntax> distinctEnums = enums.Distinct();

            // Process each enum found
            foreach (var enumSyntax in distinctEnums)
            {
                context.CancellationToken.ThrowIfCancellationRequested(); // Check for cancellation

                SemanticModel semanticModel = compilation.GetSemanticModel(enumSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(enumSyntax) is not INamedTypeSymbol enumSymbol) continue; // Should not happen

                // --- Extract Data ---
                string enumName = enumSymbol.Name;
                string enumNamespace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToDisplayString();
                string underlyingTypeName = enumSymbol.EnumUnderlyingType?.ToDisplayString() ?? "int"; // Default to int

                // Get attribute arguments
                var generateAttribute = enumSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == GenerateSmartEnumAttributeName);
                string valuePropertyName = "Value"; // Default
                string? generatedClassName = null;
                string? generatedNamespace = null;

                if (generateAttribute != null)
                {
                    foreach (var namedArg in generateAttribute.NamedArguments)
                    {
                        if (namedArg.Key == "ValuePropertyName" && namedArg.Value.Value is string vpn) valuePropertyName = vpn;
                        if (namedArg.Key == "GeneratedClassName" && namedArg.Value.Value is string gcn) generatedClassName = gcn;
                        if (namedArg.Key == "GeneratedNamespace" && namedArg.Value.Value is string gns) generatedNamespace = gns;
                    }
                }

                // Determine final class name and namespace
                string finalClassName = generatedClassName ?? SanitizeClassName(enumName);
                string finalNamespace = generatedNamespace ?? enumNamespace;

                // Extract members and their properties from [SmartEnumProps]
                var membersData = new List<SmartEnumMemberInfo>();
                var constructorParamNames = new List<string>(); // To store names of extra props for the constructor signature
                var constructorParamTypes = new List<string>(); // To store types of extra props

                bool firstMember = true;
                foreach (var memberSyntax in enumSyntax.Members)
                {
                    if (semanticModel.GetDeclaredSymbol(memberSyntax) is not IFieldSymbol memberSymbol) continue;

                    string memberName = memberSymbol.Name;
                    object memberValue = memberSymbol.ConstantValue!; // Underlying value (e.g., 1, 2)
                    string memberValueLiteral = GetValueLiteral(memberValue, underlyingTypeName); // Format value for code gen

                    // Find the [SmartEnumProps] attribute
                    var propsAttribute = memberSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == SmartEnumPropsAttributeName);
                    var propValues = new List<string>(); // Formatted values for constructor call

                    if (propsAttribute != null && propsAttribute.ConstructorArguments.Length > 0)
                    {
                        // Assumes the first constructor argument is the object[] propertyValues
                        var valuesArray = propsAttribute.ConstructorArguments[0];
                        if (valuesArray.Kind == TypedConstantKind.Array)
                        {
                            int index = 0;
                            foreach (var typedConstant in valuesArray.Values)
                            {
                                // On the first member, determine constructor param names/types from the attribute values
                                if (firstMember)
                                {
                                    // VERY basic type inference - needs improvement for production
                                    string paramType = typedConstant.Type?.ToDisplayString() ?? "object";
                                    // Infer name - THIS IS A HUGE GUESS/ASSUMPTION
                                    // A better approach would be required for production (e.g., named args in attribute, explicit config)
                                    string paramName = $"prop{index + 1}"; // Highly unreliable naming
                                    constructorParamNames.Add(paramName);
                                    constructorParamTypes.Add(paramType);
                                }
                                // Format the value for code generation (string literal, number literal, etc.)
                                propValues.Add(GetConstantValueLiteral(typedConstant));
                                index++;
                            }
                        }
                    }
                    membersData.Add(new SmartEnumMemberInfo(memberName, memberValueLiteral, propValues));
                    firstMember = false; // Constructor signature determined after first member
                }


                // --- Generate Code ---
                var sourceBuilder = new StringBuilder();

                // Header
                sourceBuilder.AppendLine("// <auto-generated/>");
                sourceBuilder.AppendLine("#nullable enable");
                sourceBuilder.AppendLine($"using System;");
                sourceBuilder.AppendLine($"using {BaseSmartEnumNamespace}; // Assuming base class namespace");
                sourceBuilder.AppendLine();
                if (!string.IsNullOrEmpty(finalNamespace))
                {
                    sourceBuilder.AppendLine($"namespace {finalNamespace}");
                    sourceBuilder.AppendLine("{");
                }

                // Class definition
                sourceBuilder.AppendLine($"    /// <summary>");
                sourceBuilder.AppendLine($"    /// Generated Smart Enum for {enumName}.");
                sourceBuilder.AppendLine($"    /// </summary>");
                sourceBuilder.AppendLine($"    public sealed partial class {finalClassName} : SmartEnum<{finalClassName}, {underlyingTypeName}>");
                sourceBuilder.AppendLine("    {");

                // Static readonly instances
                foreach (var member in membersData)
                {
                    // Construct arguments for the constructor call
                    var constructorArgs = new List<string> { member.ValueLiteral, $"nameof({member.Name})" }; // Standard value, name
                    constructorArgs.AddRange(member.PropertyValues); // Add values from [SmartEnumProps]

                    sourceBuilder.AppendLine($"        public static readonly {finalClassName} {member.Name} = new {finalClassName}({string.Join(", ", constructorArgs)});");
                }
                sourceBuilder.AppendLine();

                // Properties from [SmartEnumProps]
                for(int i = 0; i < constructorParamNames.Count; i++)
                {
                    sourceBuilder.AppendLine($"        public {constructorParamTypes[i]} {Capitalize(constructorParamNames[i])} {{ get; }}");
                }
                 // Value property alias (e.g., Rank)
                 if (valuePropertyName != "Value") // Avoid duplicate if base already has "Value"
                 {
                      sourceBuilder.AppendLine($"        public {underlyingTypeName} {valuePropertyName} => Value;");
                 }
                 sourceBuilder.AppendLine();


                // Private Constructor
                sourceBuilder.Append($"        private {finalClassName}({underlyingTypeName} value, string name");
                // Add parameters for extra properties
                for (int i = 0; i < constructorParamNames.Count; i++)
                {
                    sourceBuilder.Append($", {constructorParamTypes[i]} {constructorParamNames[i]}");
                }
                sourceBuilder.AppendLine(")");
                sourceBuilder.AppendLine($"            : base(value, name)"); // Call base constructor
                sourceBuilder.AppendLine("        {");
                 // Assign properties from constructor parameters
                 for (int i = 0; i < constructorParamNames.Count; i++)
                 {
                     sourceBuilder.AppendLine($"            this.{Capitalize(constructorParamNames[i])} = {constructorParamNames[i]};");
                 }
                sourceBuilder.AppendLine("        }"); // End constructor

                sourceBuilder.AppendLine("    }"); // End class

                if (!string.IsNullOrEmpty(finalNamespace))
                {
                    sourceBuilder.AppendLine("}"); // End namespace
                }

                // Add the generated source file to the compilation
                context.AddSource($"{finalClassName}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        // --- Helper Methods ---

        // Helper to sanitize enum name to be a valid class name
        private static string SanitizeClassName(string enumName)
        {
            if (enumName.EndsWith("Enum", StringComparison.OrdinalIgnoreCase))
                return enumName.Substring(0, enumName.Length - 4);
            if (enumName.EndsWith("Definition", StringComparison.OrdinalIgnoreCase))
                return enumName.Substring(0, enumName.Length - 10);
            return enumName;
        }

         // Helper to format the underlying value as a C# literal
         private static string GetValueLiteral(object value, string typeName)
         {
             if (typeName == "string") return $"\"{value}\""; // Basic string literal
             // Add handling for other types if needed (char, etc.)
             return value.ToString() ?? "default"; // Default for numbers/other structs
         }

         // Helper to format TypedConstant value as C# literal - NEEDS MORE ROBUSTNESS
         private static string GetConstantValueLiteral(TypedConstant constant)
         {
             if (constant.Kind == TypedConstantKind.Error) return "default"; // Or throw?
             if (constant.Value == null) return "null";
             if (constant.Type?.SpecialType == SpecialType.System_String) return $"\"{constant.Value}\"";
             if (constant.Type?.SpecialType == SpecialType.System_Boolean) return constant.Value.ToString()?.ToLowerInvariant() ?? "default"; // true/false
             // Add more types as needed (char, other numerics)
             return constant.Value.ToString() ?? "default";
         }

         // Simple capitalization helper
         private static string Capitalize(string s)
         {
             if (string.IsNullOrEmpty(s)) return s;
             return char.ToUpperInvariant(s[0]) + s.Substring(1);
         }


        // Helper record to store extracted member info
        private record SmartEnumMemberInfo(string Name, string ValueLiteral, List<string> PropertyValues);
    }
}
