using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class BinaryNode : ExpressionNode
    {
        public BinaryNode(ExpressionNode left, Token token_operator, ExpressionNode right, int line, int column)
        {
            Left = left;
            Token_Operator = token_operator;
            Right = right;
            Line = line;
            Column = column;
        }

        public ExpressionNode Left { get; }
        public Token Token_Operator { get; }
        public ExpressionNode Right { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
