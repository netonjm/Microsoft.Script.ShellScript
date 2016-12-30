using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Script.ShellScriptTests
{
	public class TestSession
	{
		public string ActualDirectory;
		public string SudoPassword => "PASSWORD";

		public TestSession ()
		{
			ActualDirectory = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
		}

		static TestSession instance;
		public static TestSession Instance {
			get {
				if (instance == null)
					instance = new TestSession ();
				return instance; 
			}
		}
	}
}
