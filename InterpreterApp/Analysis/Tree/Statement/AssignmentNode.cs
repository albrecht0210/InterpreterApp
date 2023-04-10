using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class AssignmentNode : StatementNode
    {
        public AssignmentNode(List<string> identifiers, ExpressionNode expression, int line, int column)
        {
            Identifiers = identifiers;
            Expression = expression;
            Line = line;
            Column = column;
        }

        public List<string> Identifiers { get; }
        public ExpressionNode Expression { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
