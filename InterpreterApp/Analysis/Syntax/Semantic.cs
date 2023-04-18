using InterpreterApp.Analysis.Table;
using InterpreterApp.Analysis.Tree;
using InterpreterApp.Analysis.Tree.Expression;
using InterpreterApp.Analysis.Tree.Statement;
using InterpreterApp.Analysis.Type;
using System;
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
                    case DisplayNode display_stmt:
                        AnalyzeDisplay(display_stmt);
                        break;
                    case ScanNode scan_stmt:
                        AnalyzeScan(scan_stmt);
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
            // Get the data type of the statement
            DataType data_type = Grammar.GetDataType(statement.Data_Type_Token.Token_Type);

            // Loop through the dictionary of variables
            foreach (var variable in statement.Variables)
            {
                // Get the variable name
                string identifier = variable.Key;

                // If the identifier is not declared
                if (!variable_table.Exist(identifier))
                {
                    // If the variable value is not null ex. INT a = 5
                    if (variable.Value != null)
                    {
                        // Get the data type of the expression
                        DataType data_expression_type = AnalyzeExpression(variable.Value);

                        // If the data type of the statement and expression is not same
                        if (!Grammar.MatchDataType(data_type, data_expression_type))
                            throw new Exception($"({statement.Data_Type_Token.Line},{statement.Data_Type_Token.Column}): Unable to assign {data_expression_type} on \"{variable.Key}\".");
                    }
                    // Add the identifier and data type to the table of variables
                    variable_table.AddIdentifier(identifier, data_type);
                }
                // If the identifier is already declared
                else
                    throw new Exception($"({statement.Data_Type_Token.Line},{statement.Data_Type_Token.Column}): Variable \"{variable}\" already exists.");
                
            }
        }

        private void AnalyzeAssignment(AssignmentNode statement)
        {
            int index = 0;
            // Loop through a list of identifiers
            foreach (string identifier in statement.Identifiers)
            {
                // If identifier is already declared
                if (variable_table.Exist(identifier))
                {
                    // Get the type of the variable using the table of variables
                    DataType data_type = variable_table.GetType(identifier);
                    // Get the data type of the expression
                    DataType data_expression_type = AnalyzeExpression(statement.Expression);

                    // If the data type of the statement and expression is not same
                    if (!Grammar.MatchDataType(data_type, data_expression_type))
                        throw new Exception($"({statement.Equals_Token[index].Line},{statement.Equals_Token[index].Column}): Unable to assign {data_expression_type} on \"{identifier}\".");
                }
                // If the identifier is not declared
                else
                    throw new Exception($"({statement.Equals_Token[index].Line},{statement.Equals_Token[index].Column}): Variable \"{identifier}\" does not exists.");

                index++;
            }
        }

        private void AnalyzeDisplay(DisplayNode statement)
        {
            // Loop through a list of expression
            foreach (var expression in statement.Expressions)
            {
                // Only check the identifier expression
                switch (expression)
                {
                    case IdentifierNode iden_expr:
                        // If the identifier is not declared
                        if (!variable_table.Exist(iden_expr.Name))
                            throw new Exception($"({iden_expr.Identifier_Token.Line},{iden_expr.Identifier_Token.Column}): Variable \"{iden_expr.Name}\" does not exists.");
                        break;
                }
            }
        }

        private void AnalyzeScan(ScanNode statement)
        {
            // Loop through a list of identifiers
            foreach (var identifier in statement.Identifiers)
            {
                // If the identifier is not declared
                if (!variable_table.Exist(identifier))
                    throw new Exception($"({statement.Scan_Token.Line},{statement.Scan_Token.Column}): Variable \"{identifier}\" does not exists.");
            }
        }

        private void AnalyzeCondition(ConditionalNode statement)
        {
            int index = 0;
            // Loop through a list of expressions
            foreach (ExpressionNode expression in statement.Expressions)
            {
                // If expression is not null means that the statement is either if or else if
                if (expression != null)
                {
                    // If the data type of the expression is not bool
                    if (AnalyzeExpression(expression) != DataType.Bool)
                        throw new Exception($"({statement.Tokens[index].Line},{statement.Tokens[index].Column}): Expression is not {DataType.Bool}");
                }

                // Analyze the statement block of if, else if, else statement
                Analyze(statement.Statements[index]);

                index++;
            }
        }

        private void AnalyzeLoop(LoopNode statement)
        {
            // If the data type of the expression is not bool
            if (AnalyzeExpression(statement.Expression) != DataType.Bool)
                throw new Exception($"({statement.While_Token.Line},{statement.While_Token.Column}): Expression is not {DataType.Bool}");

            // Analyze the statement block of while statement
            Analyze(statement.Statement);
        }
        // End Analyze Statements

        // Start Analyze Expression
        private DataType AnalyzeExpression(ExpressionNode expression)
        {
            switch (expression)
            {
                case BinaryNode bin_expr:
                    return AnalyzeBinaryExpression(bin_expr);

                case UnaryNode unary_expr:
                    return AnalyzeUnaryExpression(unary_expr);

                case ParenthesisNode paren_expr:
                    return AnalyzeExpression(paren_expr.Expression);

                case IdentifierNode iden_expr:
                    return AnalyzeIdentifierExpression(iden_expr);

                case LiteralNode literal_expr:
                    return AnalyzeLiteralExpression(literal_expr);

                default:
                    throw new Exception($"Unknown expression.");
            }
        }
    
        private DataType AnalyzeBinaryExpression(BinaryNode expression)
        {
            // Get the operator token from binary node
            Token operator_token = expression.Token_Operator;
            // Get the data type for the left expression
            DataType left_dt = AnalyzeExpression(expression.Left);
            // Get the data type for the right expression
            DataType right_dt = AnalyzeExpression(expression.Right);

            // If data type of left and right is not same
            if (!MatchExpressionDataType(left_dt, right_dt))
                throw new Exception($"({operator_token.Line},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");

            // If the operator token is a arithmetic operator but left_dt or right_dt is not int or float 
            if (Grammar.IsArithmeticOperator(operator_token.Token_Type)
                && ((left_dt == DataType.Char || left_dt == DataType.String || left_dt == DataType.Bool)
                && (right_dt == DataType.Char || right_dt == DataType.String || right_dt == DataType.Bool)))
                throw new Exception($"({operator_token.Line},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");
            // If the operator token is a comparison operator but left_dt or right_dt is not the same
            else if (Grammar.IsComparisonOperator(operator_token.Token_Type)
                && !MatchExpressionDataType(left_dt, right_dt))
                throw new Exception($"({operator_token.Line},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to operands of type {left_dt} and {right_dt}");
            
            // If the operator token is a logical operator
            if (Grammar.IsComparisonOperator(operator_token.Token_Type))
                return DataType.Bool;
            else
                return left_dt;
        }
        
        private DataType AnalyzeUnaryExpression(UnaryNode expression)
        {
            // Get the operator token from umary node
            Token operator_token = expression.Token_Operator;
            // Get the data type for the expression
            DataType expression_data_type = AnalyzeExpression(expression.Expression);

            // If the operator token is the not logical operator
            if (operator_token.Token_Type == TokenType.NOT)
            {
                // If data type of expression is not bool
                if (expression_data_type != DataType.Bool)
                    throw new Exception($"({operator_token.Line},{operator_token.Column}): Operator '{operator_token.Code}' cannot be applied to {expression_data_type}");
                // If data type of expression is bool
                return DataType.Bool;
            }

            // return the data type of the expression
            return expression_data_type;
        }

        private DataType AnalyzeIdentifierExpression(IdentifierNode expression)
        {
            // If the identifier is not declared
            if (!variable_table.Exist(expression.Name))
                throw new Exception($"({expression.Identifier_Token.Line},{expression.Identifier_Token.Column}): Variable \"{expression.Name}\" does not exists.");

            return variable_table.GetType(expression.Name);
        }
    
        private DataType AnalyzeLiteralExpression(LiteralNode expression)
        {
            object val = expression.Literal;
            
            // Get the type of the value
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
                throw new Exception($"({expression.Literal_Token.Line},{expression.Literal_Token.Column}): Unknown data type {expression.Literal}");
        }

        private bool MatchExpressionDataType(DataType ldt, DataType rdt)
        {
            if ((ldt == DataType.Int && rdt == DataType.Float) || (ldt == DataType.Float && rdt == DataType.Int))
                return true;
            return ldt == rdt;
        }
    }
}
