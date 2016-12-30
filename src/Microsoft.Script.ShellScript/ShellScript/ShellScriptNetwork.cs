using System;
using System.Collections.Generic;

namespace Microsoft.Script
{
	public class ShellScriptNetwork
	{
		readonly internal ShellScript Parent;
		readonly internal List<Tuple<string, string, string>> Computers;
		string range, password;

		internal event Action<string> MessageProcessed;

		public ShellScriptNetwork (ShellScript script)
		{
			Computers = new List<Tuple<string, string, string>> ();
			Parent = script;
		}

		public ShellScript Arp (string arguments)
		{
			var message = Command.GetArp (arguments).ExecuteBash (Parent.RemoteIp, Parent.RemoteUser);
			OnMessageProcessed (message);
			return Parent;
		}

		public ShellScript RefreshNetworkDevices (string range, string password)
		{
			this.range = range; this.password = password;
			return RefreshNetworkDevices ();
		}

		public ShellScript RefreshNetworkDevices ()
		{
			Computers.Clear ();
			Computers.AddRange (Network.ScanRaspberryNetwork (range, password));
			return Parent;
		}

		public ShellScript Ping (string ip)
		{
			var message = Command.GetArp (ip).ExecuteBash (Parent.RemoteIp, Parent.RemoteUser);
			OnMessageProcessed (message);
			return Parent;
		}

		internal void OnMessageProcessed (string message)
		{
			MessageProcessed?.Invoke (message);
		}
	}
}
