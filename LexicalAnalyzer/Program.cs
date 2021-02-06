using System;
using System.Collections.Generic;
using System.IO;

namespace LexicalAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo keyPressed;
            do
            {
                string Input;
                try
                {
                    Input = File.ReadAllText("../../../Code.txt");
                }
                catch (Exception)
                {
                    Console.WriteLine("\n File Code.txt not found");
                    Console.ReadKey();
                    throw new Exception();
                }
                var FormatInput = Input.Replace("\r\n", " ");
                var keywords = new List<string> { "procedure", "var", "string", "integer", "byte", "Begin", "Length", "for", "to", "do", "if", "in", "then", "End" };
                var specialSymbols = new List<string> { ".", ":", ";", ",", ":=", "[", "]", "'", "..", "(", ")" };
                var lexer = new FiniteStateMachine(keywords, specialSymbols);
                lexer.Analyze(Input);
                Print(lexer);
                static void Print(FiniteStateMachine analyzer)
                {
                    PrintTable(" \n -------------\n Keywords\n -------------", analyzer.KeywordsTable);
                    Console.WriteLine("\n");
                    PrintTable(" -------------\n Literals\n -------------", analyzer.LiteralsTable);
                    Console.WriteLine("\n");
                    PrintTable(" -------------\n Identifiers\n -------------", analyzer.IdentifiersTable);
                    Console.WriteLine("\n");
                    PrintAllLexems(analyzer.Lexemes);
                }
                static void PrintTable(string name, List<string> tables)
                {
                    Console.WriteLine(name);
                    foreach (var item in tables)
                    {
                        Console.WriteLine($" {item}");
                    }
                }
                static void PrintAllLexems(List<Lexeme> lexemes)
                {
                    Console.WriteLine(" -------------\n Lexems\n -------------");
                    Console.WriteLine(String.Format("{0,-12} | {1,-12}| {2,-12}", " Type", " Index", " Lexeme"));
                    Console.WriteLine(" ------------------------------------------");
                    foreach (Lexeme item in lexemes)
                    {
                        Console.WriteLine(String.Format(" {0,-12}| {1,-12}| {2,-12}", item.type, item.index, item.lexeme));
                    }
                }
                keyPressed = Console.ReadKey();
            } while (keyPressed.Key != ConsoleKey.Enter);
            Console.Clear();
        }
    }
}