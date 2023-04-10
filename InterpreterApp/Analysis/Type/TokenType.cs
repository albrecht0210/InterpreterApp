using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterApp.Analysis.Type
{
    public enum TokenType
    {
        // Keyword
        BeginToken, EndToken, CodeToken, IntToken, FloatToken, CharToken, BoolToken,
        IfToken, ElseToken, WhileToken, DisplayToken, ScanToken, AndToken, OrToken, NotToken,

        // Identifier
        IdentifierToken,

        // Literal
        IntLiteralToken, FloatLiteralToken, CharLiteralToken, BoolLiteralToken, StringLiteralToken,

        // Symbol
        CommaToken, EqualsToken, ColonToken, QuoteToken, ApostropheToken, PoundToken,
        DollarToken, AmpersandToken, OpenBracketToken, CloseBracketToken,

        // Operators
        OpenParenthesisToken, CloseParenthesisToken, StarToken, SlashToken, PercentToken,
        PlusToken, MinusToken, GreaterThanToken, LessThanToken, GreaterEqualToken,
        LessEqualToken, EqualToToken, NotEqualToken,

        // Other
        WhiteSpaceToken, NewLineToken, TabToken, CommentToken, EscapeToken, ErrorToken, EndOfFileToken
    }
}
