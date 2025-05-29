using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;

namespace AiReview.CodeLens.Vsix.CodeLens.VisualStudio;

public static class VsExtensions
{
    public static string GetSourceCodeFromWorkspace(this VisualStudioWorkspace workspace, string filePath, int start,
        int end)
    {
        // Step 1: Get the document from the workspace using the file path.
        var document = GetDocument(workspace, filePath);

        if (document == null)
        {
            throw new FileNotFoundException($"Document not found in workspace: {filePath}");
        }

        // Step 2: Get the source text from the document.
        if (document.TryGetText(out var sourceText))
        {
            // Step 3: Extract the substring from the specified positions.
            if (start < 0 || end > sourceText.Length || start > end)
            {
                throw new ArgumentOutOfRangeException("Invalid start or end positions.");
            }

            var textSpan = TextSpan.FromBounds(start, end);
            return sourceText.ToString(textSpan);
        }

        return "";
    }

    public static Document GetDocument(VisualStudioWorkspace workspace, string filePath)
    {
        var document = workspace.CurrentSolution
            .Projects
            .SelectMany(project => project.Documents)
            .FirstOrDefault(doc => doc.FilePath == filePath);
        return document;
    }

    public static async Task<ISymbol> GetSymbolAt(this Document doc, TextSpan span, CancellationToken ct)
    {
        var rootNode = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        var syntaxNode = rootNode.FindNode(span);

        var semanticModel = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false)
                            ?? throw new InvalidOperationException(
                                $"Document {doc.Name} does not have a semantic model.");

        return semanticModel.GetDeclaredSymbol(syntaxNode, ct)
               ?? throw new InvalidOperationException($"Node is not a symbol declaration: {syntaxNode}.");
    }

    private static readonly FieldInfo projectToGuidMapField = typeof(VisualStudioWorkspace).Assembly
        .GetType(
            "Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem.VisualStudioWorkspaceImpl",
            throwOnError: true)
        .GetField("_projectToGuidMap", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly MethodInfo getDocumentIdInCurrentContextMethod = typeof(Workspace).GetMethod(
        "GetDocumentIdInCurrentContext",
        BindingFlags.NonPublic | BindingFlags.Instance,
        binder: null,
        types: new[] { typeof(DocumentId) },
        modifiers: null);

    public static Document GetDocument(this VisualStudioWorkspace workspace, string filePath, Guid projGuid)
    {
        var projectToGuidMap = (ImmutableDictionary<ProjectId, Guid>)projectToGuidMapField.GetValue(workspace);
        var sln = workspace.CurrentSolution;

        var candidateId = sln
                              .GetDocumentIdsWithFilePath(filePath)
                              // VS will create multiple `ProjectId`s for projects with multiple target frameworks.
                              // We simply take the first one we find.
                              .FirstOrDefault(candidateId =>
                                  projectToGuidMap.GetValueOrDefault(candidateId.ProjectId) == projGuid)
                          ?? throw new InvalidOperationException(
                              $"File {filePath} (project: {projGuid}) not found in solution {sln.FilePath}.");

        var currentContextId = workspace.GetDocumentIdInCurrentContext(candidateId);
        return sln.GetDocument(currentContextId)
               ?? throw new InvalidOperationException(
                   $"Document {currentContextId} not found in solution {sln.FilePath}.");
    }

    public static DocumentId? GetDocumentIdInCurrentContext(this Workspace workspace, DocumentId? documentId)
        => (DocumentId?)getDocumentIdInCurrentContextMethod.Invoke(workspace, new[] { documentId });
}