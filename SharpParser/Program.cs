using System;
using SharpParser.Parsing;

namespace SharpParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string userInput = "";

            while (true)
            {
                Console.Write("Please enter an expression: ");
                userInput = Console.ReadLine();

                try
                {
                    Console.WriteLine(ExpressionParser.ParseExpression(userInput));
                }
                catch (ParsingException e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}