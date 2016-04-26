using Minet.Compiler.AST;
using System.Collections.Generic;

namespace Minet.Compiler
{
	public static class Status
	{
		public static List<ErrorMsg> Errors = new List<ErrorMsg>();
		public static string Main;
		public static string Class = string.Empty;
		public static string ClassChain = string.Empty;
		public static string Chain = string.Empty;
		public static bool IsInChain { get { return !string.IsNullOrEmpty(Chain); } }
		public static int Indent = 0;
		public static int FnCounter = 0;
		public static bool NeedsThisVar = false;
		public static int ForCounter = 0;
		public static VarTracker Variables = new VarTracker();
		public static bool CurrentFnStatic = false;

		public static string ChainClassName(string part)
		{
			return string.IsNullOrEmpty(ClassChain) ? part : ClassChain + "." + part;
		}

		public static string ChainName(string part)
		{
			return string.IsNullOrEmpty(Chain) ? part : Chain + "." + part;
		}
	}

	public class VarTracker
	{
		public class VarTrackerItem
		{
			public int Depth = 0;
			public Identifier Ident;
		}

		public override string ToString()
		{
			var items = new List<string>();
			foreach (var i in Items)
			{
				string ident = i.Value.Ident.ToString();
				items.Add(ident == i.Key ? ident : i.Key + " -> " + ident);
			}
			return string.Join(", ", items);
		}

		public Dictionary<string, VarTrackerItem> Items = new Dictionary<string, VarTrackerItem>();

		public void AddItem(string val, Position pos)
		{
			AddItem(val, new Identifier(pos) { Idents = { val } });
		}

		public void AddItem(string key, Identifier ident)
		{
			if (Items.ContainsKey(key))
			{
				Status.Errors.Add(new ErrorMsg("A variable named " + key + " has already been declared at this scope.", ident.Pos));
			}
			else { Items.Add(key, new VarTrackerItem { Ident = ident }); }
		}

		public Identifier GetItem(string key)
		{
			return Items.ContainsKey(key) ? Items[key].Ident : null;
		}

		public void IncrementDepth()
		{
			foreach (var i in Items) { i.Value.Depth++; }
		}

		public void DecrementDepth()
		{
			var keys = new List<string>();
			foreach (var i in Items)
			{
				i.Value.Depth--;
				if (i.Value.Depth < 0) { keys.Add(i.Key); }
			}
			foreach (var k in keys) { Items.Remove(k); }
		}
	}

	public class ErrorMsg
	{
		public string Message { get; set; }
		public Position Position { get; set; }

		public ErrorMsg(string msg, Position pos)
		{
			Message = msg;
			Position = pos;
		}

		public override string ToString() { return Position + " - " + Message; }
	}

	public class Position
	{
		public int Line, Char;
		public override string ToString() { return Line + ":" + Char; }
	}
}
