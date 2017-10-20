# Sharp Parser
An extensible symbolic math expression parser in C#

## Features
- Extensible lists of tokens for operators, functions, and identifiers
- Greedy matching for known tokens with lazy matching of literals and unknowns. Ignores whitespace
- Option to preserve unknown values
- Option to preserve and simplify fractions in division computations
- Arithmetic operations: -, ^, %, /, *, -, +
- Equality comparisons: >, >=, <, <=, =, !=
- Boolean operations: !, |, &
- Common functions: trigonometry, rounding, logarithms, max/min, absolute...
- Common constants: pi, e

## Using Sharp Parser
Parser.Eval(string): Evaluates the given expression and returns the answer as a string.

Parser.OptUseImplicitMult: If true, enables implicit multiplication.  
Parser.OptRequireRightPars: If true, parentheses groups must always be balanced.  
Parser.OptIncludeUnknowns: If true, unknown tokens will be treated as variables.  
Parser.OptUseFractions: If true, all division operations will return fractions.

Parser.AddFunction(token): Adds a function token. Functions take any number of tokens and return a token.  
Parser.AddIdentifier(token): Adds an identifier token. Identifiers may have a value or be unknown.  
Parser.AddOperator(token): Adds an operator token. Operators take one or two tokens and return a token.  
Parser.GetTokens(): Returns an immutable list of all tokens in use.  
Parser.RemoveToken(token): Removes the given token from the list.  
Parser.Tokenize(string): Converts the given string to tokens, but performs no evaluation.
