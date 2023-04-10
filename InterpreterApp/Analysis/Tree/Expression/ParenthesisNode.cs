using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class ParenthesisNode : ExpressionNode
    {
        public ParenthesisNode(Token open, ExpressionNode expression, Token close, int line, int column)
        {
            Open = open;
            Expression = expression;
            Close = close;
            Line = line;
            Column = column;
        }

        public Token Open { get; }
        public ExpressionNode Expression { get; }
        public Token Close { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
