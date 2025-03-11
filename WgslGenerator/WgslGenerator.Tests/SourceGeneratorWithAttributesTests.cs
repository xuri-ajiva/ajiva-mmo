using System.IO;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace WgslGenerator.Tests;

public class SourceGeneratorWithAttributesTests
{
    [Fact]
    public void GenerateReportMethod()
    {
        // Create an instance of the source generator.
        var generator = new SourceGeneratorWithAttributes();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(nameof(SourceGeneratorWithAttributesTests),
            new[]
            {
                CSharpSyntaxTree.ParseText(
                    File.ReadAllText("I:\\ajiva-mmo\\WgslGenerator\\WgslGenerator.Sample\\DeferredShader.cs"))
            },
            new[]
            {
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

        // Run generators and retrieve all results.
        driver.RunGenerators(compilation).GetRunResult();
    }
}
