using System;
using System.Linq;

namespace Microsoft.Script.IO
{
	public static class Directory
	{
		public static string [] GetDirectories (string directory, bool includeHiddenFiles = true, string ip = null, string user = "pi")
		{
			var command = Command.GetFiles (directory,
												  includeHiddenFiles,
												  CommandFileType.Directory)
									   .ExecuteBash (ip, user, true)
									   .Split ('\n')
									   .Skip (1)
									   .ToArray ();
			return command;
		}

		public static bool Exist (string directory, string ip = null, string user = "pi")
		{
			return Command.FileExists (directory, CommandFileType.Directory).ExecuteBash (ip, user) == "1";
		}

		public static bool Create (string directory, string ip = null, string user = "pi")
		{
			return Command.MkDir (directory).ExecuteBash (ip, user) == "1";
		}

		public static void Remove (string directory, string ip = null, string user = "pi")
		{
			Command.RemoveFile (directory, true).ExecuteBash (ip, user);
		}

		public static string [] GetFiles (string directory, bool includeHiddenFiles = true, string ip = null, string user = "pi")
		{
			return Command.GetFiles (directory, includeHiddenFiles, CommandFileType.File)
								.ExecuteBash (ip, user, true).Split ('\n');
		}
	}
}
