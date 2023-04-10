using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class ConditionalNode : StatementNode
    {
        public ConditionalNode(List<ExpressionNode> expressions, List<ProgramNode> statements, List<int> lines, List<int> columns)
        {
            Expressions = expressions;
            Statements = statements;
            Lines = lines;
            Columns = columns;
        }

        public List<ExpressionNode> Expressions { get; }
        public List<ProgramNode> Statements { get; }
        public List<int> Lines { get; }
        public List<int> Columns { get; }
    }
}
