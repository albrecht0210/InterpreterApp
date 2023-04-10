using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class IdentifierNode : ExpressionNode
    {
        public IdentifierNode(string name, int line, int column)
        {
            Name = name;
            Line = line;
            Column = column;
        }

        public string Name { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
