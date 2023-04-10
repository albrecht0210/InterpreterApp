using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Tree.Statement
{
    public class ScanNode : StatementNode
    {
        public ScanNode(List<string> identifiers, int line, int column) 
        {
            Identifiers = identifiers;
            Line = line;
            Column = column;
        }

        public List<string> Identifiers { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
