using System;
using SharpParser.Parsing.Core;

namespace SharpParser
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Please enter an expression: ");

                try
                {
                    Console.WriteLine(Parser.Eval(Console.ReadLine()));
                }
                catch (ParsingException e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
    }
}