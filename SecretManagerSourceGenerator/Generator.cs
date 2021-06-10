using System;
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
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var secretFile = context.AdditionalFiles.FirstOrDefault(x =>
                Path.GetFileName(x.Path).Equals("Secret.txt", StringComparison.OrdinalIgnoreCase));
            
            if (secretFile == null)
            {
                context.AddSource("SecretManagerGenerated", SourceText.From("", Encoding.UTF8));
                return;
            }

            var content = secretFile.GetText();
            if (content == null)
            {
                context.AddSource("SecretManagerGenerated", SourceText.From("", Encoding.UTF8));
                return;
            }

            var lines = content.ToString().Split('\n');
            var secrets = new Dictionary<string, string>(lines.Length);

            foreach (var line in lines)
            {
                var splitIndex = line.IndexOf('=');
                if (splitIndex == -1)
                    continue;

                var key = line.Substring(0, splitIndex);
                var value = line.Substring(splitIndex + 1, line.Length - splitIndex - 1).Trim();
                
                secrets.Add(key, value);
            }
            
            var sb = new StringBuilder(@"
namespace SecretManager
{
    public static class SecretManager
    {
");
            foreach (var secret in secrets)
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
            context.AddSource("SecretManager.generated.cs", SourceText.From(sb.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
    }
}
