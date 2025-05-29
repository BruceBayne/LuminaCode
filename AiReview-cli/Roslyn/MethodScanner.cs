using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LuminaCode_cli.Roslyn;

public sealed class MethodScanner
{
    public static IEnumerable<MethodData> Scan(string folderPath)
    {
        // Get all .cs files in the specified folder
        var csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

        foreach (var filePath in csFiles)
        {
            // Read the entire file content
            var sourceCode = File.ReadAllText(filePath);

            // Parse the source code using Roslyn
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = syntaxTree.GetCompilationUnitRoot();

            // Find all class declarations
            var classDeclarations = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                // Get the class name
                var className = classDeclaration.Identifier.Text;

                // Get full class text
                var fullClassText = classDeclaration.ToString().Trim();

                // Find all method declarations within the class
                var methodDeclarations = classDeclaration.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>();


                yield return new MethodData()
                {
                    ClassName = className.Trim(),
                    FullClassText = fullClassText.Trim(),
                    SourceFilePath = filePath.Trim(),
                    ClassDeclaration = classDeclaration,
                };

                foreach (var method in methodDeclarations)
                {
                    // Get full method text
                    var fullMethodText = method.ToString().Trim();

                    yield return new MethodData
                    {
                        ClassName = className.Trim(),
                        MethodName = method.Identifier.Text.Trim(),
                        ReturnType = method.ReturnType.ToString().Trim(),
                        Modifiers = string.Join(" ", method.Modifiers.Select(m => m.Text)),
                        FullMethodSignature = method.ToString().Trim(),
                        SourceFilePath = filePath,
                        FullMethodText = fullMethodText.Trim(),
                        FullClassText = fullClassText.Trim(),
                        MethodDeclaration = method
                    };
                }
            }
        }
    }
}