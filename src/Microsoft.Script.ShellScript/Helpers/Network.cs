using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Script
{
	public static class Network
	{
		#region Public static API

		public static string Arp (string arguments, string ip = null, string user = "pi")
		{
			return Command.GetArp (arguments).ExecuteBash (ip, user);
		}

		public static bool TestSsh (string ip, string user = "pi", int timeout = 2)
		{
			return Command.TestSsh (ip, user, timeout).ExecuteBash () == "1";
		}

		public static List<Tuple<string, string, string>> ScanRaspberryNetwork (string range, string password)
		{
			//TODO: force send sudo password command provokes a empty error, we need better way to do this
			var response = Command.GetNetworkConnections (range, password)
										.ExecuteBash (ignoreError: true);
			var computers = GetNetworkComputers (response);
			return FilterRaspberry (computers);
		}

		#endregion

		#region Private methods

		static string GetIp (string line)
		{
			line = line.Trim ();
			return line.Substring (line.LastIndexOf (' ')).Trim ();
		}

		static void GetMacHost (string line, out string mac, out string host)
		{
			line = line.Substring ("MAC Address: ".Length);
			var separatorIndex = line.IndexOf (' ');
			mac = line.Substring (0, separatorIndex).Trim ();
			host = line.Substring (separatorIndex).Trim ();
			host = host.Substring (1, host.Length - 2);
		}

		static List<Tuple<string, string, string>> GetNetworkComputers (string data)
		{
			var splitted = data.Split ('\n');
			var elements = new List<Tuple<string, string, string>> ();
			for (int i = 1; i + 2 < splitted.Length; i += 3) {
				string host, mac;
				GetMacHost (splitted [i + 2], out host, out mac);
				elements.Add (
					new Tuple<string, string, string> (
						GetIp (splitted [i]), host, mac)
				);
			}
			//last element our connection
			elements.Remove (elements.LastOrDefault ());
			return elements;
		}

		static List<Tuple<string, string, string>> FilterRaspberry (List<Tuple<string, string, string>> data)
		{
			return data.Where (s => s.Item2.ToUpper ().StartsWith ("B8:27:EB", StringComparison.Ordinal))
					   .ToList ();
		}

		#endregion

		//public static NetworkInterface GetMainNetworkInterface ()
		//{
		//	IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties ();
		//	NetworkInterface [] nics = NetworkInterface.GetAllNetworkInterfaces ();
		//	Console.WriteLine ("Interface information for {0}.{1}     ",
		//			computerProperties.HostName, computerProperties.DomainName);
		//	if (nics == null || nics.Length < 1) {
		//		Console.WriteLine ("  No network interfaces found.");
		//		return null;
		//	}
		//	return nics [0];
		//}
		//public static Dictionary<IPAddress, string> ShowNetworkInterfaces (NetworkInterface adapter)
		//{
		//	Console.WriteLine ();
		//	Console.WriteLine (adapter.Description);
		//	Console.WriteLine (String.Empty.PadLeft (adapter.Description.Length, '='));
		//	Console.WriteLine ("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
		//	Console.Write ("  Physical address ........................ : ");
		//	PhysicalAddress address = adapter.GetPhysicalAddress ();
		//	byte [] bytes = address.GetAddressBytes ();
		//	for (int i = 0; i < bytes.Length; i++) {
		//		// Display the physical address in hexadecimal.
		//		Console.Write ("{0}", bytes [i].ToString ("X2"));
		//		// Insert a hyphen after each byte, unless we are at the end of the
		//		// address.
		//		if (i != bytes.Length - 1) {
		//			Console.Write ("-");
		//		}
		//	}
		//	Console.WriteLine ();
		//	return null;
		//}
	}
}
