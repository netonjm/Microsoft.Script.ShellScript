using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Script
{
	public static class Network
	{
		#region Public static API

		public static string GetHostName (this IPAddress ip, string user = "pi")
		{
			return Command.GetHostName ().ExecuteBash (ip, user);
		}

		public static string SetHostName (string hostname, IPAddress ip = null, string user = "pi")
		{
			return Command.SetHostname (hostname).ExecuteBash (ip, user);
		}

		public static string SetHostName (this IPAddress ip, string hostname, string user = "pi")
		{
			return SetHostName (hostname, ip, user);
		}

		public static string Arp (this IPAddress ip, string arguments, string user = "pi")
		{
			return Arp (arguments, ip, user);
		}

		public static string Arp (string arguments, IPAddress ip = null, string user = "pi")
		{
			return Command.GetArp (arguments).ExecuteBash (ip, user);
		}

		public static bool TestSsh (this IPAddress ip, string user = "pi", int timeout = 2)
		{
			return Command.TestSsh (ip, user, timeout).ExecuteBash () == "1";
		}

		public static List<Tuple<IPAddress, string, string>> ScanRaspberryNetwork (string range, string password, string user = "pi")
		{
			//TODO: force send sudo password command provokes a empty error, we need better way to do this
			var response = Command.GetNetworkConnections (range, password)
										.ExecuteBash (ignoreError: true);
			var computers = GetNetworkComputers (response);
			return FilterRaspberry (computers, user);
		}

		#endregion

		#region Private methods

		static IPAddress GetIp (string line)
		{
			line = line.Trim ();
			line = line.Substring (line.LastIndexOf (' ')).Trim ();
			IPAddress ip;
			if (!IPAddress.TryParse (line, out ip)) {
				ip = null;
			}
			return ip;
		}

		static void GetMacHost (string line, out string mac, out string host)
		{
			line = line.Substring ("MAC Address: ".Length);
			var separatorIndex = line.IndexOf (' ');
			mac = line.Substring (0, separatorIndex).Trim ();
			host = line.Substring (separatorIndex).Trim ();
			host = host.Substring (1, host.Length - 2);
		}

		static List<Tuple<IPAddress, string, string>> GetNetworkComputers (string data)
		{
			var splitted = data.Split ('\n');
			var elements = new List<Tuple<IPAddress, string, string>> ();
			for (int i = 1; i + 2 < splitted.Length; i += 3) {
				string host, mac;
				GetMacHost (splitted [i + 2], out host, out mac);
				elements.Add (
					new Tuple<IPAddress, string, string> (
						GetIp (splitted [i]), host, mac)
				);
			}
			//last element our connection
			elements.Remove (elements.LastOrDefault ());
			return elements;
		}

		static List<Tuple<IPAddress, string, string>> FilterRaspberry (List<Tuple<IPAddress, string, string>> data, string user = "pi")
		{
			var macFilters = new [] { 
				"B8:27:EB", //raspberry 3
				"D4:7B" }; //raspberry zero
			var result = new List<Tuple<IPAddress, string, string>> ();
			var filtered = data.Where (s => macFilters.Any (f => s.Item2.ToUpper ().StartsWith (f, StringComparison.Ordinal))).ToList ();
			foreach (var item in filtered) {
				var realHost = GetHostName (item.Item1, user);
				result.Add (new Tuple<IPAddress, string, string> (item.Item1, item.Item2, realHost));
			}
			return result;
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
