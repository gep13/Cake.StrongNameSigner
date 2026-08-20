using System;
using Cake.Common.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.StrongNameSigner;

namespace Build.Tasks
{
    [TaskName("Settings-Roundtrip")]
    public sealed class SettingsRoundtripTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var settings = new StrongNameSignerSettings
            {
                AssemblyFile = new FilePath("./bin/Release/Sample.dll"),
                KeyFile = new FilePath("./key.snk"),
                Password = "fake-password",
                InputDirectory = "./bin/Release",
                OutputDirectory = new DirectoryPath("./bin/Release/Signed"),
                LogLevel = StrongNameSignerVerbosity.Verbose,
            };

            AssertThat(settings.AssemblyFile != null && settings.AssemblyFile.FullPath.EndsWith("Sample.dll"), "AssemblyFile roundtrip");
            AssertThat(settings.KeyFile != null && settings.KeyFile.FullPath.EndsWith("key.snk"), "KeyFile roundtrip");
            AssertThat(settings.Password == "fake-password", "Password roundtrip");
            AssertThat(settings.InputDirectory == "./bin/Release", "InputDirectory roundtrip");
            AssertThat(settings.OutputDirectory != null && settings.OutputDirectory.FullPath.EndsWith("Signed"), "OutputDirectory roundtrip");
            AssertThat(settings.LogLevel == StrongNameSignerVerbosity.Verbose, "LogLevel roundtrip");

            context.Information("StrongNameSignerSettings OK (all 6 properties round-tripped)");

            foreach (StrongNameSignerVerbosity verbosity in Enum.GetValues(typeof(StrongNameSignerVerbosity)))
            {
                settings.LogLevel = verbosity;
                AssertThat(settings.LogLevel == verbosity, "Verbosity " + verbosity + " roundtrip");
            }

            context.Information("StrongNameSignerVerbosity OK (all 5 enum values round-tripped)");
        }

        private static void AssertThat(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion failed: " + message);
            }
        }
    }
}
