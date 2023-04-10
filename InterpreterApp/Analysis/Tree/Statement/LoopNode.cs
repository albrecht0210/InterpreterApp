using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class LoopNode : StatementNode
    {
        public LoopNode(ExpressionNode expression, ProgramNode statement, int line, int column)
        {
            Expression = expression;
            Statement = statement;
            Line = line;
            Column = column;
        }

        public ExpressionNode Expression { get; }
        public ProgramNode Statement { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
