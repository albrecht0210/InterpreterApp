using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class DisplayNode : StatementNode
    {
        public DisplayNode(Dictionary<ExpressionNode, bool> expression, int line, int column)
        {
            Expression = expression;
            Line = line;
            Column = column;
        }

        public Dictionary<ExpressionNode, bool> Expression { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
