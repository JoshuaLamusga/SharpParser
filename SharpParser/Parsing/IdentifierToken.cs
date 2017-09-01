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

        /// <summary>
        /// Returns true if all properties of each token are the same.
        /// </summary>
        /// <param name="obj">
        /// The token to compare against for equality.
        /// </param>
        public bool Equals(IdentifierToken obj)
        {
            return (Variant == obj.Variant &&
                Format == obj.Format &&
                NumericValue == obj.NumericValue);
        }
        #endregion
    }
}