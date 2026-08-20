using System;
using Cake.Common.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.StrongNameSigner;

namespace Build.Tasks
{
    [TaskName("Alias-ResolvesToToolError")]
    public sealed class AliasResolvesToToolErrorTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            // Calling the alias should reach the tool resolution step and fail
            // there with "StrongNameSigner.Console.exe could not be found" — that
            // confirms the alias is wired correctly even though the licensed tool
            // isn't installed in CI.
            var settings = new StrongNameSignerSettings
            {
                AssemblyFile = new FilePath("./fake-input.dll"),
            };

            var threw = false;
            try
            {
                context.StrongNameSigner(settings);
            }
            catch (Exception ex) when (ex.Message.IndexOf("StrongNameSigner", StringComparison.OrdinalIgnoreCase) >= 0
                                      || ex.Message.IndexOf("not be found", StringComparison.OrdinalIgnoreCase) >= 0
                                      || ex.Message.IndexOf("could not locate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                threw = true;
                context.Information("Alias resolved correctly; tool-not-found exception was: {0}", ex.Message);
            }

            AssertThat(threw, "Expected StrongNameSigner alias to throw a tool-not-found exception (StrongNameSigner.Console.exe is a separate tool install and not present in CI)");
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
