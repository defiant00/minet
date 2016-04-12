using System.Collections.Generic;

namespace Minet.Compiler
{
	public class BuildConfig
	{
		public Dictionary<string, string> Flags = new Dictionary<string, string>();
		public List<string> Files = new List<string>();

		public bool IsSet(string flag) { return Flags.ContainsKey(flag); }
		public string this[string flag] { get { return Flags[flag]; } }
	}
}
