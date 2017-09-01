using System;

namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents a single token for evaluation.
    /// </summary>
    public class NumericToken : ParsingToken
    {
        #region Properties
        /// <summary>
        /// The associated numeric value.
        /// </summary>
        public decimal NumericValue { get; protected set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a numeric parsing token.
        /// </summary>
        /// <param name="value">
        /// The number encapsulated in the token.
        /// </param>
        public NumericToken(string value)
        {
            Variant = TokenType.Number;
            Format = value;

            if (Decimal.TryParse(value, out decimal result))
            {
                NumericValue = result;
            }
            else
            {
                throw new ParsingException("The expression '" + value +
                    "' is not a valid number.");
            }
        }

        /// <summary>
        /// Creates a numeric parsing token.
        /// </summary>
        /// <param name="value">
        /// The number encapsulated in the token.
        /// </param>
        public NumericToken(decimal value)
        {
            Variant = TokenType.Number;
            Format = value.ToString();
            NumericValue = value;
        }
        #endregion
    }
}