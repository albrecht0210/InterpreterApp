using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterApp.Analysis.Tree.Expression;
using System.Diagnostics;
using InterpreterApp.Analysis.Type;
using InterpreterApp.Analysis.Syntax;
using InterpreterApp.Analysis.Table;
using System.Windows.Forms;
using System.Reflection.Emit;

namespace InterpreterApp.Analysis
{
    public class Interpreter
    {
        private VariableTable variable_table;
        private ProgramNode program;

        public Interpreter(string code)
        {
            Lexer lex = new Lexer(code);
            Parser parser = new Parser(lex);
            Semantic semantic = new Semantic();

            program = parser.ParseProgram();
            semantic.Analyze(program);
            variable_table = new VariableTable();
        }

        public void Execute()
        { 
            foreach (StatementNode statement in program.Statements)
            {
                switch (statement)
                {
                    case VariableDeclarationNode var_stmt:
                        ExecuteVariableDeclaration(var_stmt);
                        break;
                    case AssignmentNode assign_stmt:
                        ExecuteAssignment(assign_stmt);
                        break;
                    case DisplayNode display_stmt:
                        ExecuteDisplay(display_stmt);
                        break;
                    case ScanNode scan_stmt:
                        ExecuteScan(scan_stmt);
                        break;
                    case ConditionalNode cond_stmt:
                        ExecuteCondition(cond_stmt);
                        break;
                    case LoopNode loop_stmt:
                        ExecuteLoop(loop_stmt);
                        break;
                }
            }
        }

        private void ExecuteStatementBlock(ProgramNode statement_block)
        {
            foreach (StatementNode statement in statement_block.Statements)
            {
                switch (statement)
                {
                    case AssignmentNode assign_stmt:
                        ExecuteAssignment(assign_stmt);
                        break;
                    case DisplayNode display_stmt:
                        ExecuteDisplay(display_stmt);
                        break;
                    case ScanNode scan_stmt:
                        ExecuteScan(scan_stmt);
                        break;
                    case ConditionalNode cond_stmt:
                        ExecuteCondition(cond_stmt);
                        break;
                    case LoopNode loop_stmt:
                        ExecuteLoop(loop_stmt);
                        break;
                }
            }
        }

        // Start Execute Statements

        private void ExecuteVariableDeclaration(VariableDeclarationNode statement)
        {
            foreach (var variable in statement.Variables)
            {
                string identifier = variable.Key;
                if (variable_table.Exist(identifier))
                    throw new Exception($"({statement.Line},{statement.Column}): Variable '{identifier}' already exists.");
                object value = null;

                if (variable.Value != null)
                    value = EvaulateExpression(variable.Value);

                variable_table.AddVariable(identifier, Grammar.GetDataType(statement.Data_Type_Token), value);
            }
        }

        private void ExecuteAssignment(AssignmentNode statement)
        {
            List<string> identifiers = statement.Identifiers;
            object value;

            foreach (string identifier in identifiers)
            {
                if (variable_table.Exist(identifier))
                {
                    value = EvaulateExpression(statement.Expression);
                }
                else
                    throw new Exception($"({statement.Line},{statement.Column}): Variable '{identifier}' does not exists.");

                variable_table.AddValue(identifier, value);
            }
        }

        private void ExecuteDisplay(DisplayNode statement)
        {
            string result = "";
            foreach (var expression in statement.Expression)
            {
                if (expression.Value)
                    result += "\n";

                result += EvaulateExpression(expression.Key);
            }

            //result += "\n";
            Console.Write(result);
        }

        private void ExecuteScan(ScanNode statement)
        {
            List<string> values = new List<string>();
            List<string> identifiers = statement.Identifiers;
            string inputted = "";

            Console.Write("");
            inputted = Console.ReadLine();
            values = inputted.Replace(" ", "").Split(',').ToList();
            if (values.Count != identifiers.Count)
                throw new Exception($"({statement.Line}, {statement.Column}): Missing input/s.");

            object value;

            for (int i = 0; i < values.Count; i++)
            {
                if (variable_table.Exist(identifiers[i]))
                {
                    value = Grammar.GetProperValue(values[i]);
                    if (variable_table.GetType(identifiers[i]) != Grammar.GetDataType(value))
                        throw new Exception($"({statement.Line},{statement.Column}): Unable to assign {Grammar.GetDataType(value)} on \"{identifiers[i]}\".");
                }
                else
                    throw new Exception($"({statement.Line},{statement.Column}): Variable '{identifiers[i]}' does not exists.");

                variable_table.AddValue(identifiers[i], value);
            }
        }

