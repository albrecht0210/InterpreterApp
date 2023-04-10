﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Syntax
{
    public class Token
    {
        public Token(TokenType token_type, string code, object value, int line, int column)
        {
            Token_Type = token_type;
            Code = code;
            Value = value;
            Line = line;
            Column = column;
        }

        public TokenType Token_Type { get; }
        public string Code { get; }
        public object Value { get; }
        public int Line { get; }
        public int Column { get; }

        public override string ToString()
        {
            return $"Token({Token_Type}, {Code}, {Value})";
        }
    }
}
