using InterpreterApp.Analysis.Tree;
using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Type;

namespace InterpreterApp.Analysis.Syntax
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _current_token;
        private int _cond_tabs;

        public Parser(Lexer lexer)
        {
            this._lexer = lexer;
            this._current_token = lexer.GetToken();
            this._cond_tabs = 0;
        }

        // Parse the whole code
        public ProgramNode ParseProgram()
        {
            while (MatchToken(TokenType.CommentToken))
            {
                ConsumeToken(TokenType.CommentToken);
                ConsumeToken(TokenType.NewLineToken);
            }

            while (MatchToken(TokenType.NewLineToken))
                ConsumeToken(TokenType.NewLineToken);

            ConsumeToken(TokenType.BeginToken);
            ConsumeToken(TokenType.CodeToken);
            ConsumeToken(TokenType.NewLineToken);

            List<StatementNode> statements = ParseStatements();

            ConsumeToken(TokenType.EndToken);
            ConsumeToken(TokenType.CodeToken);

            while (MatchToken(TokenType.NewLineToken))
                ConsumeToken(TokenType.NewLineToken);

            ConsumeToken(TokenType.EndOfFileToken);

            return new ProgramNode(statements);
        }

        // Parse the if block
        private ProgramNode ParseIfProgram()
        {
            for (int i = 0; i < _cond_tabs; i++)
                ConsumeToken(TokenType.TabToken);

            ConsumeToken(TokenType.TabToken);
            ConsumeToken(TokenType.BeginToken);
            ConsumeToken(TokenType.IfToken);
            ConsumeToken(TokenType.NewLineToken);

            this._cond_tabs++;

            List<StatementNode> statements = ParseStatements();

            this._cond_tabs--;

            ConsumeToken(TokenType.EndToken);
            ConsumeToken(TokenType.IfToken);
            ConsumeToken(TokenType.NewLineToken);

            return new ProgramNode(statements);
        }

        // Parse the while block
        private ProgramNode ParseWhileProgram()
        {
            for (int i = 0; i < _cond_tabs; i++)
                ConsumeToken(TokenType.TabToken);

            ConsumeToken(TokenType.TabToken);
            ConsumeToken(TokenType.BeginToken);
            ConsumeToken(TokenType.WhileToken);
            ConsumeToken(TokenType.NewLineToken);

            this._cond_tabs++;

            List<StatementNode> statements = ParseStatements();

            this._cond_tabs--;

            ConsumeToken(TokenType.EndToken);
            ConsumeToken(TokenType.WhileToken);
            ConsumeToken(TokenType.NewLineToken);

            return new ProgramNode(statements);
        }


        // Start Parse Statements

        private List<StatementNode> ParseStatements()
        {
            List<StatementNode> statement_list = new List<StatementNode>();


            while (!MatchToken(TokenType.EndToken))
            {
                while (MatchToken(TokenType.NewLineToken))
                    ConsumeToken(TokenType.NewLineToken);

                int i = 0;
                do
                {
                    ConsumeToken(TokenType.TabToken);
                    i++;
                } while (i < _cond_tabs);

                if (MatchToken(TokenType.IntToken) || MatchToken(TokenType.FloatToken) ||
                    MatchToken(TokenType.CharToken) || MatchToken(TokenType.BoolToken))
                {
                    //Debug.WriteLine("IS PARSE VARIABLE DECLARATION");
                    statement_list.Add(ParseVariableDeclarationStatement());
                }
                else if (MatchToken(TokenType.IdentifierToken))
                {
                    //Debug.WriteLine("IS PARSE ASSIGNMENT");
                    statement_list.Add(ParseAssignmentStatement());
                }
                else if (MatchToken(TokenType.DisplayToken))
                {
                    //Debug.WriteLine("IS PARSE DISPLAY");
                    statement_list.Add(ParseDisplayStatement());
                }
                else if (MatchToken(TokenType.CommentToken))
                {
                    //Debug.WriteLine("IS PARSE COMMENT");
                    ConsumeToken(TokenType.CommentToken);
                    ConsumeToken(TokenType.NewLineToken);
                }
                else if (MatchToken(TokenType.ScanToken))
                {
                    //Debug.WriteLine("IS PARSE SCAN");
                    statement_list.Add(ParseScanStatement());

                }
                else if (MatchToken(TokenType.IfToken))
                {
                    //Debug.WriteLine("IS PARSE IF");
                    statement_list.Add(ParseIfStatement());
                }
                else if (MatchToken(TokenType.WhileToken))
                {
                    //Debug.WriteLine("IS PARSE IF");
                    statement_list.Add(ParseWhileStatement());
                }
                else if (MatchToken(TokenType.EndToken))
                    break;
                else if (MatchToken(TokenType.EndOfFileToken))
                    throw new Exception($"({_current_token.Line},{_current_token.Column}): Missing End.");
                else
                    throw new Exception($"({_current_token.Line},{_current_token.Column}): Invalid syntax \"{_current_token.Code}\".");
            }

            return statement_list;
        }

        private StatementNode ParseVariableDeclarationStatement()
        {
            Token data_type_token = _current_token;
            ConsumeToken(data_type_token.Token_Type);

            Dictionary<string, ExpressionNode> variables = new Dictionary<string, ExpressionNode>();

            (string, ExpressionNode) variable = GetVariable();

            variables.Add(variable.Item1, variable.Item2);

            while (MatchToken(TokenType.CommaToken))
            {
                ConsumeToken(TokenType.CommaToken);
                variable = GetVariable();
                variables.Add(variable.Item1, variable.Item2);
            }

            ConsumeToken(TokenType.NewLineToken);

            return new VariableDeclarationNode(data_type_token.Token_Type, variables, data_type_token.Line, data_type_token.Column);
        }

        private StatementNode ParseAssignmentStatement()
        {
            List<string> identifiers = new List<string>();
            identifiers.Add(_current_token.Code);
            Token identifier_token = _current_token;
            
            ConsumeToken(TokenType.IdentifierToken);
            ConsumeToken(TokenType.EqualsToken);

            ExpressionNode expression_value = ParseExpression();
            
            while (MatchToken(TokenType.EqualsToken))
            {
                IdentifierNode id_expr = (IdentifierNode)expression_value;
                identifiers.Add(id_expr.Name);
                ConsumeToken(TokenType.EqualsToken);
                expression_value = ParseExpression();
            }
            ConsumeToken(TokenType.NewLineToken);

            return new AssignmentNode(identifiers, expression_value, identifier_token.Line, identifier_token.Column);
        }

        private StatementNode ParseDisplayStatement()
        {
            Token display = _current_token;
            ConsumeToken(TokenType.DisplayToken);
            ConsumeToken(TokenType.ColonToken);

            Dictionary<ExpressionNode, bool> expressions = new Dictionary<ExpressionNode, bool>();
            bool is_newline = false;

            if (MatchToken(TokenType.DollarToken))
            {
                ConsumeToken(TokenType.DollarToken);
                is_newline = true;
                if (MatchToken(TokenType.AmpersandToken))
                    ConsumeToken(TokenType.AmpersandToken);

                if (MatchToken(TokenType.NewLineToken))
                {
                    expressions.Add(new LiteralNode("", display.Line, display.Column), true);
                    ConsumeToken(TokenType.NewLineToken);
                    return new DisplayNode(expressions, display.Line, display.Column);
                }
            }

            expressions.Add(ParseExpression(), is_newline);

            is_newline = false;

            while (MatchToken(TokenType.AmpersandToken))
            {
                ConsumeToken(TokenType.AmpersandToken);

                if (MatchToken(TokenType.DollarToken))
                {
                    ConsumeToken(TokenType.DollarToken);
                    if (MatchToken(TokenType.AmpersandToken))
                    {
                        ConsumeToken(TokenType.AmpersandToken);
                        expressions.Add(ParseExpression(), true);
                    }

                    if (MatchToken(TokenType.NewLineToken))
                        expressions.Add(new LiteralNode("", display.Line, display.Column), true);
                }
                else
                    expressions.Add(ParseExpression(), false);
            }

            ConsumeToken(TokenType.NewLineToken);

            return new DisplayNode(expressions, display.Line, display.Column);
        }

        private StatementNode ParseScanStatement()
        {
            Token scan = _current_token;
            ConsumeToken(TokenType.ScanToken);
            ConsumeToken(TokenType.ColonToken);

            List<string> identifiers = new List<string>();
            identifiers.Add(_current_token.Code);
            ConsumeToken(TokenType.IdentifierToken);

            while (MatchToken(TokenType.CommaToken))
            {
                ConsumeToken(TokenType.CommaToken);
                identifiers.Add(_current_token.Code);
                ConsumeToken(TokenType.IdentifierToken);
            }

            ConsumeToken(TokenType.NewLineToken);

            return new ScanNode(identifiers, scan.Line, scan.Column);
        }

        private StatementNode ParseIfStatement()
        {
            List<ExpressionNode> conditions = new List<ExpressionNode>();
            List<ProgramNode> statement_blocks = new List<ProgramNode>();
            List<int> lines = new List<int>();
            List<int> columns = new List<int>();

            lines.Add(_current_token.Line);
            columns.Add(_current_token.Column);
            ConsumeToken(TokenType.IfToken);

            conditions.Add(ParseExpression());

            ConsumeToken(TokenType.NewLineToken);

            statement_blocks.Add(ParseIfProgram());
            
            for (int i = 0; i < _cond_tabs; i++)
                ConsumeToken(TokenType.TabToken);

            if (MatchToken(TokenType.TabToken))
                ConsumeToken(TokenType.TabToken);

            while (MatchToken(TokenType.ElseToken))
            {
                if (MatchToken(TokenType.TabToken))
                    ConsumeToken(TokenType.TabToken);
                lines.Add(_current_token.Line);
                columns.Add(_current_token.Column);
                ConsumeToken(TokenType.ElseToken);

                if (MatchToken(TokenType.IfToken))
                {
                    ConsumeToken(TokenType.IfToken);
                    conditions.Add(ParseExpression());
                }
                else
                    conditions.Add(null);

                ConsumeToken(TokenType.NewLineToken);
        
                statement_blocks.Add(ParseIfProgram());

                for (int i = 0; i < _cond_tabs; i++)
                    ConsumeToken(TokenType.TabToken);
            }

            return new ConditionalNode(conditions, statement_blocks, lines, columns);
        }

        private StatementNode ParseWhileStatement()
        {
            Token while_token = _current_token;
            ConsumeToken(TokenType.WhileToken);

            ExpressionNode condition = ParseExpression();

            ConsumeToken(TokenType.NewLineToken);

            ProgramNode statement_block = ParseWhileProgram();

            return new LoopNode(condition, statement_block, while_token.Line, while_token.Column);
        }

        // End Parse Statements

        // Start Parse Expressions

        private ExpressionNode ParseExpression()
        {
            ExpressionNode expression;
            if (MatchToken(TokenType.EscapeToken))
            {
                Token token = _current_token;
                ConsumeToken(TokenType.EscapeToken);
                return new LiteralNode(token.Value, token.Line, token.Column);
            }
            else if (MatchToken(TokenType.OpenParenthesisToken))
            {
                expression = ParseParenthesisExpression();
                return expression;
            }
            else if (MatchToken(TokenType.PlusToken) || MatchToken(TokenType.MinusToken) || MatchToken(TokenType.NotToken))
            {
                expression = ParseUnaryExpression();
                return expression;
            } 
            else if (MatchToken(TokenType.IdentifierToken) || MatchToken(TokenType.IntLiteralToken) || MatchToken(TokenType.FloatLiteralToken) ||
                MatchToken(TokenType.CharLiteralToken) || MatchToken(TokenType.BoolLiteralToken) || MatchToken(TokenType.StringLiteralToken))
            {
                expression = ParseBinaryExpression();
                return expression;
            }
            else
                throw new Exception($"({_current_token.Line}, {_current_token.Column}): Invalid syntax \"{_current_token.Code}\".");
        }
        
        private ExpressionNode ParseParenthesisExpression()
        {
            Token open_parenthesis = _current_token;
            ConsumeToken(TokenType.OpenParenthesisToken);

            ExpressionNode expression = ParseExpression();

            Token close_parenthesis = _current_token;
            ConsumeToken(TokenType.CloseParenthesisToken);

            int precedence = Grammar.GetBinaryPrecedence(_current_token.Token_Type);

            if (precedence > 0)
            {
                ParenthesisNode paren_expr = new ParenthesisNode(open_parenthesis, expression, close_parenthesis, open_parenthesis.Line, open_parenthesis.Column);
                return ParseBinaryExpression(paren_expr);

            }
            return new ParenthesisNode(open_parenthesis, expression, close_parenthesis, open_parenthesis.Line, open_parenthesis.Column);
        }

        private ExpressionNode ParseUnaryExpression()
        {
            Token unary = _current_token;
            ConsumeToken(unary.Token_Type);

            ExpressionNode expression = ParseExpression();

            return new UnaryNode(unary, expression, unary.Line, unary.Column);
        }

        private ExpressionNode ParseBinaryExpression(ExpressionNode prev_left = null)
        {
            ExpressionNode left;
            if (prev_left != null)
                left = prev_left;
            else
                left = ParseTerm();

            int precedence = Grammar.GetBinaryPrecedence(_current_token.Token_Type);

            while (precedence > 0)
            {
                if (MatchToken(TokenType.PlusToken) || MatchToken(TokenType.MinusToken) ||
                    MatchToken(TokenType.StarToken) || MatchToken(TokenType.SlashToken) ||
                    MatchToken(TokenType.LessThanToken) || MatchToken(TokenType.GreaterThanToken) ||
                    MatchToken(TokenType.LessEqualToken) || MatchToken(TokenType.GreaterEqualToken) ||
                    MatchToken(TokenType.EqualToToken) || MatchToken(TokenType.NotEqualToken) ||
                    MatchToken(TokenType.AndToken) || MatchToken(TokenType.OrToken) || MatchToken(TokenType.NotToken))
                {
                    Token binary = _current_token;
                    ConsumeToken(binary.Token_Type);
                    ExpressionNode right = ParseTerm();
                    int next_precedence = Grammar.GetBinaryPrecedence(_current_token.Token_Type);
                    if (next_precedence >= precedence)
                        right = ParseBinaryExpression(right);
                    left = new BinaryNode(left, binary, right, binary.Line, binary.Column);
                    precedence = Grammar.GetBinaryPrecedence(_current_token.Token_Type);
                }
            }
            return left;
        }

        private ExpressionNode ParseTerm()
        {
            if (MatchToken(TokenType.IdentifierToken))
            {
                Token token = _current_token;
                ConsumeToken(TokenType.IdentifierToken);
                return new IdentifierNode(token.Code, token.Line, token.Column);
            }
            else if (MatchToken(TokenType.IntLiteralToken) || MatchToken(TokenType.FloatLiteralToken) ||
                    MatchToken(TokenType.CharLiteralToken) || MatchToken(TokenType.BoolLiteralToken) ||
                    MatchToken(TokenType.StringLiteralToken))
            {
                Token token = _current_token;
                ConsumeToken(token.Token_Type);
                return new LiteralNode(token.Value, token.Line, token.Column);
            }
            else if (MatchToken(TokenType.OpenParenthesisToken))
                return ParseParenthesisExpression();
            else if (MatchToken(TokenType.MinusToken) || MatchToken(TokenType.PlusToken))
                return ParseUnaryExpression();
            else
                throw new Exception($"({_current_token.Line}, {_current_token.Column}): Invalid syntax \"{_current_token.Code}\".");
        }

        // End Parse Expressions

        // Get next token
        private void ConsumeToken(TokenType token_type)
        {
            if (MatchToken(token_type))
            {
                _current_token = _lexer.GetToken();
                if (MatchToken(TokenType.ErrorToken)) 
                    throw new Exception($"({_current_token.Line}, {_current_token.Column}): Invalid syntax \"{_current_token.Code}\".");
            }
            else
            {
                if (token_type == TokenType.TabToken)
                    throw new Exception($"({_current_token.Line},{_current_token.Column}): Incorrect Identation");
                throw new Exception($"({_current_token.Line},{_current_token.Column}): Unexpected token {_current_token.Token_Type} expected {token_type}");
            }
        }

        // Match current to param {token_type}
        private bool MatchToken(TokenType token_type)
        {
            return _current_token.Token_Type == token_type;
        }

        // Helper method for variable declaration
        private (string, ExpressionNode) GetVariable()
        {
            Token identifier = _current_token;
            Token value;

            ConsumeToken(TokenType.IdentifierToken);

            if (MatchToken(TokenType.EqualsToken))
            {
                ConsumeToken(TokenType.EqualsToken);
                return (identifier.Code, ParseExpression());
            }

            return (identifier.Code, null);
        }

    }
}
