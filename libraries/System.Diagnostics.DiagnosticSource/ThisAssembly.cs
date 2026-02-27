
namespace System.Diagnostics;

internal static class ThisAssembly
{
    private const string BuildAssemblyFileVersion = "1.0";

    public static Version AssemblyFileVersion { get; } = new(BuildAssemblyFileVersion);
}
