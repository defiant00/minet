using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Status
	{
		public List<string> Errors = new List<string>();
		public string Main;
		public string Class = string.Empty;
		public string Chain = string.Empty;
		public int Indent = 0;
		public int ForCounter = 0;

		public string ChainName(string part)
		{
			return string.IsNullOrEmpty(Chain) ? part : Chain + "." + part;
		}
	}
}
