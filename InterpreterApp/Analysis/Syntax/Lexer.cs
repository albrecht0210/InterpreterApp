using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;
using static System.Net.Mime.MediaTypeNames;

namespace InterpreterApp.Analysis.Syntax
{
    public class Lexer
    {
        private readonly string _code;
        private int _position;
        private int _line, _column;

        public Lexer(string code)
        {
            this._code = code;
            this._position = 0;
            this._line = 1;
            this._column = 1;
        }

        private char Current => Peek(0);
        private char LookAhead => Peek(1);

        private char Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _code.Length)
                return '\0';
            return _code[index];
        }
        
        private void Next(int offset = 1)
        {
            _position += offset;
            _column += offset;
        }
        
        private void NewLine()
        {
            _line++;
            _column = 1;
            Next();
        }

        public Token GetToken()
        {
            while (_position < _code.Length)
            {
                if (char.IsLetter(Current))
                    return GetKeywordOrDataTypeOrIdentifierToken();

                if (char.IsDigit(Current))
                    return GetNumberLiteralToken();

                switch (Current)
                {
                    // WhiteSpace
                    case ' ':
                    case '\t':
                        Next();
                        continue;
                    case '\n':
                        Token new_line = new Token(TokenType.NEWLINE, "\n", null, _line, _column);
                        NewLine();
                        return new_line;
                    // Identifier
                    case '_':
                        return GetKeywordOrDataTypeOrIdentifierToken();
                    // Literals
                    case '\'':
                        return GetCharacterLiteralToken();
                    case '\"':
                        return GetBooleanOrStringLiteralToken();
                    case '.':
                        return GetNumberLiteralToken();
                    // Comment
                    case '#':
                        while (Current != '\n' && Current != '\0')
                            Next();
                        continue;
                    // Arithmetic Operators
                    case '*':
                        Next();
                        return new Token(TokenType.STAR, "*", null, _line, _column - 1);
                    case '/':
                        Next();
                        return new Token(TokenType.SLASH, "/", null, _line, _column - 1);
                    case '%':
                        Next();
                        return new Token(TokenType.PERCENT, "%", null, _line, _column - 1);
                    case '+':
                        Next();
                        return new Token(TokenType.PLUS, "+", null, _line, _column - 1);
                    case '-':
                        Next();
                        return new Token(TokenType.MINUS, "-", null, _line, _column - 1);
                    // Logical Operators
                    case '>':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.GREATEREQUAL, ">=", null, _line, _column - 2);
                        }
                        Next();
                        return new Token(TokenType.GREATERTHAN, ">", null, _line, _column - 1);
                    case '<':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.LESSEQUAL, "<=", null, _line, _column - 2);
                        }
                        else if (LookAhead == '>')
                        {
                            Next(2);
                            return new Token(TokenType.NOTEQUAL, "<>", null, _line, _column - 2);
                        }
                        Next();
                        return new Token(TokenType.LESSTHAN, "<", null, _line, _column - 1);
                    case '=':
                        if (LookAhead == '=')
                        {
                            Next(2);
                            return new Token(TokenType.EQUALTO, "==", null, _line, _column - 2);
                        }
                        Next();
                        return new Token(TokenType.EQUAL, "=", null, _line, _column - 1);
                    // Symbols
                    case '$':
                        Next();
                        return new Token(TokenType.DOLLAR, "$", null, _line, _column - 1);
                    case '&':
                        Next();
                        return new Token(TokenType.AMPERSAND, "&", null, _line, _column - 1);
                    case '[':
                        return GetEscapeCodeToken();
                    case '(':
                        Next();
                        return new Token(TokenType.OPENPARENTHESIS, "(", null, _line, _column - 1);
                    case ')':
                        Next();
                        return new Token(TokenType.CLOSEPARENTHESIS, ")", null, _line, _column - 1);
                    case ',':
                        Next();
                        return new Token(TokenType.COMMA, ",", null, _line, _column - 1);
                    case ':':
                        Next();
                        return new Token(TokenType.COLON, ":", null, _line, _column - 1);
                    default:
                        Next();
                        return new Token(TokenType.ERROR, Current.ToString(), "Unknown symbol", _line, _column - 1);
                }
            }
            return new Token(TokenType.ENDOFFILE, "\0", null, _line, _column);
        }

        private Token GetKeywordOrDataTypeOrIdentifierToken()
        {
            int start = _position;
            int line_col = _column;

            while (char.IsLetter(Current) || Current == '_' || char.IsDigit(Current))
                Next();

            int length = _position - start;
            string text = _code.Substring(start, length);

            return Grammar.GetWordToken(text, _line, line_col);
        }

        private Token GetCharacterLiteralToken()
        {
            int start = _position;
            int line_col = _column;

            Next();
            while (Current != '\'')
                Next();

            if (Current == '\'')
            {
                Next();
                int length = _position - start;
                string text = _code.Substring(start, length);
                object value = null;

                if (text.Length == 3)
                {
                    value = text.ToCharArray()[1];
                    return new Token(TokenType.CHARLITERAL, text, value, _line, line_col);
                }
                else if (text.Length == 5)
                {
                    string escape_text = text.Substring(1, text.Length - 1);
                    if (escape_text == "[[]" || escape_text == "[]]" || escape_text == "[&]" || escape_text == "[$]" || escape_text == "[#]" || escape_text == "[']")
                    {
                        value = escape_text.ToCharArray()[1];
                        return new Token(TokenType.CHARLITERAL, text, value, _line, line_col);
                    }
                    return new Token(TokenType.ERROR, text, "Invalid CHAR literal.", _line, line_col);
                }
                return new Token(TokenType.ERROR, text, "Invalid CHAR literal.", _line, line_col);
            }
            return new Token(TokenType.ERROR, _code.Substring(start, (_position - start)), "Invalid CHAR literal.", _line, line_col);
        }

        private Token GetBooleanOrStringLiteralToken()
        {
            int start = _position;
            int line_col = _column;

            Next();
            while (Current != '\"' && Current != '\n')
                Next();

            if (Current == '\"')
            {
                Next();
                int length = _position - start;
                string text = _code.Substring(start, length);

                if (text == "\"TRUE\"")
                    return new Token(TokenType.BOOLLITERAL, text, true, _line, line_col);
                else if (text == "\"FALSE\"")
                    return new Token(TokenType.BOOLLITERAL, text, false, _line, line_col);
                else
                    return new Token(TokenType.STRINGLITERAL, text, _code.Substring(start + 1, length - 2), _line, line_col);
            }

            int err_length = _position - start;
            string err_text = _code.Substring(start, err_length);

            if (err_text == "\"TRUE" || err_text == "\"FALSE")
                return new Token(TokenType.ERROR, err_text, "Invalid BOOL literal.", _line, line_col);
            return new Token(TokenType.ERROR, err_text, "Invalid STRING literal.", _line, line_col);
        }

        private Token GetNumberLiteralToken()
        {
            bool is_float = Current == '.' ? true : false;

            int start = _position;
            int line_col = _column;

            while (char.IsDigit(Current) || Current == '.')
            {
                Next();
                if (Current == '.')
                    if (is_float)
                        return new Token(TokenType.ERROR, _code.Substring(start, (_position - start)), "Invalid FLOAT literal.", _line, line_col);
                    else
                        is_float = true;
            }
            
            int length = _position - start;
            string text = _code.Substring(start, length);

            if (is_float)
            {
                if (!float.TryParse(text, out var float_value))
                    return new Token(TokenType.ERROR, text, "Invalid FLOAT literal.", _line, line_col);
                return new Token(TokenType.FLOATLITERAL, text, float_value, _line, line_col);
            }
            else
            {
                if (!int.TryParse(text, out var int_value))
                    return new Token(TokenType.ERROR, text, "Invalid INT literal.", _line, line_col);
                return new Token(TokenType.INTLITERAL, text, int_value, _line, line_col);
            }
            
        }

        private Token GetEscapeCodeToken()
        {
            int start = _position;
            int line_col = _column;
            
            while (Current != ']')
                Next();

            if (Current == ']')
            {
                Next();

                int length = _position - start;
                string text = _code.Substring(start, length);
                object value = null;

                if (Current == ']')
                {
                    Next();
                    length = _position - start;
                    text = _code.Substring(start, length);
                    value = null;
                }

                if (text == "[[]" || text == "[]]" || text == "[&]" || text == "[$]" || text == "[#]")
                {
                    value = text.ToCharArray()[1];
                    return new Token(TokenType.ESCAPE, text, value, _line, line_col);
                }
                return new Token(TokenType.ERROR, text, $"Invalid '{text}' as escape sequence.", _line, line_col);
            }
            return new Token(TokenType.ERROR, _code.Substring(start, (_position - start)), "Missing ]", _line, line_col);
        }
    }
}
