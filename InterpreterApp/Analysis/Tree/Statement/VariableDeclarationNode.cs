using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class VariableDeclarationNode : StatementNode
    {
        public VariableDeclarationNode(TokenType data_type_token, Dictionary<string, ExpressionNode> variables, int line, int column)
        {
            Data_Type_Token = data_type_token;
            Variables = variables;
            Line = line;
            Column = column;
        }

        public TokenType Data_Type_Token { get; }
        public Dictionary<string, ExpressionNode> Variables { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
