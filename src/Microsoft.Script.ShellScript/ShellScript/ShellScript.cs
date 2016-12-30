using System;

namespace Microsoft.Script
{
	public partial class ShellScript
	{
		public ShellScriptVars Vars;

		public ShellScriptIO IO;
		public ShellScriptNetwork Network;

		public string RemoteIp
		{
			get;
			private set;
		}

		public string RemoteUser
		{
			get;
			private set;
		}

		public string LastMessage
		{
			get;
			private set;
		}

		public static ShellScript Create()
		{
			return new ShellScript();
		}

		public ShellScript()
		{
			Vars = new ShellScriptVars(this);
			IO = new ShellScriptIO(this);
			IO.MessageProcessed += m => {
				LastMessage = m;
			};
			Network = new ShellScriptNetwork (this);
			Network.MessageProcessed += m => LastMessage = m;
		}

		public ShellScript ConfigureRemoteIp(string ip, string user = "pi")
		{
			RemoteIp = ip;
			RemoteUser = user;
			return this;
		}

		public ShellScript Write(string message)
		{
			Console.WriteLine(message);
			return this;
		}

		public ShellScript Write(Func<ShellScript, string> action)
		{
			Console.WriteLine(action(this));
			return this;
		}

		#region Conditionals

		public ShellScript For(Action<ShellScript, int> action, int count)
		{
			for (int j = 0; j < count; j++)
			{
				action(this, j);
			}
			return this;
		}

		public ShellScript While(Action<ShellScript> action, Func<bool> condition)
		{
			while (condition())
			{
				action(this);
			}
			return this;
		}

		#endregion

	}
}
