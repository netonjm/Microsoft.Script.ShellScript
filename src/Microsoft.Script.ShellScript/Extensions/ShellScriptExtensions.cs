using System;
using System.Collections.Generic;
using System.Net;

namespace Microsoft.Script
{
	public static class ShellScriptExtensions
	{
		public static ShellScript FindRaspberry (this ShellScriptNetwork shell, string arguments)
		{
			var message = Network.Arp (arguments).ExecuteBash (ignoreError: true);
			shell.OnMessageProcessed (message);
			return shell.Parent;
		}
	
		static string ExecuteCommand (this string sender, string arguments = "", bool ignoreError = true, string workingDirectory = null, Dictionary<string, string> environmentVariables = null, bool returnsPid = false)
		{
			string output, error;
			ShellScript.ExecuteCommand (sender, out output, out error, arguments, workingDirectory, environmentVariables);
			if (!ignoreError && !string.IsNullOrEmpty (error)) {
				throw new ArgumentException (error);
			}
			return output;
		}

		public static string Which (this string command, IPAddress ip = null, string user = "pi")
		{
			return Command.Which (command).ExecuteBash (ip, user);
		}

		public static string ExecuteBash (this string sender, 
		                                  IPAddress ip = null, 
		                                  string user = "pi", 
		                                  string workingDirectory = null,
		                                  bool ignoreError = false, 
		                                  bool returnsPid = false, 
		                                  bool handleOutput = true,
		                                  Dictionary<string, string> environmentVariables = null)
		{
			string wait = !handleOutput ? " >> /dev/null 2>&1 &" : "";
			if (ip == null) {
				return "/bin/bash".ExecuteCommand ($"-c \"{sender}{wait}\"", ignoreError, workingDirectory, environmentVariables, returnsPid);
			}
			string output, error;
			ShellScript.ExecuteCommand ("ssh", out output, out error, $"{user}@{ip} '{sender}{wait}'", workingDirectory, environmentVariables, returnsPid);
			return output;
		}
	}
}
