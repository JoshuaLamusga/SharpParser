using System;

namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents a single token for evaluation.
    /// </summary>
    public class IdentifierToken : ParsingToken
    {
        #region Properties
        /// <summary>
        /// The associated numeric value.
        /// </summary>
        public decimal NumericValue { get; protected set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an identifier parsing token.
        /// </summary>
        /// <param name="format">
        /// The unique name for the identifier.
        /// </param>
        /// <param name="value">
        /// The number encapsulated in the token.
        /// </param>
        public IdentifierToken(string name, decimal value)
        {
            Variant = TokenType.Identifier;
            Format = name;
            NumericValue = value;
        }
        #endregion
    }
}