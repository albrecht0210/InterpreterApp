using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Syntax
{
    public class Lexer
    {
        private readonly string code;
        private int position;
        private int line, column;
        private int parenthesis_count;

        public Lexer(string code)
        {
            this.code = code;
            this.position = 0;
            this.line = 1;
            this.column = 1;
            this.parenthesis_count = 0;
        }

        private char Current => Peek(0);
        private char LookAhead => Peek(1);

        private char Peek(int offset)
        {
            int index = position + offset;
            if (index >= code.Length)
                return '\0';
            return code[index];
        }
        
        private void Next(int offset = 1)
        {
            position += offset;
            column += offset;
        }

        public Token GetToken()
        {
            while (position < code.Length)
            {
                if (char.IsLetter(Current))
                    return GetKeywordOrIdentifierToken();

                switch (Current)
                {
                    case ' ':
                        Next();
                        continue;
                    case '\n':
                        Token new_line = new Token(TokenType.NewLineToken, "\n", null, line, column);
                        line++;
                        column = 1;
                        Next();
                        return new_line;
                    case '\t':
                        Next();
                        return new Token(TokenType.TabToken, "\t", null, line, column);
                    case '\'':
                        return GetCharacterLiteralToken();
                    case '\"':
                        return GetBooleanOrStringLiteralToken();
                    case '.':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        return GetNumberLiteralToken();
                    case '_':
                        return GetKeywordOrIdentifierToken();
                    case '#':
                        return GetCommentToken();
                    case '$':
                        Next();
                        return new Token(TokenType.DollarToken, "$", null, line, column);
                    case '&':
                        Next();
                        return new Token(TokenType.AmpersandToken, "&", null, line, column);
                    case '[':
                        return GetEscapeCodeToken();
                    case '(':
                        Next();
                        parenthesis_count++;
                        return new Token(TokenType.OpenParenthesisToken, "(", null, line, column);
                    case ')':
                        Next();
                        parenthesis_count--;
                        return new Token(TokenType.CloseParenthesisToken, ")", null, line, column);
                    case '*':
                        Next();
                        return new Token(TokenType.StarToken, "*", null, line, column);
                    case '/':
                        Next();
                        return new Token(TokenType.SlashToken, "/", null, line, column);
                    case '%':
                        Next();
                        return new Token(TokenType.PercentToken, "%", null, line, column);
                    case '+':
                        Next();
                        return new Token(TokenType.PlusToken, "+", null, line, column);
                    case '-':
                        Next();
                        return new Token(TokenType.MinusToken, "-", null, line, column);
                    case '>':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.GreaterEqualToken, ">=", null, line, column);
                        }
                        Next();
                        return new Token(TokenType.GreaterThanToken, ">", null, line, column);
                    case '<':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.LessEqualToken, "<=", null, line, column);
                        }
                        else if (LookAhead == '>')
                        {
                            Next(2);
                            return new Token(TokenType.NotEqualToken, "<>", null, line, column);
                        }
                        Next();
                        return new Token(TokenType.LessThanToken, "<", null, line, column);
                    case '=':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.EqualToToken, "==", null, line, column);
                        }
                        Next();
                        return new Token(TokenType.EqualsToken, "=", null, line, column);
                    case ',':
                        Next();
                        return new Token(TokenType.CommaToken, ",", null, line, column);
                    case ':':
                        Next();
                        return new Token(TokenType.ColonToken, ":", null, line, column);
                    default:
                        Next();
                        return new Token(TokenType.ErrorToken, Current.ToString(), null, line, column);
                }
            }
            return new Token(TokenType.EndOfFileToken, "\0", null, line, column);
        }

        private Token GetKeywordOrIdentifierToken()
        {
            int start = position;
            while (char.IsLetter(Current) || Current == '_' || char.IsDigit(Current))
                Next();

            int length = position - start;
            string text = code.Substring(start, length);
            TokenType token_type = Grammar.GetKeywordTokenType(text);
            return new Token(token_type, text, null, line, column);
        }

        private Token GetCharacterLiteralToken()
        {
            // Note: the char value only accepts letters
            int start = position;
            Next();
            if (LookAhead == '\'')
            {
                char value = Current;
                Next(2);
                int lenght = position - start;
                string text = code.Substring(start, lenght);
                return new Token(TokenType.CharLiteralToken, text, value, line, column);
            }
            return new Token(TokenType.ErrorToken, Current.ToString(), null, line, column);
        }

        private Token GetBooleanOrStringLiteralToken()
        {
            int start = position;
            Next();
            while (Current != '\"')
                Next();

            if (Current == '\"')
            {
                Next();
                int length = position - start;
                string text = code.Substring(start, length);

                if (text == "\"TRUE\"")
                    return new Token(TokenType.BoolLiteralToken, text, true, line, column);
                else if (text == "\"FALSE\"")
                    return new Token(TokenType.BoolLiteralToken, text, false, line, column);
                else
                    return new Token(TokenType.StringLiteralToken, text, code.Substring(start + 1, length - 2), line, column);
            }
            return new Token(TokenType.ErrorToken, Current.ToString(), null, line, column);
        }

        private Token GetNumberLiteralToken()
        {
            bool is_float = false;
            int start = position;

            while (char.IsDigit(Current) || Current == '.')
            {
                if (Current == '.' && is_float)
                {
                    return new Token(TokenType.ErrorToken, Current.ToString(), null, line, column);
                }
                else if (Current == '.' && !is_float)
                    is_float = true;
                Next();
            }

            int length = position - start;
            string text = code.Substring(start, length);
            if (is_float)
            {
                if (!float.TryParse(text, out var float_value))
                    return new Token(TokenType.ErrorToken, text, null, line, column);
                return new Token(TokenType.FloatLiteralToken, text, float_value, line, column);
            }
            if (!int.TryParse(text, out var int_value))
                return new Token(TokenType.ErrorToken, text, null, line, column);
            return new Token(TokenType.IntLiteralToken, text, int_value, line, column);
        }

        private Token GetCommentToken()
        {
            int start = position;
            Next();
            while (Current != '\n' && Current != '\0')
                Next();

            int length = position - start;
            string text = code.Substring(start, length);
            return new Token(TokenType.CommentToken, text, null, line, column);
        }

        private Token GetEscapeCodeToken()
        {
            int start = position;
            Next();
            if (LookAhead == ']')
            {
                char value = Current;
                Next(2);
                int lenght = position - start;
                string text = code.Substring(start, lenght);
                return new Token(TokenType.EscapeToken, text, value, line, column);
            }
            return new Token(TokenType.ErrorToken, Current.ToString(), null, line, column);
        }
    }
}