        private void ExecuteCondition(ConditionalNode statement)
        {
            int disp_index = -1;
            for (int i = 0; i < statement.Expressions.Count; i++)
            {
                if (statement.Expressions[i] == null)
                {
                    disp_index = i;
                    break;
                }

                if ((bool)EvaulateExpression(statement.Expressions[i]))
                {
                    disp_index = i;
                    break;
                }
            }

            if (disp_index != -1)
                ExecuteStatementBlock(statement.Statements[disp_index]);
        }

        private void ExecuteLoop(LoopNode statement)
        {
            while ((bool)EvaulateExpression(statement.Expression))
                ExecuteStatementBlock(statement.Statement);
        }

        // End Execute Statements

        // Get the value of the expression
        private object EvaulateExpression(ExpressionNode expression)
        {
            switch(expression)
            {
                case BinaryNode binary_expr:
                    dynamic left = EvaulateExpression(binary_expr.Left);
                    dynamic right = EvaulateExpression(binary_expr.Right);
                    dynamic bin_result;

                    switch (binary_expr.Token_Operator.Token_Type)
                    {
                        case TokenType.PlusToken:
                            bin_result = left + right;
                            return bin_result;
                        case TokenType.MinusToken:
                            bin_result = left - right;
                            return bin_result;
                        case TokenType.StarToken:
                            bin_result = left * right;
                            return bin_result;
                        case TokenType.SlashToken:
                            bin_result = left / right;
                            return bin_result;
                        case TokenType.PercentToken:
                            bin_result = left % right;
                            return bin_result;
                        case TokenType.LessThanToken:
                            bin_result = left < right;
                            return bin_result;
                        case TokenType.GreaterThanToken:
                            bin_result = left > right;
                            return bin_result;
                        case TokenType.LessEqualToken:
                            bin_result = left <= right;
                            return bin_result;
                        case TokenType.GreaterEqualToken:
                            bin_result = left >= right  ;
                            return bin_result;
                        case TokenType.EqualToToken:
                            bin_result = left == right;
                            return bin_result;
                        case TokenType.NotEqualToken:
                            bin_result = left != right;
                            return bin_result;
                        case TokenType.AndToken:
                            bin_result = left && right;
                            return bin_result;
                        case TokenType.OrToken:
                            bin_result = left || right;
                            return bin_result;
                        //case TokenType.NotToken:
                        //    bin_result = left != right;
                        //    return result.ToString().ToUpper();
                        default:
                            throw new Exception($"({binary_expr.Line},{binary_expr.Column}): Unknown token.");
                    }
                case UnaryNode unary_expr:
                    dynamic unary_value = EvaulateExpression(unary_expr.Expression);
                    if (unary_expr.Token_Operator.Token_Type == TokenType.MinusToken)
                        return -unary_value;
                    return unary_value;
                case ParenthesisNode parenthesis_expr:
                    dynamic paren_expr = EvaulateExpression(parenthesis_expr.Expression);
                    return paren_expr;
                case IdentifierNode identifier_expr:
                    if (variable_table.GetValue(identifier_expr.Name) == null)
                        throw new Exception($"({identifier_expr.Line},{identifier_expr.Column}): {identifier_expr.Name} is null.");
                    object result = variable_table.GetValue(identifier_expr.Name);
                    if (result.GetType() == typeof(bool))
                        return (bool)result ? "TRUE" : "FALSE";
                    return result;
                case LiteralNode literal_expr:
                    return literal_expr.Literal;
                default:
                    throw new Exception("Unknown expression.");
            }
        }
    }
}
