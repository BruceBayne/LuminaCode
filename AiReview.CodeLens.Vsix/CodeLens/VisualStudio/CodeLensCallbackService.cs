using System;
using System.ComponentModel.Composition;
using System.IO;
using AiReview.Core;
using AiReview.Core.LLM;
using AiReview.Core.LLM.Review;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.VisualStudio;

[Export(typeof(ICodeLensCallbackListener))]
[ContentType("CSharp")]
internal class CodeLensCallbackService : ICodeLensCallbackListener, IAiReviewService, IDisposable
{
    public readonly VisualStudioWorkspace workspace;
    public readonly ITextDocumentFactoryService factoryService;


    [ImportingConstructor]
    public CodeLensCallbackService(VisualStudioWorkspace workspace, ITextDocumentFactoryService factoryService)
    {
        workspace.WorkspaceChanged += OnWorkspaceChanged;
        this.workspace = workspace;
        this.factoryService = factoryService;
    }

    private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
    {
        if (e.Kind == WorkspaceChangeKind.DocumentChanged)
        {
            var changedDocument = workspace.CurrentSolution.GetDocument(e.DocumentId);
            if (changedDocument != null)
            {
                // Trigger a refresh of your CodeLens data point
            }
        }
    }


    public string ExtractSourceCode(string filePath, int from, int to)
    {
        //var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();
        var sourceCode = workspace.GetSourceCodeFromWorkspace(filePath, from, to);
        return sourceCode;
    }

    public LuminaCodeProjectOptions GetProjectOptions(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return LuminaCodeProjectOptions.Default;
        }

        var document = VsExtensions.GetDocument(workspace, filePath);
        if (document.Project.FilePath is not { Length: > 0 })
            return LuminaCodeProjectOptions.Default;

        return TryLoadOptions(document.Project.FilePath)
               ?? TryLoadOptions(workspace.CurrentSolution.FilePath)
               ?? LuminaCodeProjectOptions.Default;

        LuminaCodeProjectOptions? TryLoadOptions(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
                return null;

            var configPath = Path.Combine(dir, LuminaCodeProjectOptions.OptionsFileName);
            return LuminaCodeProjectOptions.TryLoad(configPath, out var options) ? options : null;
        }
    }


    public void Dispose()
    {
        if (workspace != null)
        {
            workspace.WorkspaceChanged -= OnWorkspaceChanged;
        }
    }
}