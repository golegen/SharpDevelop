
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;

using ICSharpCode.SharpDevelop.Project;
using NSvn.Common;
using NSvn.Core;

namespace ICSharpCode.Svn.Commands
{
	public abstract class SubversionCommand : AbstractMenuCommand
	{
		protected string fileName;
		
		public override void Run()
		{
			AbstractProjectBrowserTreeNode node = ProjectBrowserPad.Instance.SelectedNode;
			if (node != null) {
				fileName = null;
				if (node is DirectoryNode) {
					fileName = ((DirectoryNode)node).Directory;
				} else if (node is FileNode) {
					fileName = ((FileNode)node).FileName;
				}
				if (fileName == null) {
					return;
				}
				if (StartOperation()) {
					SvnClient.Instance.WaitForOperationEnd();
					
					if (AddInOptions.AutomaticallyReloadProject) {
						//projectService.ReloadCombine();
					}
				}
			}
		}
		
		protected abstract bool StartOperation();
	}
	
	public class UpdateCommand : SubversionCommand
	{
		void DoUpdateCommand()
		{
			SvnClient.Instance.Client.Update(Directory.Exists(fileName) ? fileName : Path.GetDirectoryName(fileName), Revision.Head, true);
		}
		
		protected override bool StartOperation()
		{
			SvnClient.Instance.OperationStart("Update", new ThreadStart(DoUpdateCommand));
			return true;
		}
	}
	
	public class RevertCommand : SubversionCommand
	{
		void DoRevertCommand()
		{
			SvnClient.Instance.Client.Revert(new string[] { fileName }, true);
		}
		
		protected override bool StartOperation()
		{
			// TODO: Display warning message (Revert might cause loss of data)
			SvnClient.Instance.OperationStart("Revert", new ThreadStart(DoRevertCommand));
			return true;
		}
	}
	
	/// <summary>
	/// Description of CreatePatchCommand
	/// </summary>
	public class CreatePatchCommand : SubversionCommand
	{
		string output;
		
		void DoCreatePatchCommand()
		{
			try {
				MemoryStream outStream = new MemoryStream();
				MemoryStream errStream = new MemoryStream();
				
				SvnClient.Instance.Client.Diff(new string [] {} ,
				                               fileName,
				                               Revision.Committed,
				                               fileName,
				                               Revision.Working,
				                               true,
				                               false,
				                               true,
				                               outStream,
				                               errStream);
				output = Encoding.Default.GetString(outStream.ToArray());
				// TODO: Invoke the following method:
				// FileService.NewFile("a.patch", "txt", output);
			} catch (Exception e) {
				MessageService.ShowError(e);
			} finally {
				if (output == null) {
					output = "";
				}
			}
		}
		
		protected override bool StartOperation()
		{
			SvnClient.Instance.OperationStart("CreatePatch", new ThreadStart(DoCreatePatchCommand));
			return true;
		}
	}
}
