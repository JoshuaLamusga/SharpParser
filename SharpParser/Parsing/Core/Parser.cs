using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpParser.Parsing.Core
{
    /// <summary>
    /// Tokenizes mathematical expressions to evaluate or symbolically
    /// manipulate them.
    /// </summary>
    public static class Parser
    {
        #region Parsing Options
        /// <summary>
        /// If true, numbers next to each other will be multiplied together
        /// when no other operation is specified. True by default.
        /// </summary>
        public static bool OptUseImplicitMult;

        /// <summary>
        /// If true, parentheses groups must always be balanced. False by
        /// default.
        /// </summary>
        public static bool OptRequireRightPars;

        /// <summary>
        /// If true, tokens that aren't recognized will be added as unknown
        /// variables. True by default.
        /// </summary>
        public static bool OptIncludeUnknowns;
        #endregion

        #region Functions
        /// <summary>
        /// The sine function for radians.
        /// </summary>
        private static readonly TokenFunc Fsin;

        /// <summary>
        /// The cosine function for radians.
        /// </summary>
        private static readonly TokenFunc Fcos;

        /// <summary>
        /// The tangent function for radians.
        /// </summary>
        private static readonly TokenFunc Ftan;

        /// <summary>
        /// Rounds a single number to the nearest integer.
        /// </summary>
        private static readonly TokenFunc Frnd;

        /// <summary>
        /// Rounds a number to the nearest multiple of another.
        /// </summary>
        private static readonly TokenFunc Frnd2;
        #endregion

        #region Identifiers
        /// <summary>
        /// The mathematical constant, Pi.
        /// </summary>
        private static readonly TokenId Pi;
        #endregion

        #region Operators
        /// <summary>
        /// The addition operator.
        /// </summary>
        private static readonly TokenOp Add;

        /// <summary>
        /// The subtraction operator.
        /// </summary>
        private static readonly TokenOp Sub;

        /// <summary>
        /// The multiplication operator.
        /// </summary>
        private static readonly TokenOp Mlt;

        /// <summary>
        /// The division operator.
        /// </summary>
        private static readonly TokenOp Div;

        /// <summary>
        /// The modulus operator.
        /// </summary>
        private static readonly TokenOp Mod;

        /// <summary>
        /// The negation operator.
        /// </summary>
        private static readonly TokenOp Neg;

        /// <summary>
        /// The exponentiation operator.
        /// </summary>
        private static readonly TokenOp Exp;

        /// <summary>
        /// The factorial operator.
        /// </summary>
        private static readonly TokenOp Fac;

        /// <summary>
        /// The equality operator.
        /// </summary>
        private static readonly TokenOp Eq;

        /// <summary>
        /// The inequality operator.
        /// </summary>
        private static readonly TokenOp NotEq;

        /// <summary>
        /// The greater-than operator.
        /// </summary>
        private static readonly TokenOp Gt;

        /// <summary>
        /// The greater-than-or-equal operator.
        /// </summary>
        private static readonly TokenOp Gte;

        /// <summary>
        /// The less-than operator.
        /// </summary>
        private static readonly TokenOp Lt;

        /// <summary>
        /// The less-than-or-equal operator.
        /// </summary>
        private static readonly TokenOp Lte;

        /// <summary>
        /// The logical not operator.
        /// </summary>
        private static readonly TokenOp LogNot;

        /// <summary>
        /// The logical and operator.
        /// </summary>
        private static readonly TokenOp LogAnd;

        /// <summary>
        /// The logical or operator.
        /// </summary>
        private static readonly TokenOp LogOr;
        #endregion

        #region Symbols
        /// <summary>
        /// Represents a left parenthesis.
        /// </summary>
        private static readonly TokenSym Lpar;

        /// <summary>
        /// Represents a right parenthesis.
        /// </summary>
        private static readonly TokenSym Rpar;

        /// <summary>
        /// Represents a function argument separator.
        /// </summary>
        private static readonly TokenSym ArgSep;
        #endregion

        #region Token List
        /// <summary>
        /// A mutable list of all tokens to parse with.
        /// </summary>
        private static List<Token> tokens;
        #endregion

        #region Static Constructor
        /// <summary>
        /// Sets all default values for all statically-accessible items.
        /// </summary>
        static Parser()
        {
            //Sets parsing option defaults.
            OptUseImplicitMult = true;
            OptRequireRightPars = false;
            OptIncludeUnknowns = true;

            //Sets default operators.
            Fac = new TokenOp(Placements.Left,
                Associativity.Left, 9, "!",
                new Func<object[], Token>((num) =>
                {
                    if (num[0] is TokenNum n0)
                    {
                        decimal givenVal = n0.Value;
                        decimal value = 1;

                        while (n0.Value > 1)
                        {
                            value *= givenVal--;
                        }

                        return new TokenNum(value);
                    }

                    return null;
                }));

            Neg = new TokenOp(Placements.Right,
                Associativity.Right, 8, "-",
                new Func<object[], Token>((num) =>
                {
                    if (num[1] is TokenNum n1)
                    {
                        return new TokenNum(-n1.Value);
                    }

                    return null;
                }));

            Exp = new TokenOp(Placements.Both,
                Associativity.Right, 8, "^",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        try
                        {
                            return checked(new TokenNum((decimal)Math.Pow(
                                (double)n0.Value, (double)n1.Value)));
                        }
                        catch (OverflowException e)
                        {
                            throw new ParsingException(
                                n0.StrForm + " ^ " + n1.StrForm +
                                " is too large to compute.", e);
                        }
                    }

                    return null;
                }));

            Mod = new TokenOp(Placements.Both,
                Associativity.Left, 7, "%",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        if (n1.Value == 0)
                        {
                            throw new ParsingException("The expression " +
                                n0.StrForm + " % " + n1.StrForm +
                                " causes division by zero.");
                        }

                        return new TokenNum(n0.Value % n1.Value);
                    }

                    return null;
                }));

            Div = new TokenOp(Placements.Both,
                Associativity.Left, 7, "/",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        if (n1.Value == 0)
                        {
                            throw new ParsingException("The expression " +
                                n0.StrForm + " / " + n1.StrForm +
                                " causes division by zero.");
                        }

                        return new TokenNum(n0.Value / n1.Value);
                    }

                    return null;
                }));

            Mlt = new TokenOp(Placements.Both,
                Associativity.Left, 7, "*",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenNum(n0.Value * n1.Value);
                    }

                    return null;
                }));

            Sub = new TokenOp(Placements.Both,
                Associativity.Left, 6, "-",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenNum(n0.Value - n1.Value);
                    }

                    return null;
                }));

            Add = new TokenOp(Placements.Both,
                Associativity.Left, 6, "+",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenNum(n0.Value + n1.Value);
                    }

                    return null;
                }));

            Gt = new TokenOp(Placements.Both,
                Associativity.Left, 5, ">",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value > n1.Value);
                    }

                    return null;
                }));

            Gte = new TokenOp(Placements.Both,
                Associativity.Left, 5, ">=",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value >= n1.Value);
                    }

                    return null;
                }));

            Lt = new TokenOp(Placements.Both,
                Associativity.Left, 5, "<",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value < n1.Value);
                    }

                    return null;
                }));

            Lte = new TokenOp(Placements.Both,
                Associativity.Left, 5, "<=",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value <= n1.Value);
                    }

                    return null;
                }));

            Eq = new TokenOp(Placements.Both,
                Associativity.Left, 4, "=",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value == n1.Value);
                    }

                    else if (
                        (num[0] is TokenBool n0bool) &&
                        (num[1] is TokenBool n1bool))
                    {
                        return new TokenBool(n0bool.Value == n1bool.Value);
                    }

                    return null;
                }));

            NotEq = new TokenOp(Placements.Both,
                Associativity.Left, 4, "!=",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenBool(n0.Value != n1.Value);
                    }

                    else if (
                        (num[0] is TokenBool n0bool) &&
                        (num[1] is TokenBool n1bool))
                    {
                        return new TokenBool(n0bool.Value != n1bool.Value);
                    }

                    return null;
                }));

            LogNot = new TokenOp(Placements.Right,
                Associativity.Left, 3, "!",
                new Func<object[], Token>((num) =>
                {
                    if (num[1] is TokenBool n1)
                    {
                        return new TokenBool(!n1.Value);
                    }

                    return null;
                }));

            LogOr = new TokenOp(Placements.Both,
                Associativity.Left, 2, "|",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenBool n0) &&
                        (num[1] is TokenBool n1))
                    {
                        return new TokenBool(n0.Value || n1.Value);
                    }

                    return null;
                }));

            LogAnd = new TokenOp(Placements.Both,
                Associativity.Left, 1, "&",
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenBool n0) &&
                        (num[1] is TokenBool n1))
                    {
                        return new TokenBool(n0.Value && n1.Value);
                    }

                    return null;
                }));

            //Sets default functions.
            Fsin = new TokenFunc("sin", 1,
                new Func<object[], Token>((num) =>
                {
                    if (num[0] is TokenNum n0)
                    {
                        return new TokenNum(
                            (decimal)Math.Sin((double)n0.Value));
                    }

                    return null;
                }));

            Fcos = new TokenFunc("cos", 1,
                new Func<object[], Token>((num) =>
                {
                    if (num[0] is TokenNum n0)
                    {
                        return new TokenNum(
                            (decimal)Math.Cos((double)n0.Value));
                    }

                    return null;
                }));

            Ftan = new TokenFunc("tan", 1,
                new Func<object[], Token>((num) =>
                {
                    if (num[0] is TokenNum n0)
                    {
                        return new TokenNum(
                            (decimal)Math.Tan((double)n0.Value));
                    }

                    return null;
                }));

            Frnd = new TokenFunc("round", 1,
                new Func<object[], Token>((num) =>
                {
                    if (num[0] is TokenNum n0)
                    {
                        return new TokenNum(Math.Round(n0.Value));
                    }

                    return null;
                }));

            Frnd2 = new TokenFunc("round", 2,
                new Func<object[], Token>((num) =>
                {
                    if ((num[0] is TokenNum n0) &&
                        (num[1] is TokenNum n1))
                    {
                        return new TokenNum(
                            Math.Round(n0.Value / n1.Value) * n1.Value);
                    }

                    return null;
                }));

            //Sets default identifiers.
            Pi = new TokenId("pi", (decimal)Math.PI);

            //Sets default symbols
            Lpar = new TokenSym("(");
            Rpar = new TokenSym(")");
            ArgSep = new TokenSym(",");

            //Sets the token list.
            tokens = new List<Token>()
            {
                //Omitted: Fac
                Exp, Neg, Mod, Div, Mlt, Sub, Add, LogNot, LogOr, LogAnd,
                Eq, Gt, Gte, Lt, Lte, NotEq,
                Fsin, Fcos, Ftan, Frnd, Frnd2,
                Pi,
                Lpar, Rpar, ArgSep
            };

            //Sorts tokens in reverse lexicographic order to support deferring.
            tokens = tokens.OrderBy(o => o.StrForm).Reverse().ToList();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a string-lowercased copy of the function.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddFunction(TokenFunc token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            tokens.Add(new TokenFunc(
                token.StrForm.ToLower(),
                token.NumArgs,
                token.Function));

            //Sorts tokens in reverse lexicographic order to support deferring.
            tokens = tokens.OrderBy(o => o.StrForm).Reverse().ToList();
        }

        /// <summary>
        /// Adds a string-lowercased copy of the identifier.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddIdentifier(TokenId token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            tokens.Add(new TokenId(
                token.StrForm.ToLower(),
                token.Value));

            //Sorts tokens in reverse lexicographic order to support deferring.
            tokens = tokens.OrderBy(o => o.StrForm).Reverse().ToList();
        }

        /// <summary>
        /// Adds a string-lowercased copy of the operator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a null token is given.
        /// </exception>
        public static void AddOperator(TokenOp token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            tokens.Add(new TokenOp(
                token.Placement,
                token.Assoc,
                token.Prec,
                token.StrForm.ToLower(),
                token.Function));

            //Sorts tokens in reverse lexicographic order to support deferring.
            tokens = tokens.OrderBy(o => o.StrForm).Reverse().ToList();
        }

        ///<summary>
        /// Parses an expression with operators, functions, and identifiers.
        /// </summary>
        /// <param name="expression">
        /// An expression composed of numbers and included operators,
        /// and functions. No implicit multiplication.
        /// </param>
        public static object Eval(string expression)
        {
            List<Token> tokensList = Tokenize(expression);
            var functions = tokens.OfType<TokenFunc>().ToList();
            object result = 0;

            //Solves each parenthesis group from deepest depth outward.
            while (true)
            {
                //Finds the end of the nearest complete sub-expression.
                int rbrPos = tokensList.IndexOf(Rpar) + 1;
                int subExpressionEnd = (rbrPos >= 1) ? rbrPos : (tokensList.Count);

                //Finds the start of the nearest complete sub-expression.
                int lbrPos = tokensList.GetRange(0, subExpressionEnd).LastIndexOf(Lpar);
                int subExpressionBegin = (lbrPos >= 0) ? lbrPos : 0;

                //Isolates the sub-expression.
                List<Token> expressionLHS = tokensList.GetRange(0, subExpressionBegin);
                List<Token> expressionRHS = tokensList.GetRange(subExpressionEnd, tokensList.Count - subExpressionEnd);
                List<Token> subExpression = tokensList.GetRange(subExpressionBegin, subExpressionEnd - subExpressionBegin);

                //Includes functions and picks a proper overload.
                TokenFunc subExpressionFunc = null;
                if (expressionLHS.LastOrDefault() is TokenFunc tokFunc)
                {
                    expressionLHS.RemoveAt(expressionLHS.Count - 1);
                    int numArgs = subExpression.Count((tok) => tok == ArgSep) + 1;
                    subExpressionFunc = functions.FirstOrDefault((f) =>
                        f.NumArgs == numArgs &&
                        f.StrForm == tokFunc.StrForm);
                }

                //Evaluates sub-expressions.
                tokensList = expressionLHS;
                tokensList.Add(EvalNoPar(subExpression, subExpressionFunc));
                tokensList.AddRange(expressionRHS);

                //Returns when a single token is left.
                if (tokensList.Count == 1)
                {
                    return tokensList.First().StrForm;
                }
            }
        }

        /// <summary>
        /// Parses a non-relational expression without parentheses with an
        /// optional argument to treat the expression as function arguments.
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
        private static Token EvalNoPar(
            List<Token> subExpression,
            TokenFunc function)
        {
            var identifiers = tokens.OfType<TokenId>().ToList();
            var operators = tokens.OfType<TokenOp>().ToList();

            //Creates a string representation of the token list for errors.
            string subExpressionStr = "";
            for (int i = 0; i < subExpression.Count; i++)
            {
                subExpressionStr += subExpression[i].StrForm;
            }

            //Strips () and catches empty expressions.
            if (OptRequireRightPars &&
                subExpression.FirstOrDefault() == Lpar &&
                subExpression.LastOrDefault() != Rpar)
            {
                throw new ParsingException("The expression '" +
                    subExpressionStr + "' is missing a right parenthesis " +
                    "at the end.");
            }

            subExpression.RemoveAll((tok) => tok == Lpar || tok == Rpar);

            if (subExpression.Count == 0)
            {
                throw new ParsingException("An empty parenthesis group was " +
                    "provided; there is nothing to process within it.");
            }

            //Parses each argument separately, then applies the function.
            if (function != null)
            {
                List<List<Token>> args = subExpression.Split(ArgSep);
                Token[] argVals = new Token[args.Count];

                //Catches overloads with the wrong number of arguments.
                if (function.NumArgs != args.Count)
                {
                    throw new ParsingException("In expression '" +
                        subExpressionStr + "', the number of arguments for " +
                        function.StrForm + " should be " + function.NumArgs +
                        ", but " + args.Count + " arguments were given.");
                }

                //Simplifies each argument.
                for (int i = 0; i < args.Count; i++)
                {
                    object subResult = EvalNoPar(args[i], null);
                    if (subResult is TokenNum subResultNum)
                    {
                        argVals[i] = subResultNum;
                    }
                    else
                    {
                        throw new ParsingException("In expression '" +
                            subExpressionStr + "', a boolean argument was " +
                            "provided instead of a decimal value.");
                    }
                }

                //Applies functions.
                Token result = function.Function(argVals);
                if (result == null)
                {
                    throw new ParsingException("In expression '" +
                        subExpressionStr + "', arguments do not match " +
                        "parameter types used.");
                }
                else
                {
                    return result;
                }
            }

            //Minuses are binary by default; determines which ones are unary.
            //If the first token is a minus, it's a negation.
            if (subExpression[0] == Sub)
            {
                subExpression[0] = Neg;
            }

            //Performs left-to-right modifications on the token list.
            for (int i = 1; i < subExpression.Count; i++)
            {
                //A minus after a binary operator or negation is a negation.
                if (subExpression[i] == Sub &&
                    (subExpression[i - 1] is TokenOp &&
                    ((subExpression[i - 1] as TokenOp).NumArgs > 1 ||
                    subExpression[i - 1] == Neg)) ||
                    (subExpression[i - 1] is TokenFunc))
                {
                    subExpression[i] = Neg;
                }

                //Performs implicit multiplication.
                if (subExpression[i - 1] is TokenNum &&
                    subExpression[i] is TokenNum &&
                    OptUseImplicitMult)
                {
                    subExpression.Insert(i, Mlt);
                }
            }

            //Gets max precedence within sub-expression.
            var opTokens = subExpression.OfType<TokenOp>();
            int maxPrecedence = (opTokens.Count() > 0)
                ? opTokens.Max(o => o.Prec) : 0;

            //Computes all operators with equal precedence.
            while (maxPrecedence > 0)
            {
                bool isRightAssociative =
                    (operators.Any(o => maxPrecedence == o.Prec &&
                    o.Assoc == Associativity.Right));

                //Iterates through each token forwards or backwards.
                int j = (isRightAssociative) ? subExpression.Count - 1 : 0;
                while ((isRightAssociative && j >= 0) ||
                    (!isRightAssociative && j < subExpression.Count))
                {
                    if (subExpression[j] is TokenOp &&
                        (subExpression[j] as TokenOp).Prec == maxPrecedence)
                    {
                        TokenOp opToken = (TokenOp)subExpression[j];
                        List<Token> argVals = new List<Token>();
                        argVals.Add(subExpression.ElementAtOrDefault(j - 1));
                        argVals.Add(subExpression.ElementAtOrDefault(j + 1));

                        Token result = null;

                        //Handles missing arguments.
                        if (argVals[0] == null &&
                            (opToken.Placement == Placements.Both ||
                            opToken.Placement == Placements.Left))
                        {
                            throw new ParsingException("In '" + subExpressionStr +
                                "', the '" + subExpression[j].StrForm + "' operator " +
                                "is missing a lefthand operand.");
                        }
                        else if (argVals[1] == null &&
                            (opToken.Placement == Placements.Both ||
                            opToken.Placement == Placements.Right))
                        {
                            throw new ParsingException("In '" + subExpressionStr +
                                "', the '" + subExpression[j].StrForm + "' operator " +
                                "is missing a righthand operand.");
                        }

                        //Applies each operator.
                        result = opToken.Function(argVals.ToArray());

                        //Removes affected tokens and inserts new value.
                        if (result == null)
                        {
                            throw new ParsingException("In expression '" +
                                subExpressionStr + "', operand type(s) do " +
                                "not match operator.");
                        }
                        else
                        {
                            subExpression[j] = result;
                        }

                        if (opToken.Placement ==
                            Placements.Left)
                        {
                            subExpression.RemoveAt(j - 1);
                            j += (isRightAssociative) ? 0 : -1;
                        }
                        else if (opToken.Placement ==
                            Placements.Right)
                        {
                            subExpression.RemoveAt(j + 1);
                            j += (isRightAssociative) ? 1 : 0;
                        }
                        else if (opToken.Placement ==
                            Placements.Both)
                        {
                            subExpression.RemoveAt(j + 1);
                            subExpression.RemoveAt(j - 1);
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
                opTokens = subExpression.OfType<TokenOp>();
                maxPrecedence = (opTokens.Count() > 0)
                    ? opTokens.Max(o => o.Prec) : 0;
            }

            //Returns the final value.
            if (subExpression.Count == 1)
            {
                return subExpression[0];
            }
            else
            {
                string extraTokens = "";

                for (int i = 0; i < subExpression.Count; i++)
                {
                    extraTokens += subExpression[i].StrForm;
                    if (i != subExpression.Count - 1)
                    {
                        extraTokens += " ";
                    }
                }

                throw new ParsingException("In '" + subExpressionStr +
                    "', the symbols '" + extraTokens + "' are unexpected " +
                    "and cannot be processed.");
            }
        }

        /// <summary>
        /// Returns an immutable list of all tokens in use.
        /// </summary>
        public static ReadOnlyCollection<Token> GetTokens()
        {
            return new ReadOnlyCollection<Token>(tokens);
        }

        /// <summary>
        /// Removes the first match for the given token from the list of
        /// tokens, if it exists. Returns true if found; false otherwise.
        /// </summary>
        public static bool RemoveToken(Token token)
        {
            if (token == null)
            {
                return false;
            }

            //Removes the first match, if any.
            for (int i = tokens.Count; i > 0; i--)
            {
                if (token.Equals(tokens[i]))
                {
                    tokens.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns all consecutive items between each matched delimiter item.
        /// For example, a list containing [0, 2, 1, 3, 1] delimited by 1 will
        /// return the lists [0, 2][3].
        /// </summary>
        /// <param name="delimiter">
        /// The item marking the boundaries between sub-lists. Matched
        /// delimiter items will not be included in the resulting lists.
        /// </param>
        /// <returns></returns>
        private static List<List<T>> Split<T>(this List<T> list, T delimiter)
        {
            List<List<T>> lists = new List<List<T>>();
            List<T> currentList = new List<T>();

            //Stores the running list and creates another for each delimiter.
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], delimiter))
                {
                    lists.Add(new List<T>(currentList));
                    currentList = new List<T>();
                }
                else
                {
                    currentList.Add(list[i]);
                }
            }
            if (currentList.Count > 0)
            {
                lists.Add(currentList);
            }

            return lists;
        }

        /// <summary>
        /// Converts the given string to tokens.
        /// </summary>
        /// <param name="expression">
        /// An expression with operators, functions, and identifiers.
        /// </param>
        public static List<Token> Tokenize(string expression)
        {
            List<Token> tokensList = new List<Token>();
            string token = "";

            //Catches null or whitespace strings.
            if (String.IsNullOrWhiteSpace(expression))
            {
                throw new ParsingException("No expression provided.");
            }

            //Lowercases and removes whitespaces.
            expression = Regex.Replace(expression, @"\s", "").ToLower();

            //Builds a token list.
            Token longestMatch = null;
            Token shortestMatch = null;
            Token candidateBeforeDefer = null;
            for (int i = 0; i < expression.Length; i++)
            {
                token += expression[i];

                //Matches longer tokens and tokens of the same length.
                longestMatch = tokens.FirstOrDefault(
                    (tok) => tok.StrForm.StartsWith(token));

                //Defers when the token is longer.
                if (i != expression.Length - 1 &&
                    longestMatch?.StrForm.Length > token.Length)
                {
                    shortestMatch = tokens.FirstOrDefault(
                        (tok) => tok.StrForm == token);

                    //Stores valid matches as token matching is deferred.
                    if (shortestMatch?.StrForm == token)
                    {
                        candidateBeforeDefer = shortestMatch;
                    }
                }

                //Matches when there are no longer candidates.
                else if (i != expression.Length - 1 &&
                    longestMatch != null)
                {
                    //Adds the token, parsing identifiers.
                    if (longestMatch is TokenId tokenAsId &&
                        tokenAsId.Value != null)
                    {
                        tokensList.Add(new TokenNum(tokenAsId.Value ?? 0));
                    }
                    else
                    {
                        tokensList.Add(longestMatch);
                    }

                    token = "";
                    candidateBeforeDefer = null;
                }

                else
                {
                    //Backtracks to the last valid token.
                    if (candidateBeforeDefer != null)
                    {
                        i -= (token.Length - candidateBeforeDefer.StrForm.Length);

                        //Adds the token, parsing identifiers.
                        if (candidateBeforeDefer is TokenId tokenAsId &&
                        tokenAsId.Value != null)
                        {
                            tokensList.Add(new TokenNum(tokenAsId.Value ?? 0));
                        }
                        else
                        {
                            tokensList.Add(candidateBeforeDefer);
                        }

                        token = "";
                        candidateBeforeDefer = null;
                    }

                    //Matches literals.
                    else if (Decimal.TryParse(token,
                        NumberStyles.AllowDecimalPoint, null, out decimal val))
                    {
                        //Adds the numeric token at end of string or boundary.
                        if (i == expression.Length - 1 ||
                            !Decimal.TryParse(token + expression[i + 1],
                            NumberStyles.AllowDecimalPoint, null, out decimal val2))
                        {
                            tokensList.Add(new TokenNum(val));
                            token = "";
                        }
                    }

                    //Matches unknowns by-character if allowed.
                    else if (OptIncludeUnknowns)
                    {
                        tokensList.Add(new TokenId(token[0].ToString(), null));
                        i -= token.Length - 1;
                        token = "";
                    }
                    else
                    {
                        throw new ParsingException("Error: token '" +
                            token + "' is not a recognized symbol.");
                    }
                }
            }

            return tokensList;
        }
        #endregion
    }
}