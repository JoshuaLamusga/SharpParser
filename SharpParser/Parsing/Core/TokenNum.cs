using System;

namespace SharpParser.Parsing.Core
{
    /// <summary>
    /// A numeric token to store a decimal literal.
    /// </summary>
    public class TokenNum : Token
    {
        #region Properties
        /// <summary>
        /// The associated numeric value.
        /// </summary>
        public decimal Value { get; protected set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a numeric parsing token.
        /// </summary>
        /// <param name="value">
        /// The number encapsulated in the token.
        /// </param>
        public TokenNum(string value)
        {
            StrForm = value;

            if (Decimal.TryParse(value, out decimal result))
            {
                Value = result;
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
        public TokenNum(decimal value)
        {
            StrForm = value.ToString();
            Value = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if all properties of each token are the same.
        /// </summary>
        /// <param name="obj">
        /// The token to compare against for equality.
        /// </param>
        public bool Equals(TokenNum obj)
        {
            return (StrForm == obj.StrForm &&
                Value == obj.Value);
        }
        #endregion
    }
}