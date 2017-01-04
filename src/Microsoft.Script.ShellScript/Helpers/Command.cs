using System;
using System.Text;

namespace Microsoft.Script
{
	public enum CommandFileType
	{
		File, Directory
	}

	public static class Command
	{
		#region System profiler

		public static string GetSPApplicationsDataType ()
		{
			return "system_profiler SPApplicationsDataType -xml";
		}

		#endregion

		#region Environtment Variables

		public static string SetEnvirontmentVariable (string variable, string value)
		{
			return $"export {variable}={value}";
		}

		public static string GetEnvirontmentVariables (string pid = "")
		{
			if (pid == "")
				return GetEnvirontmentVariable ();
			return $"ps eww {pid}";
		}

		public static string GetEnvirontmentVariable (string variable = "")
		{
			variable = !string.IsNullOrEmpty (variable) ? " {variable}" : "";
			return $"/usr/bin/printenv{variable}";
		}

		#endregion

		#region Process

		public static string GetDetailedProcess ()
		{
			return $"ps -eo pid,args,user";
		}

		public static string GetProcesses ()
		{
			return $"ps -eo pid,args";
		}

		public static string GetProcess (int pid)
		{
			return $"ps -p {pid} -o args";
		}

		public static string GetPids (string process)
		{
			return $"pidof {process}";
		}

		public static string GetActualPID ()
		{
			return "echo $$";
		}

		public static string GetBASHPID ()
		{
			return "echo $BASHPID";
		}

		public static string KillProcess (int pid, bool force, bool sudo = false)
		{
			var sudoParam = sudo ? "sudo " : "";
			var forceParam = force ? "-9 " : "";
			return $"{sudoParam}kill {forceParam}{pid}";
		}

		#endregion

		#region Network

		public static string GetNetworkConnections (string range, string sudoPassword)
		{
			return $"echo {sudoPassword} | sudo -S /usr/local/bin/nmap -sP {range}/24";
		}

		public static string GetArp (string command)
		{
			return $"arp {command}";
		}

		internal static string RunMonoBackgroundWithDebug (string executable, int debugPort, bool sudo=false)
		{
			var sudoParam = sudo ? "sudo " : "";
			return $"nohup {sudoParam}mono --debug --debugger-agent=transport=dt_socket,address=0.0.0.0:{debugPort},server=y {executable} >> /dev/null 2>&1 &";
		}

		public static string RunMonoBackground (string executable, bool sudo = false)
		{
			var sudoParam = sudo ? "sudo " : "";
			return $"nohup {sudoParam}mono {executable} >> /dev/null 2>&1 &";
		}

		public static string TestSsh (string ip, string user = "pi", int timeout = 2)
		{
			return $"ssh -q -o BatchMode=yes -o ConnectTimeout={timeout} {user}@{ip} \'echo 2>&1\' && echo \'1\' || echo \'0\'";
		}

		#endregion

		#region File system

		public static string GetFullPath (string command)
		{
			return $"which {command}";
		}

		public static string CopyFileToRemote (string source, string destination, string ip, string user = "pi")
		{
			return $"scp {source} {user}@{ip}:{destination}";
		}

		public static string CopySshKeysToRemote (string ip, string user = "pi")
		{
			return $"cat .ssh/id_rsa.pub | ssh {user}@{ip} 'cat >> .ssh/authorized_keys'";
		}

		public static string CopyFile (string source, string destination)
		{
			return $"cp {source} {destination}";
		}

		public static string RemoveFile (string source, bool recursively = false)
		{
			return $"rm {(recursively ? "-r " : "")}{source}";
		}

		public static string FileExists (string file, CommandFileType fileType)
		{
			var type = fileType == CommandFileType.File ? "f" : "d";
			return $"test -{type} {file} && echo \'1\' || echo \'0\'";
		}

		public static string GetFileFromRemote (string source, string destination, string ip, string user = "pi")
		{
			return $"scp {user}@{ip}:{source} {destination}";
		}

		public static string GetLS (string parameter = "")
		{
			return $"ls {parameter}";
		}

		public static string GetMonoDirectory ()
		{
			return GetFullPath ("mono");
		}

		public static string MkDir (string directory)
		{
			return $"mkdir {directory}";
		}

		public static string GetFiles (string directoryBase, bool includeHiddenFiles, CommandFileType fileType)
		{
			var hiddenFiles = includeHiddenFiles ? " $'*/\\.*'" : "";
			var type = fileType == CommandFileType.File ? "f" : "d";
			return $"find {directoryBase} { hiddenFiles } -maxdepth 1 -type {type}";
		}

		#endregion

		#region Scripts

		public static string GenerateOsaScript (string command)
		{
			command = command.Replace ('\"', '\'');
			var osascript = "osascript".FullPath ();
			return $"{osascript} -e \'{command}\'";
		}

		public static string GenerateBashParameter (string command)
		{
			command = command.Replace ('\"', '\'');
			return $"-c \"{command}\"";
		}

		#endregion
	}
}
