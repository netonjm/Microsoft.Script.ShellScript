using System;

namespace Microsoft.Script
{
	public static class PS
	{
		public static void Kill (int pid, bool force = false, string ip = null, string user = "pi")
		{
			Command.KillProcess (pid, force).ExecuteBash (ip, user);
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

		public static Tuple<int, string, string> [] GetList (string ip = null, string user = "pi")
		{
			var variables = Command.GetProcess ().ExecuteBash (ip, user);
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

	}
}
