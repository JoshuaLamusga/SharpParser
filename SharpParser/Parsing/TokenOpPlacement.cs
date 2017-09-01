namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents which side of a number operator tokens are expected to be on.
    /// </summary>
    public enum TokenOpPlacement
    {
        /// <summary>
        /// For unary tokens that use the preceeding number, like negation.
        /// </summary>
        Left,

        /// <summary>
        /// For unary tokens that use the following number, like factorial.
        /// </summary>
        Right,

        /// <summary>
        /// For non-unary tokens.
        /// </summary>
        Both,

        /// <summary>
        /// For non-operator tokens.
        /// </summary>
        None
    }
}
