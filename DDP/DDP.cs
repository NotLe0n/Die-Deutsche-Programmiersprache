using System;
using System.Collections.Generic;
using System.IO;

namespace DDP
{
    class DDP
    {
        private static readonly Interpreter interpreter = new();

        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Verwendung: DDP [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }

            RunPrompt();
            Console.Read();
        }

        private static void RunFile(string path)
        {
            string str = File.ReadAllText(path);
            Run(str);

            if (hadError) return;
            if (hadRuntimeError) return;
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                hadError = false;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n> ");
                Console.ResetColor();
                string line = Console.ReadLine();

                if (line == null) break;

                Run(line);
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Statement> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            // Stop if there was a resolution error.
            if (hadError) return;

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, 0, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, token.position, "am ende", message);
            }
            else
            {
                Report(token.line, token.position, "bei '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (error.token == null)
            {
                Console.Error.WriteLine($"Laufzeitfehler: {error.Message}");
            }
            else
            {
                Console.Error.WriteLine($"[{error.token.line}, {error.token.position}] Laufzeitfehler bei '{error.token.lexeme}' : {error.Message}");
            }

            Console.ResetColor();
            hadRuntimeError = true;
        }

        private static void Report(int line, int position, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[{line}, {position}] Fehler {where} : {message}");
            Console.ResetColor();
            hadError = true;
        }
    }
}
