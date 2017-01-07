using System;
using System.Linq;
using System.Net;

namespace Microsoft.Script.IO
{
	public static class File
	{
		public static bool Exist (string file, IPAddress ip = null, string user = "pi")
		{
			return Command.FileExists (file, CommandFileType.File).ExecuteBash (ip, user) == "1";
		}

		public static string Copy (string source, string destination, IPAddress ip = null, string user = "pi")
		{
			return Command.CopyFile (source, destination).ExecuteBash (ip, user);
		}

		public static string CopyToRemote (string source, string destination, IPAddress ip = null, string user = "pi")
		{
			return Command.CopyFileToRemote (source, destination, ip, user).ExecuteBash ();
		}

		public static void Remove (string file, IPAddress ip = null, string user = "pi")
		{
			Command.RemoveFile (file, false).ExecuteBash (ip, user);
		}

		public static string GetFromRemote (string source, string destination, IPAddress ip, string user = "pi")
		{
			var fullPath = "scp".FullPath (ip, user);
			return fullPath.ExecuteBash ($"{user}@{ip}:{source} {destination}");
		}
	}
}
