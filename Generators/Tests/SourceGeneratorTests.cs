using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace WgslGenerator.Tests;

public class SourceGeneratorWithAttributesTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SourceGeneratorWithAttributesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GenerateReportMethod()
    {
        // Create an instance of the source generator.
        var generator = new WgslShaderGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        var basePath = "../../../../../Client/Wgsl/";
        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(nameof(SourceGeneratorWithAttributesTests),
            [
                CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/Compositor.wgsl.cs")),
                //CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/TempleShader.wgsl.cs")),
                //CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/PbrDeferredLightingShader.wgsl.cs")),
                //CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/ObjectSamplingShader.wgsl.cs")),
            ],
            [
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            ]);
        var start = Stopwatch.GetTimestamp();

        // Run generators and retrieve all results.
        GeneratorDriverRunResult result = driver.RunGenerators(compilation).GetRunResult();

        var end = Stopwatch.GetTimestamp();
        _testOutputHelper.WriteLine($"Generator: {TimeSpan.FromTicks(end - start)}");

        List<SyntaxTree> syntaxTrees =
        [
            CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/DeferredShader.wgsl.cs")),
            CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/../Wgsl/BuiltIn.cs")),
            CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/../Wgsl/BuiltIn.g.cs")),
            CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/../Wgsl/IWgslShader.cs")),
            CSharpSyntaxTree.ParseText(File.ReadAllText($"{basePath}/../GlobalUsings.cs")),
        ];
        syntaxTrees.AddRange(result.GeneratedTrees);

        //get all MetadataReference for current process
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();


        List<MetadataReference> metadataReferences =
        [
            compilation.ToMetadataReference(),
            MetadataReference.CreateFromFile(typeof(ValueTuple).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GlmSharp.glm).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Numerics.Vector3).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.WebGPU.Adapter).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.WebGPU.Safe.AdapterInfo).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.Core.RawImage).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DynamicAttribute).Assembly
                .Location),
        ];

        metadataReferences.AddRange(references);

        var next = CSharpCompilation.Create(nameof(SourceGeneratorWithAttributesTests),
            syntaxTrees, metadataReferences, new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary, true, allowUnsafe: true));


        MemoryStream str = new();
        start = Stopwatch.GetTimestamp();
        var emit = next.Emit(str);
        end = Stopwatch.GetTimestamp();
        _testOutputHelper.WriteLine($"Emit: {TimeSpan.FromTicks(end - start)}");

        if (emit.Success)
        {
            start = Stopwatch.GetTimestamp();
            // load assembly
            var assembly = System.Reflection.Assembly.Load(str.ToArray());
            // get type
            var type = assembly.GetType("Client.Renderer.DeferredShader");
            var shaderSourceFiled = type?.GetField("ShaderSource");
            var shaderCode = shaderSourceFiled?.GetValue(null);
            end = Stopwatch.GetTimestamp();
            _testOutputHelper.WriteLine($"Assembly: {TimeSpan.FromTicks(end - start)}");
            Assert.NotNull(shaderCode);
        }
        else
        {
            Assert.True(false, "Emit failed");
        }
    }
}
