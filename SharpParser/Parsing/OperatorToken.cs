using System;

namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents a single token for evaluation.
    /// </summary>
    public class OperatorToken : ParsingToken
    {
        #region Properties
        /// <summary>
        /// Indicates which side of the operator the operand is on, or if
        /// the operator is binary.
        /// </summary>
        public TokenOpPlacement OpPlacement { get; protected set; }

        /// <summary>
        /// Sets whether an expression such as a ~ b ~ c is evaluated
        /// left-to-right or right-to-left.
        /// </summary>
        public TokenOpAssociativity OpAssociativity { get; protected set; }

        /// <summary>
        /// The order in which operator tokens are evaluated.
        /// </summary>
        public int Precedence { get; protected set; }

        /// <summary>
        /// The number of arguments to be used.
        /// </summary>
        public int NumberOfArgs { get; protected set; }

        /// <summary>
        /// When this operator is used, the input numbers can be accessed as
        /// an array of decimals. The lefthand operand is element 0 for
        /// binary operators.
        /// </summary>
        public Func<decimal[], decimal> Operation
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an operator parsing token.
        /// </summary>
        /// <param name="associativity">
        /// Determines whether a ~ b ~ c is evaluated from left or right side.
        /// </param>
        /// <param name="opPlacement">
        /// Determines which side of a number operator tokens are expected
        /// to be on.
        /// </param>
        /// <param name="precedence">
        /// Operator precedence determines the order in which operators are
        /// evaluated.
        /// </param>
        /// <param name="format">
        /// The unique symbols identifying the operator.
        /// </param>
        public OperatorToken(
            TokenOpPlacement opPlacement,
            TokenOpAssociativity associativity,
            int precedence,
            string format)
        {
            Variant = TokenType.Operator;
            OpPlacement = opPlacement;
            OpAssociativity = associativity;
            Precedence = precedence;

            if (opPlacement == TokenOpPlacement.Both)
            {
                NumberOfArgs = 2;
            }
            else
            {
                NumberOfArgs = 1;
            }

            Format = format;
            Operation = null;
        }

        /// <summary>
        /// Creates an operator parsing token.
        /// </summary>
        /// <param name="associativity">
        /// Determines whether a ~ b ~ c is evaluated from left or right side.
        /// </param>
        /// <param name="opPlacement">
        /// Determines which side of a number operator tokens are expected
        /// to be on.
        /// </param>
        /// <param name="precedence">
        /// Operator precedence determines the order in which operators are
        /// evaluated.
        /// </param>
        /// <param name="format">
        /// The unique symbols identifying the operator.
        /// </param>
        /// <param name="operation">
        /// During evaluation, all involved numbers are passed to this
        /// function and returned.
        /// </param>
        public OperatorToken(
            TokenOpPlacement opPlacement,
            TokenOpAssociativity associativity,
            int precedence,
            string format,
            Func<decimal[], decimal> operation)
        {
            Variant = TokenType.Operator;
            OpPlacement = opPlacement;
            OpAssociativity = associativity;
            Precedence = precedence;

            if (opPlacement == TokenOpPlacement.Both)
            {
                NumberOfArgs = 2;
            }
            else
            {
                NumberOfArgs = 1;
            }

            Format = format;
            Operation = operation;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if all properties of each token are the same.
        /// </summary>
        /// <param name="obj">
        /// The token to compare against for equality.
        /// </param>
        public bool Equals(OperatorToken obj)
        {
            return (Variant == obj.Variant &&
                Format == obj.Format &&
                OpPlacement == obj.OpPlacement &&
                OpAssociativity == obj.OpAssociativity &&
                Precedence == obj.Precedence &&
                NumberOfArgs == obj.NumberOfArgs &&
                Operation == obj.Operation);
        }

        /// <summary>
        /// During evaluation, all involved numbers are passed to this
        /// operator and returned.
        /// </summary>
        public void SetOperation(Func<decimal[], decimal> operation)
        {
            Operation = operation;
        }

        /// <summary>
        /// Clears the associated operation so no function is performed.
        /// </summary>
        public void ClearOperation()
        {
            Operation = null;
        }
        #endregion
    }
}