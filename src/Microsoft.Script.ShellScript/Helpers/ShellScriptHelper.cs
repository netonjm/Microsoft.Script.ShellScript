using System;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Script
{
	public partial class ShellScript
	{
		#region Public helpers

		public static string [] GetApplicationsTitleInstalled ()
		{
			var spApplicationDatatype = GetSPApplicationsDataType ();
			var array = (JArray)spApplicationDatatype ["plist"] ["array"] ["dict"] ["array"] [1] ["dict"];
			var applicationsData = array.Select (s => s ["string"]).ToArray ();
			return applicationsData.Select (s => (string)s [0]).ToArray ();
		}

		public static void SetEnvirontmentVariable (string name, string value, string ip = null, string user = "pi")
		{
			Command.SetEnvirontmentVariable (name, value).ExecuteBash (ip, user);
		}

		public static string GetEnvirontmentVariable (string name, string ip = null, string user = "pi")
		{
			return Command.GetEnvirontmentVariable (name).ExecuteBash (ip, user);
		}

	
		public static Dictionary<string, string> GetEnvirontmentVariables (string ip = null, string user = "pi")
		{
			var variables = Command.GetEnvirontmentVariable ().ExecuteBash (ip, user);
			var variablesDictionary = new Dictionary<string, string> ();
			foreach (var item in variables.Split ('\n')) {
				var equalsIndex = item.IndexOf ('=');
				var key = item.Substring (0, equalsIndex).Trim ();
				var keyValue = item.Substring (equalsIndex + 1).Trim ();
				variablesDictionary.Add (key, keyValue);
			}
			return variablesDictionary;
		}

		public static void ExecuteCommand (string fileName, out string output, out string error, string arguments = "", string workingDirectory = null, 
		                                   Dictionary<string, string> environmentVariables = null)
		{
			using (var process = new Process ()) {
				process.StartInfo.FileName = fileName;
				process.StartInfo.Arguments = arguments; // Note the /c command (*)
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;

				if (workingDirectory != null)
					process.StartInfo.WorkingDirectory = workingDirectory;

				if (environmentVariables != null) {
					foreach (var item in environmentVariables) {
						process.StartInfo.EnvironmentVariables [item.Key] = item.Value;
					}
				}

				process.Start ();
				//* Read the output (or the error)
				output = process.StandardOutput.ReadToEnd ().Trim ();
				error = process.StandardError.ReadToEnd ().Trim ();
				process.WaitForExit ();
			};
#if DEBUG
			Console.WriteLine ($"{fileName} {arguments}");
			Console.WriteLine ($"{output}");
			if (!string.IsNullOrEmpty (error))
				Console.WriteLine ($"ERROR: {output}");
#endif
		}

		#endregion

		static dynamic GetSPApplicationsDataType ()
		{
			string output = Command.GetSPApplicationsDataType ().ExecuteBash ();
			var json = JsonConvert.SerializeXNode (XDocument.Parse (output));
			return JsonConvert.DeserializeObject<dynamic> (json);
		}
	}
}
