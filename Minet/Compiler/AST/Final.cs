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

		public void Build(int indent, StringBuilder buffer)
		{
			var consSigBuffer = new StringBuilder();		// Constructor signature
			var consDefBuffer = new StringBuilder();		// Constructor defaults
			var consCodeBuffer = new StringBuilder();       // Constructor code
			var funcBuffer = new StringBuilder();           // Functions
			var staticPropBuffer = new StringBuilder();		// Static properties
			var classBuffer = new StringBuilder();			// Classes


			//
			// Initialize buffers
			//

			// Constructor signature
			Helper.PrintIndented("function ", indent + 1, consSigBuffer);
			consSigBuffer.Append(Name);
			consSigBuffer.Append("(");

			//
			// Statements and classes
			//
			foreach (var s in Statements) { s.AppendJS(indent + 1, Name, consSigBuffer, consDefBuffer, consCodeBuffer, funcBuffer, staticPropBuffer); }
			foreach (var c in Classes) { c.Build(indent + 1, classBuffer); }


			//
			// Finish buffers
			//

			// Constructor signature
			consSigBuffer.AppendLine(") {");

			// Constructor body
			Helper.PrintIndentedLine("}", indent + 1, consCodeBuffer);

			Helper.PrintIndented("var ", indent, buffer);
			buffer.Append(Name);
			buffer.AppendLine(" = (function () {");

			buffer.Append(consSigBuffer);
			buffer.Append(consDefBuffer);
			buffer.Append(consCodeBuffer);
			buffer.Append(funcBuffer);
			buffer.Append(staticPropBuffer);
			buffer.Append(classBuffer);

			Helper.PrintIndented("return ", indent + 1, buffer);
			buffer.Append(Name);
			buffer.AppendLine(";");
			Helper.PrintIndentedLine("})();", indent, buffer);
		}
	}

	public class F_Project : F_Class_Search
	{
		public List<JSBlock> JSBlocks = new List<JSBlock>();
		public StringBuilder Buffer = new StringBuilder();

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
			if (JSBlocks.Count > 0)
			{
				Buffer.AppendLine("// Javascript Blocks");
				foreach (var b in JSBlocks) { Buffer.AppendLine(b.Val); }
				Buffer.AppendLine();
			}

			if (Classes.Count > 0)
			{
				Buffer.AppendLine("// Classes");
				foreach (var c in Classes) { c.Build(0, Buffer); }
			}

			if (!string.IsNullOrEmpty(Compiler.Main))
			{
				Buffer.AppendLine("window.onload = function () {");
				Helper.PrintIndented(Compiler.Main, 1, Buffer);
				Buffer.AppendLine("();");
				Buffer.AppendLine("};");
			}

			return Buffer.ToString();
		}
	}
}
