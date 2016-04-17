using System.Collections.Generic;

namespace Minet.Compiler
{
	public class Status
	{
		public List<string> Errors = new List<string>();
		public string Main;
		public int Indent = 0;
		public int ForCounter = 0;
	}
}
