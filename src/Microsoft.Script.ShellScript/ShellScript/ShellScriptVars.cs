using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Script
{
	public class ShellScriptVars
	{
		List<Tuple<string, string>> variables = new List<Tuple<string, string>> ();
		ShellScript Parent;

		public ShellScriptVars (ShellScript script)
		{
			Parent = script;
		}

		#region Variables

		public ShellScript StoreLastMessage (string key)
		{
			return Add (key, Parent.LastMessage);
		}

		public ShellScript Add (string key, string keyValue)
		{
			variables.Add (new Tuple<string, string> (key, keyValue));
			return Parent;
		}

		public ShellScript Clear ()
		{
			variables.Clear ();
			return Parent;
		}

		public int Count {
			get { return variables.Count; }
		}

		public string Get (string id)
		{
			return variables.FirstOrDefault (s => s.Item1 == id)?.Item2;
		}

		public ShellScript Remove (string key)
		{
			variables.Remove (variables.FirstOrDefault (s => s.Item1 == key));
			return Parent;
		}

		public ShellScript WriteAll ()
		{
			Console.WriteLine ($"Number of Vars: {variables.Count}");
			foreach (var item in variables) {
				Console.WriteLine ($"{item.Item1} => {item.Item2}");
			}
			return Parent;
		}

		#endregion
	}
}
