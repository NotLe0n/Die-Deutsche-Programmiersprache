using System;
using System.Collections.Generic;
using System.IO;

namespace DDP
{
    class DDP
    {
        private static readonly Interpreter interpreter = new();

        private static bool hatteFehler = false;
        private static bool hatteLaufzeitfehler = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Verwendung: DDP [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                DateiAusführen(args[0]);
            }

            // wenn es einen Fehler gab -> Programm nicht schließen
            if (hatteFehler || hatteLaufzeitfehler)
                Console.Read();
        }

        private static void DateiAusführen(string pfad)
        {
            string str = File.ReadAllText(pfad);
            Ausführen(str);

            if (hatteFehler) return;
            if (hatteLaufzeitfehler) return;
        }

        private static void Ausführen(string quelle)
        {
            Scanner scanner = new Scanner(quelle);
            List<Symbol> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Anweisung> statements = parser.Parse();

            // Aufhören wenn es einen Fehler gab
            if (hatteFehler) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            // Aufhören wenn es einen Laufzeitfehler gab
            if (hatteFehler) return;

            interpreter.Interpret(statements);
        }

        public static void Fehler(int zeile, string nachricht)
        {
            FehlerMelden(zeile, 0, "", nachricht);
        }

        public static void Fehler(Symbol token, string nachricht)
        {
            if (token.typ == SymbolTyp.EOF)
            {
                FehlerMelden(token.zeile, token.position, "am ende", nachricht);
            }
            else
            {
                FehlerMelden(token.zeile, token.position, "bei '" + token.lexeme + "'", nachricht);
            }
        }

        public static void Laufzeitfehler(Laufzeitfehler fehler)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (fehler.symbol == null)
            {
                Console.Error.WriteLine($"Laufzeitfehler: {fehler.Message}");
            }
            else
            {
                Console.Error.WriteLine($"[{fehler.symbol.zeile}, {fehler.symbol.position}] Laufzeitfehler bei '{fehler.symbol.lexeme}' : {fehler.Message}");
            }

            Console.ResetColor();
            hatteLaufzeitfehler = true;
        }

        private static void FehlerMelden(int zeile, int position, string ort, string nachricht)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[{zeile}, {position}] Fehler {ort} : {nachricht}");
            Console.ResetColor();
            hatteFehler = true;
        }
    }
}
