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
				else if (s is Enum && doStatic)
				{
					var e = s as Enum;
					foreach (var n in e.Names)
					{
						if (n != Name)
						{
							Status.Variables.AddItem(n, new Identifier(e.Pos) { Idents = { Name, n } });
						}
					}
				}
			}
		}

		public void Build(StringBuilder buffer)
		{
			var consSigBuffer = new StringBuilder();        // Constructor signature
			var consDefBuffer = new StringBuilder();        // Constructor defaults
			var consCodeBuffer = new StringBuilder();       // Constructor code
			var classBuffer = new StringBuilder();          // Subclasses
			var instPropBuffer = new StringBuilder();       // Instance properties
			var instFuncBuffer = new StringBuilder();       // Instance functions
			var statVarBuffer = new StringBuilder();        // Static variables
			var statPropBuffer = new StringBuilder();       // Static properties
			var statFuncBuffer = new StringBuilder();       // Static functions
			var jsBlockBuffer = new StringBuilder();        // JavaScript blocks

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
			foreach (var st in Statements) { st.AppendJS(true, consSigBuffer, consDefBuffer, consCodeBuffer, instPropBuffer, instFuncBuffer, statVarBuffer, statPropBuffer, statFuncBuffer, jsBlockBuffer); }

			// Add instance variables
			BuildVarStmtList(false);

			// Instance statements
			foreach (var st in Statements) { st.AppendJS(false, consSigBuffer, consDefBuffer, consCodeBuffer, instPropBuffer, instFuncBuffer, statVarBuffer, statPropBuffer, statFuncBuffer, jsBlockBuffer); }

			// Remove variables before building classes
			Status.Variables.DecrementDepth();

			foreach (var c in Classes) { c.Build(classBuffer); }

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

			Status.Indent++;

			Helper.PrintIndentedLine("// Constructor", buffer);
			buffer.Append(consSigBuffer);
			buffer.Append(consDefBuffer);
			buffer.Append(consCodeBuffer);
			if (classBuffer.Length > 0) { Helper.PrintIndentedLine("// Subclasses", buffer); }
			buffer.Append(classBuffer);
			if (instPropBuffer.Length > 0) { Helper.PrintIndentedLine("// Instance properties", buffer); }
			buffer.Append(instPropBuffer);
			if (instFuncBuffer.Length > 0) { Helper.PrintIndentedLine("// Instance functions", buffer); }
			buffer.Append(instFuncBuffer);
			if (statVarBuffer.Length > 0) { Helper.PrintIndentedLine("// Static variables", buffer); }
			buffer.Append(statVarBuffer);
			if (statPropBuffer.Length > 0) { Helper.PrintIndentedLine("// Static properties", buffer); }
			buffer.Append(statPropBuffer);
			if (statFuncBuffer.Length > 0) { Helper.PrintIndentedLine("// Static functions", buffer); }
			buffer.Append(statFuncBuffer);
			if (jsBlockBuffer.Length > 0) { Helper.PrintIndentedLine("// JavaScript blocks", buffer); }
			buffer.Append(jsBlockBuffer);

			Helper.PrintIndented("return ", buffer);
			buffer.Append(Name);
			buffer.AppendLine(";");

			Status.Indent--;

			Helper.PrintIndentedLine("})();", buffer);

			Status.Class = priorClass;
			Status.ClassChain = priorClassChain;
		}
	}

	public class F_Project : F_Class_Search
	{
		public List<JSBlock> JSBlocks = new List<JSBlock>();
		public StringBuilder Buffer = new StringBuilder();

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
						foreach (var i in use.Items)
						{
							if (i.Repl != null) { Status.Variables.AddItem(i.Name, i.Repl); }
							else { Status.Variables.AddItem(i.Name, i.Pos); }
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
				foreach (var c in Classes) { c.Build(Buffer); }
			}

			if (JSBlocks.Count > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Javascript Blocks");
				foreach (var b in JSBlocks) { Buffer.AppendLine(b.Val); }
			}

			if (Status.Inits.Count > 0 || !string.IsNullOrEmpty(Status.Main))
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Call Init and Main");
				Buffer.AppendLine("window.onload = function() {");
				foreach (var i in Status.Inits)
				{
					Helper.PrintIndented(i, 1, Buffer);
					Buffer.AppendLine("();");
				}
				if (!string.IsNullOrEmpty(Status.Main))
				{
					Helper.PrintIndented(Status.Main, 1, Buffer);
					Buffer.AppendLine("();");
				}
				Buffer.AppendLine("};");
			}

			Status.Variables.DecrementDepth();
			return Buffer.ToString();
		}
	}
}
