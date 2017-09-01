using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpParser.Parsing
{
    public class ExpressionParser
    {
        #region Recognized Tokens and Lists
        //Recognized Operators
        /// <summary>
        /// The addition operator.
        /// </summary>
        private static readonly OperatorToken ADD;

        /// <summary>
        /// The subtraction operator.
        /// </summary>
        private static readonly OperatorToken SUB;

        /// <summary>
        /// The multiplication operator.
        /// </summary>
        private static readonly OperatorToken MLT;

        /// <summary>
        /// The division operator.
        /// </summary>
        private static readonly OperatorToken DIV;

        /// <summary>
        /// The modulus operator.
        /// </summary>
        private static readonly OperatorToken MOD;

        /// <summary>
        /// The negation operator.
        /// </summary>
        private static readonly OperatorToken NEG;

        /// <summary>
        /// The exponentiation operator.
        /// </summary>
        private static readonly OperatorToken EXP;

        /// <summary>
        /// The factorial operator.
        /// </summary>
        private static readonly OperatorToken FAC;

        //Recognized Functions
        /// <summary>
        /// The sine function for radians.
        /// </summary>
        private static readonly FunctionToken F_SIN;

        /// <summary>
        /// The cosine function for radians.
        /// </summary>
        private static readonly FunctionToken F_COS;

        /// <summary>
        /// The tangent function for radians.
        /// </summary>
        private static readonly FunctionToken F_TAN;

        /// <summary>
        /// Rounds a single number to the nearest integer.
        /// </summary>
        private static readonly FunctionToken F_ROUND;

        /// <summary>
        /// Rounds a number to the nearest multiple of another.
        /// </summary>
        private static readonly FunctionToken F_ROUND2;

        //Recognized Identifiers
        /// <summary>
        /// The mathematical constant, Pi.
        /// </summary>
        private static readonly IdentifierToken PI;

        //Token Lists
        /// <summary>
        /// A mutable list of all operators to parse with.
        /// </summary>
        private static List<OperatorToken> operators;

        /// <summary>
        /// A mutable list of all functions to parse with.
        /// </summary>
        private static List<FunctionToken> functions;

        /// <summary>
        /// A mutable list of all identifiers to parse with.
        /// </summary>
        private static List<IdentifierToken> identifiers;
        #endregion

        #region Static Constructor
        /// <summary>
        /// Sets all default values for all statically-accessible items.
        /// </summary>
        static ExpressionParser()
        {
            //Sets default operators.
            FAC = new OperatorToken(TokenOpPlacement.Left,
                TokenOpAssociativity.Left, 6, "!",
                new Func<decimal[], decimal>((num) =>
                {
                    decimal value = 1;

                    while (num[0] > 1)
                    {
                        value *= num[0]--;
                    }

                    return value;
                }));

            EXP = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Right, 5, "^",
                new Func<decimal[], decimal>((num) =>
                {
                    try
                    {
                        return checked((decimal)Math.Pow(
                            (double)num[0], (double)num[1]));
                    }
                    catch (OverflowException e)
                    {
                        throw new ParsingException(num[0] + " ^ " + num[1] +
                            " is too large to compute.", e);
                    }
                }));

            NEG = new OperatorToken(TokenOpPlacement.Right,
                TokenOpAssociativity.Right, 4, "-",
                new Func<decimal[], decimal>((num) =>
                {
                    return -num[0];
                }));

            MOD = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Left, 3, "%",
                new Func<decimal[], decimal>((num) =>
                {
                    if (num[1] == 0)
                    {
                        throw new ParsingException("The expression " +
                            num[0] + " % " + num[1] +
                            " causes division by zero.");
                    }

                    return num[0] % num[1];
                }));

            DIV = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Left, 3, "/",
                new Func<decimal[], decimal>((num) =>
                {
                    if (num[1] == 0)
                    {
                        throw new ParsingException("The expression " +
                            num[0] + " / " + num[1] +
                            " causes division by zero.");
                    }

                    return num[0] / num[1];
                }));

            MLT = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Left, 3, "*",
                new Func<decimal[], decimal>((num) =>
                {
                    return num[0] * num[1];
                }));

            SUB = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Left, 2, "-",
                new Func<decimal[], decimal>((num) =>
                {
                    return num[0] - num[1];
                }));

            ADD = new OperatorToken(TokenOpPlacement.Both,
                TokenOpAssociativity.Left, 2, "+",
                new Func<decimal[], decimal>((num) =>
                {
                    return num[0] + num[1];
                }));

            operators = new List<OperatorToken>()
            {
                FAC, EXP, NEG, MOD, DIV, MLT, SUB, ADD
            };

            //Sets default functions.
            F_SIN = new FunctionToken("sin", 1,
                new Func<decimal[], decimal>((num) =>
                {
                    return (decimal)Math.Sin((double)num[0]);
                }));

            F_COS = new FunctionToken("cos", 1,
                new Func<decimal[], decimal>((num) =>
                {
                    return (decimal)Math.Cos((double)num[0]);
                }));

            F_TAN = new FunctionToken("tan", 1,
                new Func<decimal[], decimal>((num) =>
                {
                    return (decimal)Math.Tan((double)num[0]);
                }));

            F_ROUND = new FunctionToken("round", 1,
                new Func<decimal[], decimal>((num) =>
                {
                    return Math.Round(num[0]);
                }));

            F_ROUND2 = new FunctionToken("round", 2,
                new Func<decimal[], decimal>((num) =>
                {
                    return Math.Round(num[0] / num[1]) * num[1];
                }));

            functions = new List<FunctionToken>()
            {
                F_SIN, F_COS, F_TAN, F_ROUND, F_ROUND2
            };

            //Sets default identifiers.
            PI = new IdentifierToken("pi", (decimal)Math.PI);

            identifiers = new List<IdentifierToken>()
            {
                PI
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses a user-entered infix mathematical expression.
        /// </summary>
        /// <param name="expression">
        /// An expression composed of numbers and included basic operators
        /// and functions. No implicit multiplication.
        /// </param>
        public static decimal ParseExpression(string expression)
        {
            decimal result = 0;

            //Catches null or whitespace strings.
            if (String.IsNullOrWhiteSpace(expression))
            {
                throw new ParsingException("No expression provided.");
            }

            //Removes all whitespace and makes expression lowercase.
            expression = Regex.Replace(expression, @"\s", "").ToLower();

            //Solves each parenthesis group from deepest depth outward.
            while (true)
            {
                //Finds the end of the nearest complete sub-expression.
                int rbrPos = expression.IndexOf(')') + 1;
                int subExpressionEnd = (rbrPos >= 1) ? rbrPos : (expression.Length);

                //Finds the start of the nearest complete sub-expression.
                int lbrPos = expression.Substring(0, subExpressionEnd).LastIndexOf('(');
                int subExpressionBegin = (lbrPos >= 0) ? lbrPos : 0;

                //Isolates the sub-expression.
                string expressionLHS = expression.Substring(0, subExpressionBegin);
                string expressionRHS = expression.Substring(subExpressionEnd);
                string subExpression = expression.Substring(subExpressionBegin,
                    subExpressionEnd - subExpressionBegin);

                //Includes functions from sub-expressions.
                FunctionToken subExpressionFunc = null;
                for (int i = 0; i < functions.Count; i++)
                {
                    FunctionToken candidate = functions[i];

                    if (expressionLHS.EndsWith(candidate.Format))
                    {
                        expressionLHS = expressionLHS.Substring(0,
                            expressionLHS.Length - candidate.Format.Length);

                        subExpressionFunc = candidate;
                        break;
                    }
                }

                //Evaluates sub-expressions.
                expression = expressionLHS +
                    ParseSubExpression(subExpression, subExpressionFunc) +
                    expression.Substring(subExpressionEnd);

                //Stops and returns when a single numeric value is left.
                if (Decimal.TryParse(expression, out result))
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// Parses an expression without parentheses, optionally as arguments
        /// to a given function.
        /// </summary>
        /// <param name="subExpression">
        /// A mathematical expression without parentheses.
        /// </param>
        /// <param name="functionExpression">
        /// A function to apply using the sub-expression provided.
        /// </param>
        /// <exception cref="ParsingException">
        /// A parsing exception is thrown when an empty expression is
        /// provided or the expression is malformed.
        /// </exception>
        private static decimal ParseSubExpression(
            string subExpression,
            FunctionToken function)
        {
            //Strips () and catches empty expressions.
            subExpression = subExpression.Replace("(", "").Replace(")", "");
            if (subExpression == "")
            {
                throw new ParsingException("An empty parenthesis group was " +
                    "provided; there is nothing to process within it.");
            }

            //Functions make a call to this method for each argument.
            if (function != null)
            {
                string[] args = subExpression.Split(',');
                decimal[] argVals = new decimal[args.Length];

                //If wrong number of arguments, errors or selects an overload.
                if (function.NumberOfArgs != args.Length)
                {
                    //Finds a correct overload.
                    var funcOverloads = functions.Where(
                        o => o.Format == function.Format &&
                        o.NumberOfArgs == args.Length);

                    if (funcOverloads.Count() != 0)
                    {
                        function = funcOverloads.First();
                    }
                    else
                    {
                        throw new ParsingException("In expression '" + subExpression +
                        "', the number of arguments for " + function.Format +
                        " should be " + function.NumberOfArgs + ", but " +
                        args.Length + " arguments were given.");
                    }
                }

                //Simplifies each argument.
                for (int i = 0; i < args.Length; i++)
                {
                    argVals[i] = ParseSubExpression(args[i], null);
                }

                //Applies functions.
                return function.Operation(argVals);
            }

            //Initializes list of tokens and current token.
            List<ParsingToken> tokens = new List<ParsingToken>();
            string token = "";

            //Builds a token list.
            foreach (char chr in subExpression)
            {
                token += chr;

                //Ensures decimal numbers have a zero in front.
                if (token == ".")
                {
                    token = "0.";
                    continue;
                }

                //Concatenates until a non-digit is concatenated.
                if (!Char.IsDigit(chr))
                {
                    //Adds numeric tokens at a non-digit boundary.
                    if (Char.IsDigit(token[0]))
                    {
                        if (chr == '.')
                        {
                            if (token.Substring(0, token.Length - 1).Contains("."))
                            {
                                throw new ParsingException(
                                    "In '" + subExpression +
                                    "', the expression '" + token +
                                    "' is an invalid number.");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            tokens.Add(new NumericToken(
                                token.Substring(0, token.Length - 1)));

                            token = chr.ToString();
                        }
                    }

                    //Adds operator tokens as they appear. Since - has two
                    //meanings, assumes subtraction first.
                    bool isOperatorFound = false;
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (token == operators[i].Format &&
                            operators[i] != NEG)
                        {
                            isOperatorFound = true;
                            tokens.Add(operators[i]);
                            token = "";                            
                            break;
                        }
                    }

                    //Adds identifier tokens as they appear.
                    if (!isOperatorFound)
                    {
                        for (int i = 0; i < identifiers.Count; i++)
                        {
                            if (token == identifiers[i].Format)
                            {
                                tokens.Add(new NumericToken(identifiers[i].NumericValue));
                                token = "";
                                break;
                            }
                        }
                    }
                }
            }

            //Handles remaining text.
            if (token != "")
            {
                //If remaining text is a number.
                if (Decimal.TryParse(token, out decimal val))
                {
                    tokens.Add(new NumericToken(val));
                }

                //Remaining text is invalid.
                else
                {
                    throw new ParsingException("In '" + subExpression +
                        "', the symbols '" + token + "' are not recognized.");
                }
            }

            //Minuses are binary by default; determines which ones are unary.
            //If the first token is a minus, it's a negation.
            if (tokens[0] == SUB)
            {
                tokens[0] = NEG;
            }

            //A minus after a binary operator or negation is a negation.
            for (int i = 1; i < tokens.Count; i++)
            {
                if (tokens[i] == SUB &&
                    (tokens[i - 1].Variant == TokenType.Operator &&
                    (tokens[i - 1].NumberOfArgs > 1 ||
                    tokens[i - 1] == NEG)) ||
                    (tokens[i - 1].Variant == TokenType.Function))
                {
                    tokens[i] = NEG;
                }
            }

            //Gets max precedence within sub-expression.
            var opTokens = tokens.Where(o => o.Variant == TokenType.Operator);
            int maxPrecedence = (opTokens.Count() > 0)
                ? opTokens.Max(o => o.Precedence) : 0;

            //Computes all tokens with equal precedence.
            while (maxPrecedence > 0)
            {
                bool isRightAssociative =
                    (operators.Any(o => maxPrecedence == o.Precedence &&
                    o.OpAssociativity == TokenOpAssociativity.Right));

                //Iterates through each token forwards or backwards.
                int j = (isRightAssociative) ? tokens.Count - 1 : 0;
                while ((isRightAssociative && j >= 0) ||
                    (!isRightAssociative && j < tokens.Count))
                {
                    if (tokens[j].Variant == TokenType.Operator &&
                        tokens[j].Precedence == maxPrecedence)
                    {
                        decimal value = 0;
                        List<decimal> argVals = new List<decimal>();

                        //Computes adjacent numeric tokens.
                        bool leftValueFound = false;
                        bool rightValueFound = false;

                        if (j > 0 &&
                            tokens[j - 1].Variant == TokenType.Number)
                        {
                            argVals.Add(tokens[j - 1].NumericValue);
                            leftValueFound = true;
                        }
                        if (j < tokens.Count - 1 &&
                            tokens[j + 1].Variant == TokenType.Number)
                        {
                            argVals.Add(tokens[j + 1].NumericValue);
                            rightValueFound = true;
                        }

                        //Handles missing arguments.
                        if (!leftValueFound &&
                            (tokens[j].OpPlacement == TokenOpPlacement.Both ||
                            tokens[j].OpPlacement == TokenOpPlacement.Left))
                        {
                            throw new ParsingException("In '" + subExpression +
                                "', the '" + tokens[j].Format + "' operator " +
                                "is missing a lefthand operand.");
                        }
                        else if (!rightValueFound &&
                            (tokens[j].OpPlacement == TokenOpPlacement.Both ||
                            tokens[j].OpPlacement == TokenOpPlacement.Right))
                        {
                            throw new ParsingException("In '" + subExpression +
                                "', the '" + tokens[j].Format + "' operator " +
                                "is missing a righthand operand.");
                        }

                        //Applies each operator.
                        value = (tokens[j] as OperatorToken).Operation(argVals.ToArray());

                        //Removes affected tokens and inserts new value.
                        if (tokens[j].OpPlacement ==
                            TokenOpPlacement.Left)
                        {
                            tokens[j] = new NumericToken(value);
                            tokens.RemoveAt(j - 1);
                            j += (isRightAssociative) ? 0 : -1;
                        }
                        else if (tokens[j].OpPlacement ==
                            TokenOpPlacement.Right)
                        {
                            tokens[j] = new NumericToken(value);
                            tokens.RemoveAt(j + 1);
                            j += (isRightAssociative) ? 1 : 0;
                        }
                        else if (tokens[j].OpPlacement ==
                            TokenOpPlacement.Both)
                        {
                            tokens[j] = new NumericToken(value);
                            tokens.RemoveAt(j + 1);
                            tokens.RemoveAt(j - 1);
                            j += (isRightAssociative) ? 0 : -1;
                        }
                    }

                    //Moves to next token to evaluate.
                    if (isRightAssociative)
                    {
                        j--;
                    }
                    else
                    {
                        j++;
                    }
                }

                //Gets new precedence within sub-expression.
                opTokens = tokens.Where(o => o.Variant == TokenType.Operator);
                maxPrecedence = (opTokens.Count() > 0)
                    ? opTokens.Max(o => o.Precedence) : 0;
            }

            //Returns the final value.
            if (tokens.Count > 0 && tokens[0].Variant == TokenType.Number &&
                Decimal.TryParse(tokens[0].Format, out decimal result))
            {
                return result;
            }
            else
            {
                string extraTokens = "";

                for (int i = 0; i < tokens.Count; i++)
                {
                    extraTokens += tokens[i].Format;
                    if (i != tokens.Count - 1)
                    {
                        extraTokens += " ";
                    }
                }

                throw new ParsingException("In '" + subExpression +
                    "', the symbols '" + extraTokens + "' are unexpected " +
                    "and cannot be processed.");
            }
        }

        /// <summary>
        /// Returns an immutable list of all operators in use.
        /// </summary>
        public static ReadOnlyCollection<OperatorToken> GetOperators()
        {
            return new ReadOnlyCollection<OperatorToken>(operators);
        }

        /// <summary>
        /// Returns an immutable list of all functions in use.
        /// </summary>
        public static ReadOnlyCollection<FunctionToken> GetFunctions()
        {
            return new ReadOnlyCollection<FunctionToken>(functions);
        }

        /// <summary>
        /// Returns an immutable list of all identifiers in use.
        /// </summary>
        public static ReadOnlyCollection<IdentifierToken> GetIdentifiers()
        {
            return new ReadOnlyCollection<IdentifierToken>(identifiers);
        }

        /// <summary>
        /// Adds the given token as a function.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddFunction(FunctionToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            functions.Add(token);
        }

        /// <summary>
        /// Adds the given token as an identifier.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddIdentifier(IdentifierToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            identifiers.Add(token);
        }

        /// <summary>
        /// Adds the given token as an operator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddOperator(OperatorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            operators.Add(token);
        }

        /// <summary>
        /// Removes the given token from the list of functions, if it
        /// exists. Returns true if found; false otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static bool RemoveFunction(FunctionToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            return functions.Remove(token);
        }

        /// <summary>
        /// Removes the given token from the list of identifiers, if it
        /// exists. Returns true if found; false otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static bool RemoveIdentifier(IdentifierToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            return identifiers.Remove(token);
        }

        /// <summary>
        /// Removes the given token from the list of operators, if it
        /// exists. Returns true if found; false otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static bool RemoveOperator(OperatorToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            return operators.Remove(token);
        }
        #endregion
    }
}
