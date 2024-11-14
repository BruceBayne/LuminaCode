using System;
using System.ComponentModel.Composition;
using AiReview.CodeLens.Vsix.CodeLens.Provider;
using AiReview.Core;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
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

	public void Dispose()
	{
		if (workspace != null)
		{
			workspace.WorkspaceChanged -= OnWorkspaceChanged;
		}
	}
}