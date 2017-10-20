using System;

namespace SharpParser.Parsing.Core
{
    /// <summary>
    /// A function token.
    /// </summary>
    public class TokenFunc : Token
    {
        #region Properties
        /// <summary>
        /// The number of arguments to be used.
        /// </summary>
        public int NumArgs { get; protected set; }

        /// <summary>
        /// When this function is used, the input numbers can be accessed as
        /// an array of objects. As many as provided by the number of
        /// arguments may be used.
        /// </summary>
        public Func<Token[], Token> Function
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a function parsing token.
        /// </summary>
        /// <param name="name">
        /// The unique name identifying the function.
        /// </param>
        /// <param name="numberOfArgs">
        /// The number of arguments the function takes.
        /// </param>
        /// <param name="operation">
        /// During evaluation, all involved numbers are passed to this
        /// function and returned.
        /// </param>
        public TokenFunc(string name,
            int numberOfArgs,
            Func<Token[], Token> operation)
        {
            NumArgs = numberOfArgs;
            StrForm = name;
            Function = operation;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if all properties of each token are the same.
        /// </summary>
        /// <param name="obj">
        /// The token to compare against for equality.
        /// </param>
        public bool Equals(TokenFunc obj)
        {
            return (StrForm == obj.StrForm &&
                NumArgs == obj.NumArgs &&
                Function == obj.Function);
        }
        #endregion
    }
}