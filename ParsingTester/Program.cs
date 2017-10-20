using SharpParser.Parsing.Algebra;
using System;

namespace ParsingTester
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
                catch (OverflowException)
                {
                    Console.WriteLine("Number too large");
                }
            }
        }
    }
}