namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents the valid types of a parsing token.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// The token represents a numeric literal.
        /// </summary>
        Number,

        /// <summary>
        /// The token represents an operator in an expression.
        /// </summary>
        Operator,

        /// <summary>
        /// The token represents a named function.
        /// </summary>
        Function,

        /// <summary>
        /// The token represents a named entity with a numeric value.
        /// </summary>
        Identifier
    }
}
