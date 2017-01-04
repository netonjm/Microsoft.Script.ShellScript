using System;
using NUnit.Framework;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Script.ShellScriptTests
{
	[TestFixture]
	public class GeneralTests
	{
		const string remote = "10.67.1.53";
		const string remoteExecutable = "/home/pi/deployed/RaspberryTestProject.exe";
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

		[TestCase ("127.0.0.1", false)]
		[TestCase (remote, true)]
		public void TestSsh (string ip, bool value)
		{
			Assert.AreEqual (value, Network.TestSsh (ip), "#1");
		}

		[TestCase (remote, remoteExecutable, "mono")]
		[Ignore ("we need a long process to make this test pass")]
		public void GetMonoProcess (string ip, string file, string processName)
		{
			Assert.IsTrue (IO.File.Exist (remoteExecutable, ip), "#0 file doesn't exists");
			//hacky but openssh seems to ignore signals
			PS.RunMonoBackground ($"{file}", ip);
			//ps x | grep 'mono RaspMonoProject.exe' | grep -v 'grep 'mono RaspMonoProject.exe' | awk '{ print $1 }' | xargs kill
			var processes = PS.GetMonoProcess (file, ip);
			Assert.IsTrue (processes.Any (), "#1");
			foreach (var process in processes) {
				PS.Kill (process.Item1, true, ip);
			}
			processes = PS.GetMonoProcess (file, ip);
			Assert.IsFalse (processes.Any (), "#2");
		}

		[TestCase (remote, "/home/pi/deployed/RaspberryTestProject.exe")]
		public void KillMonoProcess (string ip, string file)
		{
			var processes = PS.GetMonoProcess (file, ip);
			Assert.IsTrue (processes.Any (), "#1");
			foreach (var process in processes) {
				PS.Kill (process.Item1, true, ip, sudo:true);
			}
			processes = PS.GetMonoProcess (file, ip);
			Assert.IsFalse (processes.Any (), "#2");
		}

		[Ignore ("we need a long process to make this test pass")]
		[TestCase (remote, remoteExecutable, "mono")]
		public void RunBackground (string ip, string file, string processName)
		{
			//hacky but openssh seems to ignore signals
			PS.RunMonoBackground (file, ip);
			//ps x | grep 'mono RaspMonoProject.exe' | grep -v 'grep 'mono RaspMonoProject.exe' | awk '{ print $1 }' | xargs kill
			var list = PS.GetPids (processName, ip);
			Assert.IsTrue (list.Count () > 0, "#1");

			foreach (var item in list) {
				PS.Kill (item, true, ip);
			}

			list = PS.GetPids (processName, ip);
			Assert.IsTrue (list.Count () == 0, "#2");
		}

		[Test]
		[Ignore ("if sudo password is wrong we don't get here any result and the test fails")]
		public void ScanRaspberryNetwork ()
		{
			var range = remote.Substring (0, remote.LastIndexOf ('.'));
			var raspberry = Network.ScanRaspberryNetwork ($"{range}.0", SudoPassword);
			Assert.IsTrue (raspberry.Any (), "#1");
			Assert.AreEqual (remote, raspberry.FirstOrDefault ().Item1, "#2");
		}

		#endregion

		#region PS

		[TestCase (remoteExecutable, remote)]
		[Ignore ("we need a long process to make this test pass")]
		public void ExecuteMonoProcess (string process, string remoteIp)
		{
			Assert.IsTrue (IO.File.Exist (process, remoteIp), "#0 file doesn't exists in remote");
			PS.RunMonoBackgroundWithDebug (process ,ip: remoteIp, sudo: true);
			var ps = PS.GetMonoProcess (process, remoteIp);
			Assert.IsNotNull (ps, "#1");
			Assert.IsTrue (ps.Any (), "#2");
			foreach (var pid in ps) {
				PS.Kill (pid.Item1, true, remoteIp, sudo:true);
			}
			ps = PS.GetMonoProcess (process, remoteIp);
			Assert.IsNotNull (ps, "#1");
			Assert.IsFalse (ps.Any (), "#2");
		}

		[TestCase ("")]
		[TestCase (remote)]
		public void GetProcesses (string remoteIp)
		{
			var ps = PS.GetList (remoteIp);
			Assert.IsNotNull (ps);
			Assert.IsTrue (ps.Any ());
		}

		[TestCase ("/Applications/Safari.app/Contents/MacOS/Safari", "", "Safari")]
		public void KillProcess (string process, string remoteIp, string processName)
		{
			Process.Start (process);
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
