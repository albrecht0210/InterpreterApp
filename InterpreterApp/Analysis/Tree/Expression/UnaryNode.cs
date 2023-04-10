using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class UnaryNode : ExpressionNode
    {
        public UnaryNode(Token token_operator, ExpressionNode expression, int line, int column)
        {
            Token_Operator = token_operator;
            Expression = expression;
            Line = line;
            Column = column;
        }

        public Token Token_Operator { get; }
        public ExpressionNode Expression { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
