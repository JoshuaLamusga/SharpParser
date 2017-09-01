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
        /// The string format of the token.
        /// </summary>
        public string Format { get; protected set; }
        #endregion
    }
}