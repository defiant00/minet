using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minet.Compiler.AST
{
	public class F_Class_Search
	{
		public List<F_Class> Classes = new List<F_Class>();

		public F_Class GetClass(List<string> name, Position pos)
		{
			var c = Classes.Where(cl => cl.Name == name[0]).FirstOrDefault();
			if (c == null)
			{
				c = new F_Class(pos) { Name = name[0] };
				Classes.Add(c);
			}

			if (name.Count == 1)
			{
				return c;
			}
			return c.GetClass(name.Skip(1).ToList(), pos);
		}
	}

	public class F_Class : F_Class_Search
	{
		public override string ToString() { return Name; }

		public Position Pos;
		public string Name;
		public List<IClassStatement> Statements = new List<IClassStatement>();

		public F_Class(Position pos) { Pos = pos; }

		private void BuildVarClassList()
		{
			foreach (var c in Classes)
			{
				Status.Variables.AddItem(c.Name, new Identifier(c.Pos) { Idents = { Name, c.Name } });
			}
		}

		private void BuildVarStmtList(bool doStatic)
		{
			foreach (var s in Statements)
			{
				if (s is PropertySet)
				{
					var ps = s as PropertySet;
					foreach (var prop in ps.Props)
					{
						if (doStatic == prop.Static)
						{
							if (Name == prop.Name)
							{
								Status.Variables.AddItem(Name + " constructor", prop.Pos);
							}
							else
							{
								string parent = prop.Static ? Name : "this";
								Status.Variables.AddItem(prop.Name, new Identifier(prop.Pos) { Idents = { parent, prop.Name } });
							}
						}
					}
				}
				else if (s is PropGetSet)
				{
					var gs = s as PropGetSet;
					if (doStatic == gs.Prop.Static)
					{
						string parent = gs.Prop.Static ? Name : "this";
						if (parent != gs.Prop.Name)
						{
							Status.Variables.AddItem(gs.Prop.Name, new Identifier(gs.Pos) { Idents = { parent, gs.Prop.Name } });
						}
					}
				}
			}
		}

		public void Build(StringBuilder buffer, StringBuilder initBuffer)
		{
			var consSigBuffer = new StringBuilder();        // Constructor signature
			var consDefBuffer = new StringBuilder();        // Constructor defaults
			var consCodeBuffer = new StringBuilder();       // Constructor code
			var funcBuffer = new StringBuilder();           // Functions
			var classBuffer = new StringBuilder();          // Classes
			var staticBuffer = new StringBuilder();         // Static variables

			string priorClass = Status.Class;
			string priorClassChain = Status.ClassChain;
			Status.Class = Name;
			Status.ClassChain = Status.ChainClassName(Name);

			Status.Indent++;

			// Constructor signature
			Helper.PrintIndented("function ", consSigBuffer);
			consSigBuffer.Append(Name);
			consSigBuffer.Append("(");

			// Add class names
			Status.Variables.IncrementDepth();
			BuildVarClassList();

			// Add static variables
			Status.Variables.IncrementDepth();
			BuildVarStmtList(true);

			// Static statements
			foreach (var st in Statements) { st.AppendJS(true, consSigBuffer, consDefBuffer, consCodeBuffer, funcBuffer, staticBuffer, initBuffer); }

			// Add instance variables
			BuildVarStmtList(false);

			// Instance statements
			foreach (var st in Statements) { st.AppendJS(false, consSigBuffer, consDefBuffer, consCodeBuffer, funcBuffer, staticBuffer, initBuffer); }

			// Remove variables before building classes
			Status.Variables.DecrementDepth();

			foreach (var c in Classes) { c.Build(classBuffer, initBuffer); }

			// Remove classes
			Status.Variables.DecrementDepth();

			// Constructor signature
			consSigBuffer.AppendLine(") {");

			// Constructor body
			Helper.PrintIndentedLine("}", consCodeBuffer);

			Status.Indent--;

			Helper.PrintIndented(string.IsNullOrEmpty(priorClass) ? "var " : priorClass + ".", buffer);
			buffer.Append(Name);
			buffer.AppendLine(" = (function() {");

			buffer.Append(consSigBuffer);
			buffer.Append(consDefBuffer);
			buffer.Append(consCodeBuffer);
			buffer.Append(funcBuffer);
			buffer.Append(classBuffer);
			buffer.Append(staticBuffer);

			Helper.PrintIndented("return ", Status.Indent + 1, buffer);
			buffer.Append(Name);
			buffer.AppendLine(";");
			Helper.PrintIndentedLine("})();", buffer);

			Status.Class = priorClass;
			Status.ClassChain = priorClassChain;
		}
	}

	public class F_Project : F_Class_Search
	{
		public List<JSBlock> JSBlocks = new List<JSBlock>();
		public StringBuilder Buffer = new StringBuilder();
		public StringBuilder InitBuffer = new StringBuilder();

		public F_Project(List<File> files)
		{
			Buffer.Append("/* Built with ");
			Buffer.Append(Constants.Program);
			Buffer.Append(" on ");
			Buffer.AppendLine(DateTime.Now.ToString());
			Buffer.Append(" * Input Files: ");
			Buffer.AppendLine(string.Join(", ", files.Select(f => f.Name)));
			Buffer.AppendLine(" */");

			Status.Variables.AddItem("this", new Position("", 0, 0));

			foreach (var f in files)
			{
				foreach (var s in f.Statements)
				{
					if (s is Class)
					{
						var cl = s as Class;
						foreach (var n in cl.Names)
						{
							var fc = GetClass(n.Idents, n.Pos);
							fc.Statements.AddRange(cl.Statements);
						}
					}
					else if (s is JSBlock) { JSBlocks.Add(s as JSBlock); }
					else if (s is Use)
					{
						var use = s as Use;
						foreach (var n in use.Names)
						{
							Status.Variables.AddItem(n, use.Pos);
						}
					}
					else { Status.Errors.Add(new ErrorMsg("Unknown statement type " + s.GetType(), s.Pos)); }
				}
			}
		}

		public string Build()
		{
			Status.Variables.IncrementDepth();
			foreach (var c in Classes)
			{
				Status.Variables.AddItem(c.Name, c.Pos);
			}

			if (Classes.Count > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Classes");
				foreach (var c in Classes) { c.Build(Buffer, InitBuffer); }
			}

			if (InitBuffer.Length > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Call Init");
				Buffer.Append(InitBuffer);
			}

			if (JSBlocks.Count > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Javascript Blocks");
				foreach (var b in JSBlocks) { Buffer.AppendLine(b.Val); }
			}

			if (!string.IsNullOrEmpty(Status.Main))
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Run Main");
				Buffer.AppendLine("window.onload = function() {");
				Helper.PrintIndented(Status.Main, 1, Buffer);
				Buffer.AppendLine("();");
				Buffer.AppendLine("};");
			}

			Status.Variables.DecrementDepth();
			return Buffer.ToString();
		}
	}
}
