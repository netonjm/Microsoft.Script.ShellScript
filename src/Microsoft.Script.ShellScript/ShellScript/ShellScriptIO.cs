using System;

namespace Microsoft.Script
{
	public class ShellScriptIO
	{
		ShellScript Parent;
		internal event Action<string> MessageProcessed;

		public ShellScriptIO (ShellScript script)
		{
			Parent = script;
		}

		public ShellScript GetFile (string source, string destination)
		{
			var command = Command.GetFileFromRemote (source, destination, Parent.RemoteIp, Parent.RemoteUser)
										.ExecuteBash ();
			OnMessageProcessed (command);
			return Parent;
		}

		public ShellScript RemoveFile (string source)
		{
			var command = Command.RemoveFile (source)
			                           .ExecuteBash (Parent.RemoteIp, Parent.RemoteUser);
			OnMessageProcessed (command);
			return Parent;
		}

		public ShellScript CopyFile (string source, string destination)
		{
			var command = Command.CopyFileToRemote (source, destination,
			                                            Parent.RemoteIp, Parent.RemoteUser)
									   .ExecuteBash ();
			OnMessageProcessed (command);
			return Parent;
		}

		public ShellScript FileExits (string source, CommandFileType fileType)
		{
			var command = Command.FileExists (
				source.FullPath (Parent.RemoteIp, Parent.RemoteUser),
				fileType)
			                           .ExecuteBash (Parent.RemoteIp, Parent.RemoteUser);
			OnMessageProcessed (command);
			return Parent;
		}

		public ShellScript List (string parameter = "")
		{
			var command = Command.GetLS (parameter)
			                           .ExecuteBash (Parent.RemoteIp, Parent.RemoteUser);
			OnMessageProcessed (command);
			return Parent;
		}

		internal void OnMessageProcessed (string message)
		{
			MessageProcessed?.Invoke (message);
		}
	}
}
