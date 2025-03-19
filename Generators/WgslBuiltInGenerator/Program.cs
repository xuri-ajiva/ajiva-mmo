using WgslBuiltInGenerator;

var bsDocumentPath = "index.bs"; // Path to your index.bs file
if (File.Exists(bsDocumentPath))
{
    var bsContent = File.ReadAllText(bsDocumentPath);
    var csharpCode = BikeshedParser.ParseBikeshedDocumentToCSharp(bsContent);
    File.WriteAllText("BuiltIn.g.cs", csharpCode);
    //Console.WriteLine(csharpCode);
    Console.WriteLine("C# code generated to BuiltIn.g.cs");
}
else
{
    Console.WriteLine($"Error: Bikeshed document not found at {bsDocumentPath}");
}
