using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class LiteralNode : ExpressionNode
    {
        public LiteralNode(Token literal_token, object literal)
        {
            Literal_Token = literal_token;
            Literal = literal;
        }

        public Token Literal_Token { get; }
        public object Literal { get; }
    }
}
