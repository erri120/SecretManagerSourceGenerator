# SecretManagerSourceGenerator

## What

This is a Source Generator which generates a [`SecretManager`](SecretManagerSourceGenerator.Example/Generated/SecretManagerSourceGenerator/SecretManagerSourceGenerator.Generator/SecretManager.generated.cs) class that holds secrets:

```c#
namespace SecretManager
{
    public static class SecretManager
    {
        public static string GetApikey() => "MyApiKey";
    }
}
```

(Generated output, see [`SecretManager.generated.cs`](SecretManagerSourceGenerator.Example/Generated/SecretManagerSourceGenerator/SecretManagerSourceGenerator.Generator/SecretManager.generated.cs))

## Why

Firstly I wanted to learn how to create a Source Generator but secondly I hat a problem of dealing with Application API Keys in Libraries.

Let's assume you are using some kind of API, how about one of the Google APIs. When dealing with Google APIs you can either use OAuth2 or Application API Keys. The problem with OAuth2 is that you are building some kind of Library or maybe a Console Application and you can't really implement OAuth2 correctly or you don't benefits that the User Authentication brings (access to private data). So you basically only need public data and that can be accessed via an Application API Key.

> Note that this is about obfuscating not hiding your secrets. In my case I restricted the Key I generated to only one Google API so that it can't do anything else. My only real concern was rate limiting and misuse of the Key.

Now how do you store this key? Initially I was like

```c#
private const string APIKey = "Something";
```

but this is a very bad idea because you should not be leaving your secrets in your code. GitHub will actually inform you that this is bad if you commit this. The problem is that others can see and then use this key by simply scrapping GitHub repos for secrets.

The Application API Key has to somehow get into your code without being committed. This is where Source Generators come into play. These puppies run at build time and can simply generate a new class that contains the Key.

## How

Add the Source Generator Project (`SecretManagerSourceGenerator.csproj`) as a Reference to your project and configure your `.csproj` file correctly:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <!-- Important to set the OutputItemType to "Analyzer" -->
        <ProjectReference Include="..\SecretManagerSourceGenerator\SecretManagerSourceGenerator.csproj" OutputItemType="Analyzer" />
    </ItemGroup>

    <PropertyGroup>
        <!-- only if you want to see the generated output, eg for testing -->
        <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
        <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
    </PropertyGroup>

    <ItemGroup>
        <!-- Don't include the output from a previous source generator execution into future runs; the */** trick here ensures that there's
        at least one subdirectory, which is our key that it's coming from a source generator as opposed to something that is coming from
        some other tool. -->
        <Compile Remove="$(CompilerGeneratedFilesOutputPath)/*/**/*.cs" />
    </ItemGroup>

</Project>

```

### With Environment Variables (Recommended)

The Generator will scan all Environment Variables **in the Process Scope** and look for Variables that start with `SECRET_MANAGER_SOURCE_GEN_`.

The Source Generator will then remove the prefix from the Variable and create a new Function called `Get{Key}() => "{Value}"`:

`SECRET_MANAGER_SOURCE_GEN_MyAPIKey=Something` will result in:

```c#
public static string GetMyAPIKey() => "Something";
```

Using Environment Variables is recommended because you can easily pass your GitHub Secrets to your GitHub Actions build workflow ([Docs](https://docs.github.com/en/actions/reference/encrypted-secrets#using-encrypted-secrets-in-a-workflow)) or configure your IDE to use certain Environment Variables while running/building/testing your application.

### With a Secret file

This method uses the `AdditionalFiles` property that a Source Generator has access to. Create a new file called `Secret.txt` and put 1 Key per line in this Format into the file:

```txt
MyAPIKey=Something
```

Next add the file as an Additional File in your `.csproj` file:

```xml
<ItemGroup>
    <AdditionalFiles Include="Secret.txt" />
</ItemGroup>
```

The Generator will then generate the following Function:

```c#
public static string GetMyAPIKey() => "Something";
```
