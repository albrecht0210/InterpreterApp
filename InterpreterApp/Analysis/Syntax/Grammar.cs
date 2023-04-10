using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Syntax
{
    public static class Grammar
    {
        public static TokenType GetKeywordTokenType(string input)
        {
            switch (input)
            {
                case "BEGIN":
                    return TokenType.BeginToken;
                case "END":
                    return TokenType.EndToken;
                case "CODE":
                    return TokenType.CodeToken;
                case "INT":
                    return TokenType.IntToken;
                case "FLOAT":
                    return TokenType.FloatToken;
                case "CHAR":
                    return TokenType.CharToken;
                case "BOOL":
                    return TokenType.BoolToken;
                case "IF":
                    return TokenType.IfToken;
                case "ELSE":
                    return TokenType.ElseToken;
                case "WHILE":
                    return TokenType.WhileToken;
                case "DISPLAY":
                    return TokenType.DisplayToken;
                case "SCAN":
                    return TokenType.ScanToken;
                case "AND":
                    return TokenType.AndToken;
                case "OR":
                    return TokenType.OrToken;
                case "NOT":
                    return TokenType.NotToken;
                default:
                    return TokenType.IdentifierToken;
            }
        }

        public static DataType GetDataType(TokenType token_type)
        {
            switch (token_type)
            {
                case TokenType.IntToken:
                case TokenType.IntLiteralToken:
                    return DataType.Int;
                case TokenType.FloatToken:
                case TokenType.FloatLiteralToken:
                    return DataType.Float;
                case TokenType.CharToken:
                case TokenType.CharLiteralToken:
                    return DataType.Char;
                case TokenType.BoolToken:
                case TokenType.BoolLiteralToken:
                    return DataType.Bool;
                default:
                    throw new Exception($"Unknown data type");
            }
        }

        public static DataType GetDataType(object val)
        {
            if (val.GetType() == typeof(int))
                return DataType.Int;
            else if (val.GetType() == typeof(float) || val.GetType() == typeof(double))
                return DataType.Float;
            else if (val.GetType() == typeof(char))
                return DataType.Char;
            else if (val.GetType() == typeof(bool))
                return DataType.Bool;
            else
                return DataType.String;
        }

        public static int GetUnaryPrecedence(TokenType token_type)
        {
            switch (token_type)
            {
                case TokenType.PlusToken:
                case TokenType.MinusToken:
                    return 8;
                default:
                    return 0;
            }
                
        }

        public static int GetBinaryPrecedence(TokenType token_type)
        {
            switch (token_type)
            {
                case TokenType.OrToken:
                    return 1;
                case TokenType.AndToken:
                    return 2;
                case TokenType.NotToken:
                    return 3;
                case TokenType.LessThanToken:
                case TokenType.LessEqualToken:
                case TokenType.GreaterThanToken:
                case TokenType.GreaterEqualToken:
                case TokenType.EqualToToken:
                case TokenType.NotEqualToken:
                    return 4;
                case TokenType.PlusToken:
                case TokenType.MinusToken:
                    return 5;
                case TokenType.PercentToken:
                    return 6;
                case TokenType.StarToken:
                case TokenType.SlashToken:
                    return 7;
                //case TokenType.OpenParenthesisToken:
                //case TokenType.CloseParenthesisToken:
                //    return 9;
                default:
                    return 0;
            }
        }

        public static object GetProperValue(string val)
        {
            string float_pattern = @"^-?\d+(\.\d+)?$";
            string int_pattern = @"^-?\d+$";
            string char_pattern = @"^'.'$";
            string bool_pattern = @"^""TRUE""|""FALSE""$";

            Regex float_regex = new Regex(float_pattern);
            Regex int_regex = new Regex(int_pattern);
            Regex char_regex = new Regex(char_pattern);
            Regex bool_regex = new Regex(bool_pattern);
            Debug.WriteLine(val);
            if (int_regex.IsMatch(val))
                return Convert.ToInt32(val);
            else if(float_regex.IsMatch(val))
                return Convert.ToDouble(val);
            else if (char_regex.IsMatch(val))
                return val;
            else if (bool_regex.IsMatch(val))
                return val == "\"TRUE\"" ? true : false;
            else
                return val;
        }

        public static bool IsLogicalOperator(TokenType token_type)
        {
            return (token_type == TokenType.LessThanToken || token_type == TokenType.GreaterThanToken ||
                token_type == TokenType.LessEqualToken || token_type == TokenType.GreaterEqualToken ||
                token_type == TokenType.EqualToToken || token_type == TokenType.NotEqualToken);
        }
    }
}
