using System.IO;
using NUnit.Framework;

namespace Microsoft.Script.ShellScriptTests
{
	[TestFixture]
	public class GeneralApiTests
	{
		readonly string directory = TestSession.Instance.ActualDirectory;

		[Test]
		public void StoreVariables ()
		{
			var script = ShellScript.Create ()
									.Vars.Add ("1", "hello")
									.Vars.Add ("2", "bye");

			Assert.AreEqual (script.Vars.Get ("1"), "hello", "#1");
			Assert.AreEqual (script.Vars.Get ("2"), "bye", "#2");
			Assert.AreEqual (script.Vars.Count, 2, "#3");
			script.Vars.Remove ("1");
			Assert.AreEqual (script.Vars.Count, 1, "#4");
			script.Vars.Clear ();
			Assert.AreEqual (script.Vars.Count, 0, "#5");
		}

		[Test]
		[Ignore ("we need create true/false context")]
		public void CreateAndRemoveFile ()
		{
			var emptyFile = Path.Combine (directory, "test");
			if (!File.Exists (emptyFile))
				File.Create (emptyFile).Dispose ();

			var script = ShellScript.Create ();
			script.IO.FileExits (emptyFile, CommandFileType.File);
			Assert.AreEqual (script.LastMessage, "1", "#5");

			script.IO.RemoveFile (emptyFile);
			script.IO.FileExits (emptyFile, CommandFileType.File);
			Assert.AreEqual (script.LastMessage, "0", "#5");
		}


		[Test]
		public void GetArp ()
		{
			var d = ShellScript.Create ()
					   .Network.Arp ("-a")
					   .Write (s => s.LastMessage);

			Assert.IsNotNull (d.LastMessage);
			Assert.IsNotEmpty (d.LastMessage);
		}
	}
}
