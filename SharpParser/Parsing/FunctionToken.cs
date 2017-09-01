using System;

namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents a single token for evaluation.
    /// </summary>
    public class FunctionToken : ParsingToken
    {
        #region Properties
        /// <summary>
        /// When this function is used, the input numbers can be accessed as
        /// an array of decimals. As many as provided by the number of
        /// arguments may be used.
        /// </summary>
        public Func<decimal[], decimal> Operation
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a function parsing token.
        /// </summary>
        /// <param name="name">
        /// The unique name identifying the function.
        /// </param>
        /// <param name="numberOfArgs">
        /// The number of arguments the function takes.
        /// </param>
        public FunctionToken(string name, int numberOfArgs)
        {
            Variant = TokenType.Function;
            OpPlacement = TokenOpPlacement.None;
            OpAssociativity = TokenOpAssociativity.None;
            Precedence = 0;
            NumberOfArgs = numberOfArgs;
            Format = name;
            NumericValue = 0;
            Operation = null;
        }

        /// <summary>
        /// Creates a function parsing token.
        /// </summary>
        /// <param name="name">
        /// The unique name identifying the function.
        /// </param>
        /// <param name="numberOfArgs">
        /// The number of arguments the function takes.
        /// </param>
        /// <param name="operation">
        /// During evaluation, all involved numbers are passed to this
        /// function and returned.
        /// </param>
        public FunctionToken(string name,
            int numberOfArgs,
            Func<decimal[], decimal> operation)
        {
            Variant = TokenType.Function;
            OpPlacement = TokenOpPlacement.None;
            OpAssociativity = TokenOpAssociativity.None;
            Precedence = 0;
            NumberOfArgs = numberOfArgs;
            Format = name;
            NumericValue = 0;
            Operation = operation;
        }
        #endregion

        #region Methods
        /// <summary>
        /// During evaluation, all involved numbers are passed to this
        /// function and returned.
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