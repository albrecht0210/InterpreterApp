﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Tree.Expression
{
    public class LiteralNode : ExpressionNode
    {
        public LiteralNode(object literal, int line, int column)
        {
            Literal = literal;
            Line = line;
            Column = column;
        }

        public object Literal { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
