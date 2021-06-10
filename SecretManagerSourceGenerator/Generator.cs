using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SecretManagerSourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        private const string FileName = "SecretManager.generated.cs";
        public const string EnvStart = "SECRET_MANAGER_SOURCE_GEN_";

        private readonly Dictionary<string, string> _secrets = new Dictionary<string, string>();
        
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var variables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
            foreach (DictionaryEntry de in variables)
            {
                if (!(de.Key is string key)) continue;
                if (!(de.Value is string value)) continue;
                if (!key.StartsWith(EnvStart)) continue;

                key = key.Substring(EnvStart.Length);
                
                _secrets.Add(key, value);
            }
            
            var secretFile = context.AdditionalFiles.FirstOrDefault(x =>
                Path.GetFileName(x.Path).Equals("Secret.txt", StringComparison.OrdinalIgnoreCase));

            var content = secretFile?.GetText();
            if (content != null)
            {
                var lines = content.ToString().Split('\n');

                foreach (var line in lines)
                {
                    var splitIndex = line.IndexOf('=');
                    if (splitIndex == -1)
                        continue;

                    var key = line.Substring(0, splitIndex);
                    var value = line.Substring(splitIndex + 1, line.Length - splitIndex - 1).Trim();
                
                    _secrets.Add(key, value);
                }
            }

            var sb = new StringBuilder(@"
namespace SecretManager
{
    public static class SecretManager
    {
");
            foreach (var secret in _secrets)
            {
                var key = secret.Key;
                var value = secret.Value;
                
                var fb = new StringBuilder(@"
        public static string Get");
                fb.Append($"{key}()");
                fb.Append($" => \"{value}\";");

                sb.Append(fb);
            }
            
            sb.Append(@"
    }
}
");
            context.AddSource(FileName, SourceText.From(sb.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
    }
}
