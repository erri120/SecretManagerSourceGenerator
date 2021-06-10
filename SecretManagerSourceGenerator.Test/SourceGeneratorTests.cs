using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using VerifyCS = SecretManagerSourceGenerator.Test.CSharpSourceGeneratorVerifier<SecretManagerSourceGenerator.Generator>;

namespace SecretManagerSourceGenerator.Test
{
    public class SourceGeneratorTests
    {
        [Fact]
        public async Task TestSourceGeneratorWithEnvironmentVariables()
        {
            const string generated = @"
namespace SecretManager
{
    public static class SecretManager
    {

        public static string GetArchiveKey() => ""MyArchiveKey"";
    }
}
";
            
            Environment.SetEnvironmentVariable($"{Generator.EnvStart}ArchiveKey", "MyArchiveKey", EnvironmentVariableTarget.Process);
            
            await new VerifyCS.Test
            {
                TestState =
                {
                    GeneratedSources =
                    {
                        (typeof(Generator), "SecretManager.generated.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256))
                    }
                }
            }.RunAsync();
        }
        
        [Fact]
        public async Task TestSourceGeneratorWithSecretFile()
        {
            // making sure to delete this Environment Variable so the Generator does not pick it up
            Environment.SetEnvironmentVariable($"{Generator.EnvStart}ArchiveKey", null, EnvironmentVariableTarget.Process);
            
            const string generated = @"
namespace SecretManager
{
    public static class SecretManager
    {

        public static string GetApiKey() => ""MyApiKey"";
    }
}
";

            await new VerifyCS.Test
            {
                TestState =
                {
                    GeneratedSources =
                    {
                        (typeof(Generator), "SecretManager.generated.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256))
                    },
                    AdditionalFiles =
                    {
                        ("Secret.txt", "ApiKey=MyApiKey")
                    }
                }
            }.RunAsync(); 
        }

    }
}
