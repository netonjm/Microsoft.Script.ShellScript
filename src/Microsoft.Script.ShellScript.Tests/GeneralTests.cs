using System;
using NUnit.Framework;
using System.Linq;

namespace Microsoft.Script.ShellScriptTests
{
	[TestFixture]
	public class GeneralTests
	{
		const string networkrange = "10.67.1.";
		const string remote = "10.67.1.53";
		string directory => TestSession.Instance.ActualDirectory;
		string SudoPassword => TestSession.Instance.SudoPassword;

		[Test]
		public void GetApplicationTitleInstalled ()
		{
			var appInstalled = ShellScript.GetApplicationsTitleInstalled ();
			Assert.IsTrue (appInstalled.Length > 0, "Not applications installed");
		}

		[Test]
		[Ignore ("needs some fixes")]
		public void ScriptExecution ()
		{
			var output = Command.GenerateOsaScript ("set output to (\"test\")")
									  .ExecuteBash (ignoreError: true);
			Assert.AreEqual ("test", output, "applescript not executed correctly");
		}

		#region Network

		[Test]
		public void GetArp ()
		{
			var d = ShellScript.Create ()
					   .Network.Arp ("-a")
					   .Write (s => s.LastMessage);

			Assert.IsNotNull (d.LastMessage);
			Assert.IsNotEmpty (d.LastMessage);
		}

		[TestCase ("10.67.1.15", false)]
		[TestCase (remote, true)]
		public void TestSsh (string ip, bool value)
		{
			Assert.AreEqual (value, Network.TestSsh (ip), "#1");
		}

		[Test]
		[Ignore ("if sudo password is wrong we don't get here any result and the test fails")]
		public void ScanRaspberryNetwork ()
		{
			var range = remote.Substring (0, remote.LastIndexOf ('.'));
			var raspberry = Network.ScanRaspberryNetwork ($"{range}.0", SudoPassword);
			Assert.AreEqual (1, raspberry.Count, "#1");
			Assert.AreEqual (remote, raspberry.FirstOrDefault ().Item1, "#2");
		}

		#endregion

		#region PS

		[TestCase ("")]
		[TestCase (remote)]
		public void GetProcesses (string remoteIp)
		{
			var ps = PS.GetList (remoteIp);
			Assert.IsNotNull (ps);
			Assert.IsTrue (ps.Any ());
		}

		[TestCase ("/Applications/Safari.app/Contents/MacOS/Safari", "", "Safari")]
		//[TestCase ("",remote, "")]
		public void KillProcess (string process, string remoteIp, string processName)
		{
			System.Diagnostics.Process.Start (process);
			var processes = PS.GetList (remoteIp);
			var filteredProcess = processes.Where (s => s.Item2.Contains (processName)).ToList ();
			Assert.IsTrue (filteredProcess != null && filteredProcess.Any (), "#1");
			foreach (var proc in filteredProcess) {
				PS.Kill (proc.Item1, true, remoteIp);
			}
			filteredProcess = PS.GetList (remoteIp).Where (s => s.Item2.Contains (processName)).ToList ();
			Assert.IsTrue (filteredProcess == null || filteredProcess.Count == 0, "#2");
		}

		#endregion

		#region Environtment Variables

		[Test]
		public void GetSetEnvirontmentVariable ()
		{
			var variableName = "hello";
			var variableValue = "1234";
			Environment.SetEnvironmentVariable (variableName, null, EnvironmentVariableTarget.Process);
			var original = ShellScript.GetEnvirontmentVariables ().FirstOrDefault (s => s.Key == variableName);
			Assert.IsNull (original.Key, "#1-1");
			Assert.IsNull (original.Key, "#1-2");
			Environment.SetEnvironmentVariable (variableName, variableValue, EnvironmentVariableTarget.Process);
			original = ShellScript.GetEnvirontmentVariables ().FirstOrDefault (s => s.Key == variableName);
			Assert.AreEqual (original.Key, variableName, "#2-1");
			Assert.AreEqual (original.Value, variableValue, "#2-2");
		}

		#endregion

		#region IO

		[Test]
		public void CopyActualProject ()
		{
			var remoteDirectory = "/home/pi/test";
			if (IO.Directory.Exist (remoteDirectory, remote)) {
				IO.Directory.Remove (remoteDirectory, remote);
			}
			Assert.IsFalse (IO.Directory.Exist (remoteDirectory, remote), "#1");
			IO.Directory.Create (remoteDirectory, remote);
			Assert.IsTrue (IO.Directory.Exist (remoteDirectory, remote), "#2");

			var file = "$HOME/newfile";
			$"touch {file}".ExecuteBash ();
			IO.File.CopyToRemote (file, remoteDirectory, remote);

			var remoteFiles = IO.Directory.GetFiles (remoteDirectory, false, remote);
			Assert.AreEqual (1, remoteFiles.Length, "#3");
		}

		[Test]
		public void GetFullPath ()
		{
			Assert.AreEqual (true, IO.File.Exist ("/usr/bin/ftp"), "#1");
		}

		[TestCase ("")]
		[TestCase (remote)]
		public void GetFiles (string remoteIp)
		{
			var files = IO.Directory.GetFiles ("/home", false, remoteIp);
			Assert.IsTrue (files.Any (), "#1");
		}

		[TestCase ("")]
		[TestCase (remote)]
		public void GetDirectories (string remoteIp)
		{
			var files = IO.Directory.GetDirectories ("$HOME", false, remoteIp);
			Assert.IsTrue (files.Any (), "#1");
		}

		[TestCase ("")]
		[TestCase (remote)]
		public void DeleteFiles (string remoteIp)
		{
			var file = "$HOME/newfile";
			$"touch {file}".ExecuteBash (remoteIp);
			Assert.IsTrue (IO.File.Exist (file, remoteIp), "#1");
			IO.File.Remove (file, ip: remoteIp);
			Assert.IsFalse (IO.File.Exist (file, remoteIp), "#2");
		}
		#endregion
	}
}
