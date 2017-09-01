using System;
using System.Collections.Generic;

namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents a single token for evaluation. Abstract.
    /// </summary>
    public abstract class ParsingToken
    {
        #region Properties
        /// <summary>
        /// The type of token.
        /// </summary>
        public TokenType Variant { get; protected set; }

        /// <summary>
        /// Determines whether to use the number on the left or right for
        /// unary operator tokens, or both for binary tokens.
        /// </summary>
        public TokenOpPlacement OpPlacement { get; protected set; }

        /// <summary>
        /// Sets whether an expression such as a ~ b ~ c is evaluated
        /// left-to-right or right-to-left for operator tokens.
        /// </summary>
        public TokenOpAssociativity OpAssociativity { get; protected set; }

        /// <summary>
        /// The order in which operator tokens are evaluated.
        /// </summary>
        public int Precedence { get; protected set; }

        /// <summary>
        /// The number of arguments for an operator or function token.
        /// </summary>
        public int NumberOfArgs { get; protected set; }

        /// <summary>
        /// The string format of the token.
        /// </summary>
        public string Format { get; protected set; }

        /// <summary>
        /// For numeric tokens, this is the value.
        /// </summary>
        public decimal NumericValue { get; protected set; }
        #endregion
    }
}