using System;
using System.Linq;
using System.Net;

namespace Microsoft.Script
{
	public static class PS
	{
		public static void Kill (int pid, bool force = false, IPAddress ip = null, string user = "pi", bool sudo = false)
		{
			Command.KillProcess (pid, force, sudo).ExecuteBash (ip, user);
		}

		public static int [] GetMonoPids (IPAddress ip = null, string user = "pi")
		{
			return GetPids ("mono", ip, user).Union (GetPids ("sudo mono", ip, user)).ToArray ();
		}

		public static Tuple<int, string> [] GetMonoProcesses (IPAddress ip = null, string user = "pi")
		{
			var pids = GetMonoPids (ip, user);
			var elements = new Tuple<int, string> [pids.Length];
			for (int i = 0; i < pids.Length; i++) {
				var actual = pids [i];
				elements [i] = new Tuple<int, string> (actual, GetProcess (actual, ip));
			}
			return elements;
		}

		public static Tuple<int, string> [] GetMonoProcess (string processName, IPAddress ip = null, string user = "pi")
		{
			return GetMonoProcesses (ip, user).Where (s => s.Item2.EndsWith ($" {processName}", StringComparison.Ordinal))
			                                           .ToArray ();
		}

		public static string GetProcess (int pid, IPAddress ip = null, string user = "pi")
		{
			var processLine = Command.GetProcess (pid).ExecuteBash (ip, user)
			                         .Split ('\n')
			                         .Skip (1)
			                         .FirstOrDefault ();
			return processLine;
		}

		public static int[] GetPids (string process, IPAddress ip = null, string user = "pi")
		{
			return Command.GetPids (process).ExecuteBash (ip, user)
				          .Split (new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				          .Select (s => int.Parse (s)).ToArray ();
		}

		public static int RunMonoBackground (string executable, IPAddress ip = null, string user = "pi", bool sudo = false)
		{
			var pid = Command.RunMonoBackground (executable, sudo)
			                 .ExecuteBash (ip, user, returnsPid: true, handleOutput: false);
			return int.Parse (pid);
		}

		public static int RunMonoBackgroundWithDebug (string executable, int debugPort = 10000, IPAddress ip = null, string user = "pi", bool sudo = false)
		{
			var pid = Command.RunMonoBackgroundWithDebug (executable, debugPort, sudo)
			                 .ExecuteBash (ip, user, returnsPid: true, handleOutput: false);
			return int.Parse (pid);
		}

		static Tuple<int, int, int> ExtractTitleIndexes (string title)
		{
			var pid_position = title.IndexOf ("PID", StringComparison.Ordinal) + "PID".Length;
			var args_position = title.IndexOf ("COMMAND", StringComparison.Ordinal);
			if (args_position == -1)
				args_position = title.IndexOf ("ARGS", StringComparison.Ordinal);
			var user_position = title.IndexOf ("USER", StringComparison.Ordinal);
			return new Tuple<int, int, int> (pid_position, args_position, user_position);
		}

		public static Tuple<int, string, string> [] GetDetailedList (IPAddress ip = null, string user = "pi")
		{
			var variables = Command.GetDetailedProcess ().ExecuteBash (ip, user);
			var lines = variables.Split ('\n');

			//generated index
			var indexes = ExtractTitleIndexes (lines [0]);

			//we don't want start from first index
			int start = 1;
			var processes = new Tuple<int, string, string> [lines.Length - start];

			for (int i = start; i < lines.Length; i++) {

				var pid = lines [i].Substring (0, indexes.Item1 + 2);
				var command = lines [i].Substring (indexes.Item2 + 2, indexes.Item3 - indexes.Item2 - 1);
				var usr = lines [i].Substring (indexes.Item3 + 2);

				processes [i - start] = new Tuple<int, string, string> (
					int.Parse (pid.Trim ()),
					command.Trim (),
					usr.Trim ()
				);
			}

			return processes;
		}

		public static Tuple<int, string> [] GetList (IPAddress ip = null, string user = "pi")
		{
			var variables = Command.GetDetailedProcess ().ExecuteBash (ip, user);
			var lines = variables.Split ('\n');

			//generated index
			var indexes = ExtractTitleIndexes (lines [0]);

			//we don't want start from first index
			int start = 1;
			var processes = new Tuple<int, string> [lines.Length - start];

			for (int i = start; i < lines.Length; i++) {

				var pid = lines [i].Substring (0, indexes.Item1 + 2);
				var command = lines [i].Substring (indexes.Item2 + 2);

				processes [i - start] = new Tuple<int, string> (
					int.Parse (pid.Trim ()),
					command.Trim ()
				);
			}

			return processes;
		}
	}
}
