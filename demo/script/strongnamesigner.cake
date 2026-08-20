#reference "../../BuildArtifacts/temp/_PublishedLibraries/Cake.StrongNameSigner/netstandard2.0/Cake.StrongNameSigner.dll"

using Cake.StrongNameSigner;

// Self-contained exercise of Cake.StrongNameSigner's alias + settings
// surface. The actual StrongNameSigner alias shells out to a third-party
// tool (StrongNameSigner.Console.exe from Brutal.Dev.StrongNameSigner),
// which CI doesn't install via this script — so this script verifies the
// addin loads, the StrongNameSignerSettings type can be constructed and
// round-tripped, and the alias is callable (it'll throw at tool-resolution
// time, which we catch).

void AssertThat(bool condition, string message)
{
    if (!condition)
    {
        throw new Exception("Assertion failed: " + message);
    }
}

Task("Default")
    .IsDependentOn("Settings-Roundtrip")
    .IsDependentOn("Alias-ResolvesToToolError");

Task("Settings-Roundtrip")
    .Does(() =>
{
    var settings = new StrongNameSignerSettings
    {
        AssemblyFile = File("./bin/Release/Sample.dll"),
        KeyFile = File("./key.snk"),
        Password = "fake-password",
        InputDirectory = "./bin/Release",
        OutputDirectory = Directory("./bin/Release/Signed"),
        LogLevel = StrongNameSignerVerbosity.Verbose,
    };

    AssertThat(settings.AssemblyFile != null && settings.AssemblyFile.FullPath.EndsWith("Sample.dll"), "AssemblyFile roundtrip");
    AssertThat(settings.KeyFile != null && settings.KeyFile.FullPath.EndsWith("key.snk"), "KeyFile roundtrip");
    AssertThat(settings.Password == "fake-password", "Password roundtrip");
    AssertThat(settings.InputDirectory == "./bin/Release", "InputDirectory roundtrip");
    AssertThat(settings.OutputDirectory != null && settings.OutputDirectory.FullPath.EndsWith("Signed"), "OutputDirectory roundtrip");
    AssertThat(settings.LogLevel == StrongNameSignerVerbosity.Verbose, "LogLevel roundtrip");

    Information("StrongNameSignerSettings OK (all 6 properties round-tripped)");

    // Also smoke-test every StrongNameSignerVerbosity enum value can be assigned.
    foreach (StrongNameSignerVerbosity verbosity in System.Enum.GetValues(typeof(StrongNameSignerVerbosity)))
    {
        settings.LogLevel = verbosity;
        AssertThat(settings.LogLevel == verbosity, "Verbosity " + verbosity + " roundtrip");
    }

    Information("StrongNameSignerVerbosity OK (all 5 enum values round-tripped)");
});

Task("Alias-ResolvesToToolError")
    .Does(() =>
{
    // Calling the alias should reach the tool resolution step and fail
    // there with "StrongNameSigner.Console.exe could not be found" — that
    // confirms the alias is wired correctly even though the licensed tool
    // isn't installed in CI.
    var settings = new StrongNameSignerSettings
    {
        AssemblyFile = File("./fake-input.dll"),
    };

    var threw = false;
    try
    {
        StrongNameSigner(settings);
    }
    catch (Exception ex) when (ex.Message.IndexOf("StrongNameSigner", StringComparison.OrdinalIgnoreCase) >= 0
                              || ex.Message.IndexOf("not be found", StringComparison.OrdinalIgnoreCase) >= 0
                              || ex.Message.IndexOf("could not locate", StringComparison.OrdinalIgnoreCase) >= 0)
    {
        threw = true;
        Information("Alias resolved correctly; tool-not-found exception was: {0}", ex.Message);
    }

    AssertThat(threw, "Expected StrongNameSigner alias to throw a tool-not-found exception (StrongNameSigner.Console.exe is a separate tool install and not present in CI)");
});

RunTarget("Default");
