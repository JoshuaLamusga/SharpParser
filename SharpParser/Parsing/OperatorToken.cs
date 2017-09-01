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
            NumericValue = 0;
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
            NumericValue = 0;
            Operation = operation;
        }
        #endregion

        #region Methods
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