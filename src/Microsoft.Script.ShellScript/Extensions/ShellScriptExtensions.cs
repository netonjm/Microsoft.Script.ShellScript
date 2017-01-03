using System;
using System.Collections.Generic;

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
	
		static string ExecuteCommand (this string sender, string arguments = "", bool ignoreError = true, string workingDirectory = null, Dictionary<string, string> environmentVariables = null)
		{
			string output, error;
			ShellScript.ExecuteCommand (sender, out output, out error, arguments, workingDirectory, environmentVariables);
			if (!ignoreError && !string.IsNullOrEmpty (error)) {
				throw new ArgumentException (error);
			}
			return output;
		}

		public static string FullPath (this string command, string ip = null, string user = "pi")
		{
			return Command.GetFullPath (command).ExecuteBash (ip, user);
		}

		public static string ExecuteBash (this string sender, string ip = null, string user = "pi", bool ignoreError = false, string workingDirectory = null, Dictionary<string, string> environmentVariables = null)
		{
			if (string.IsNullOrEmpty (ip)) {
				return "/bin/bash".ExecuteCommand ($"-c \"{sender}\"", ignoreError, workingDirectory, environmentVariables);
			}
			string output, error;
			ShellScript.ExecuteCommand ("ssh", out output, out error, $"{user}@{ip} '{sender}'", workingDirectory, environmentVariables);
			return output;
		}
	}
}
