using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minet.Compiler.AST
{
	public class F_Class_Search
	{
		public List<F_Class> Classes = new List<F_Class>();

		public F_Class GetClass(List<string> name)
		{
			var c = Classes.Where(cl => cl.Name == name[0]).FirstOrDefault();
			if (c == null)
			{
				c = new F_Class { Name = name[0] };
				Classes.Add(c);
			}

			if (name.Count == 1)
			{
				return c;
			}
			return c.GetClass(name.Skip(1).ToList());
		}
	}

	public class F_Class : F_Class_Search
	{
		public override string ToString() { return Name; }

		public string Name;
		public List<IClassStatement> Statements = new List<IClassStatement>();

		public void Build(StringBuilder buffer, StringBuilder staticBuffer)
		{
			string priorClass = Status.Class;
			string priorClassChain = Status.ClassChain;
			Status.Class = Name;
			Status.ClassChain = Status.ChainClassName(Name);

			var consSigBuffer = new StringBuilder();        // Constructor signature
			var consDefBuffer = new StringBuilder();        // Constructor defaults
			var consCodeBuffer = new StringBuilder();       // Constructor code
			var funcBuffer = new StringBuilder();           // Functions
			var classBuffer = new StringBuilder();          // Classes


			//
			// Initialize buffers
			//

			Status.Indent++;

			// Constructor signature
			Helper.PrintIndented("function ", consSigBuffer);
			consSigBuffer.Append(Name);
			consSigBuffer.Append("(");

			//
			// Statements and classes
			//
			foreach (var st in Statements) { st.AppendJS(consSigBuffer, consDefBuffer, consCodeBuffer, funcBuffer, staticBuffer); }
			foreach (var c in Classes) { c.Build(classBuffer, staticBuffer); }


			//
			// Finish buffers
			//

			// Constructor signature
			consSigBuffer.AppendLine(") {");

			// Constructor body
			Helper.PrintIndentedLine("}", consCodeBuffer);

			Status.Indent--;

			Helper.PrintIndented(string.IsNullOrEmpty(priorClass) ? "var " : priorClass + ".", buffer);
			buffer.Append(Name);
			buffer.AppendLine(" = (function () {");

			buffer.Append(consSigBuffer);
			buffer.Append(consDefBuffer);
			buffer.Append(consCodeBuffer);
			buffer.Append(funcBuffer);
			buffer.Append(classBuffer);

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
		public StringBuilder StaticBuffer = new StringBuilder();

		public F_Project(List<File> files)
		{
			Buffer.Append("/* Built with ");
			Buffer.AppendLine(Constants.Program);
			Buffer.Append(" * Input Files: ");
			Buffer.AppendLine(string.Join(", ", files.Select(f => f.Name)));
			Buffer.AppendLine(" */");
			Buffer.AppendLine();

			foreach (var f in files)
			{
				foreach (var s in f.Statements)
				{
					if (s is Class)
					{
						var cl = s as Class;
						var fc = GetClass((cl.Name as Identifier).Idents);
						fc.Statements = cl.Statements;
					}
					else if (s is JSBlock) { JSBlocks.Add(s as JSBlock); }
					else { Buffer.AppendLine("Unknown statement type " + s.GetType()); }
				}
			}
		}

		public string Build()
		{
			if (Classes.Count > 0)
			{
				Buffer.AppendLine("// Classes");
				foreach (var c in Classes) { c.Build(Buffer, StaticBuffer); }
			}

			if (StaticBuffer.Length > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Static Variables");
				Buffer.Append(StaticBuffer);
			}

			if (JSBlocks.Count > 0)
			{
				Buffer.AppendLine();
				Buffer.AppendLine("// Javascript Blocks");
				foreach (var b in JSBlocks) { Buffer.AppendLine(b.Val); }
				Buffer.AppendLine();
			}

			if (!string.IsNullOrEmpty(Status.Main))
			{
				Buffer.AppendLine("window.onload = function () {");
				Helper.PrintIndented(Status.Main, 1, Buffer);
				Buffer.AppendLine("();");
				Buffer.AppendLine("};");
			}

			return Buffer.ToString();
		}
	}
}
