using System.ComponentModel;

// Define this type within the System.Runtime.CompilerServices namespace
// Make it internal to avoid conflicts if this generator is consumed by a project
// that already has this type defined (e.g., targeting .NET 5+).
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// This dummy class is required to compile records or use init setters
    /// when targeting older frameworks like .NET Standard 2.0.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
        // No members needed - its presence is sufficient.
    }
}