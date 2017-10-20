using System;

namespace SharpParser.Parsing.Core
{
    /// <summary>
    /// A symbolic token to store a general symbol.
    /// </summary>
    public class TokenSym : Token
    {
        #region Constructors
        /// <summary>
        /// Creates a symbol token.
        /// </summary>
        public TokenSym(string name)
        {
            StrForm = name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if all properties of each token are the same.
        /// </summary>
        /// <param name="obj">
        /// The token to compare against for equality.
        /// </param>
        public bool Equals(TokenSym obj)
        {
            return (StrForm == obj.StrForm);
        }
        #endregion
    }
}