using System;

namespace SecretManagerSourceGenerator.Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(SecretManager.SecretManager.GetApikey());
        }
    }
}
