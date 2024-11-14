using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;

namespace AiReview.CodeLens.Vsix.CodeLens.VisualStudio;

[Export(typeof(ICommandHandler))]
[Name(nameof(SaveCommandHandler))]
[ContentType("CSharp")]
[ContentType("Basic")]
[TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
public class SaveCommandHandler : ICommandHandler<SaveCommandArgs>
{
	public string DisplayName => nameof(SaveCommandHandler);

	public bool ExecuteCommand(SaveCommandArgs args, CommandExecutionContext ctx)
	{
		try
		{



			{
				_ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate {
					// CodeLenses usually only live as long as the document is open so we just refresh all the connected ones.

					await Task.CompletedTask;
				});
			}

			return true;
		}
		catch (Exception ex)
		{
			
			throw;
		}
	}

	public CommandState GetCommandState(SaveCommandArgs args) => CommandState.Available;
}