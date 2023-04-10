using InterpreterApp.Analysis.Table;
using InterpreterApp.Analysis.Tree;
using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Type;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using DataType = InterpreterApp.Analysis.Type.DataType;

namespace InterpreterApp.Analysis.Syntax
{
    public class Semantic
    {
        private VariableTable variable_table;

        public Semantic()
        {
            variable_table = new VariableTable();
        }

        public void Analyze(ProgramNode program)
        {
            foreach (StatementNode statement in program.Statements)
            {
                switch (statement)
                {
                    case VariableDeclarationNode var_stmt:
                        AnalyzeVariableDeclaration(var_stmt);
                        break;
                    case AssignmentNode assign_stmt:
                        AnalyzeAssignment(assign_stmt);
                        break;
                    case ConditionalNode cond_stmt:
                        AnalyzeCondition(cond_stmt);
                        break;
                    case LoopNode loop_stmt:
                        AnalyzeLoop(loop_stmt);
                        break;
                }
            }
        }

        // Start Analyze Statements

        // No need to analyze display and scan statements

        private void AnalyzeVariableDeclaration(VariableDeclarationNode statement)
        {
            DataType data_type = Grammar.GetDataType(statement.Data_Type_Token);
            foreach (var variable in statement.Variables)
            {
                string identifier = variable.Key;

                if (!variable_table.Exist(identifier))
                {
                    if (variable.Value != null)
                    {
                        DataType data_expression_type = AnalyzeExpression(variable.Value);

                        if (data_type != data_expression_type)
                            throw new Exception($"({statement.Line},{statement.Column}): Unable to assign {data_expression_type} on \"{variable.Key}\".");
                    }
                    variable_table.AddIdentifier(identifier, data_type);
                }
                else
                    throw new Exception($"({statement.Line},{statement.Column}): \"{variable}\" already exists.");
                
            }
        }

        private void AnalyzeAssignment(AssignmentNode statement)
        {
            List<string> identifiers = statement.Identifiers;
            foreach (string identifier in identifiers)
            {
                if (variable_table.Exist(identifier))
                {
                    DataType data_type = variable_table.GetType(identifier);
                    DataType data_expression_type = AnalyzeExpression(statement.Expression);

                    if (data_type != data_expression_type)
                        throw new Exception($"({statement.Line},{statement.Column}): Unable to assign {data_expression_type} on \"{identifier}\".");
                }
                else
                    throw new Exception($"({statement.Line},{statement.Column}): \"{identifier}\" does not exists.");
            }

        }

        private void AnalyzeCondition(ConditionalNode statement)
        {
            for (int i = 0; i < statement.Expressions.Count; i++)
            {
                if (statement.Expressions[i] != null)
                {
                    if (AnalyzeExpression(statement.Expressions[i]) != DataType.Bool)
                        throw new Exception($"({statement.Lines[i]},{statement.Columns[i]}): Expression is not {DataType.Bool}");
                }
            }
        }

        private void AnalyzeLoop(LoopNode statement)
        {
            if (AnalyzeExpression(statement.Expression) != DataType.Bool)
                throw new Exception($"({statement.Line},{statement.Column}): Expression is not {DataType.Bool}");
        }

        // End Analyze Statements

        // Analyze Expression and return what data type

        private DataType AnalyzeExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case BinaryNode bin_expr:
                    Token token = bin_expr.Token_Operator;
                    DataType left_dt = AnalyzeExpression(bin_expr.Left);
                    DataType right_dt = AnalyzeExpression(bin_expr.Right);

                    if (left_dt != right_dt)
                        throw new Exception($"({token.Line},{token.Column}): Unable to operate '{token.Code}' on {left_dt} and {right_dt}.");

                    if (Grammar.IsLogicalOperator(token.Token_Type))
                        return DataType.Bool;

                    return left_dt;
                case UnaryNode unary_expr:
                    DataType unary_dt = AnalyzeExpression(unary_expr.Expression);

                    return unary_dt;
                case ParenthesisNode paren_expr:
                    DataType parenthesis_dt = AnalyzeExpression(paren_expr.Expression);
                    
                    return parenthesis_dt;
                case IdentifierNode iden_expr:
                    if (!variable_table.Exist(iden_expr.Name))
                        throw new Exception($"({iden_expr.Line},{iden_expr.Column}): \"{iden_expr.Name}\" does not exists.");

                    return variable_table.GetType(iden_expr.Name);
                case LiteralNode literal_expr:
                    object val = literal_expr.Literal;
                    if (val.GetType() == typeof(int))
                        return DataType.Int;
                    else if (val.GetType() == typeof(float) || val.GetType() == typeof(double))
                        return DataType.Float;
                    else if (val.GetType() == typeof(char))
                        return DataType.Char;
                    else if (val.GetType() == typeof(bool))
                        return DataType.Bool;
                    else if (val.GetType() == typeof(string))
                        return DataType.String;
                    else
                        throw new Exception($"Unknown data type.");
                default:
                    throw new Exception($"Unknown expression.");
            }
        }
    }
}
