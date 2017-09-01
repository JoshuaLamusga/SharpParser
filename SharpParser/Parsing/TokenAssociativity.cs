namespace SharpParser.Parsing
{
    /// <summary>
    /// Represents which direction to evaluate multiple homogenous operators
    /// in.
    /// </summary>
    public enum TokenOpAssociativity
    {
        /// <summary>
        /// Left associative operators compute a ~ b ~ c as (a ~ b) ~ c.
        /// </summary>
        Left,
        
        /// <summary>
        /// Right associative operators compute a ~ b ~ c as a ~ (b ~ c).
        /// </summary>
        Right,

        /// <summary>
        /// Associativity does not apply or is irrelevant.
        /// </summary>
        None
    }
}
