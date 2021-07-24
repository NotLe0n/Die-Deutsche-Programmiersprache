using System;

namespace DDP
{
    static class Fehlermeldungen
    {
        public const string opSameType = "Operanden müssen den gleichen Typ besitzten!";
        public const string opOnlyInt = "Operanden können nur integrale Zahlen sein!";
        public const string opOnlyNum = "Operanden können nur Zahlen sein!";
        public const string opOnlyNumOrString = "Operanden können nur Zahlen oder Zeichenketten sein!";
        public const string opInvalid = "Ungültiger Operanden Typ!";
        public const string onlyCallFunc = "Man kann nur Funktionen aufrufen!";
        public const string forWrongType = "für anweisung nimmt nur Nummern als Typ!";
        public const string forNoVar = "eine für anweisung braucht eine Variablen Deklaration";
        public const string dotAfterVarDeclaration = "Satzzeichen beachten! Ein Punkt muss nach einer Variablen Deklaration folgen!";
        public const string dotAfterExpression = "Satzzeichen beachten! Ein Punkt muss nach einer Anweisung folgen!";
        public const string varNameExpected = "Es wurde einen Variablen-name erwartet!";
        public const string varInvalidAssignment = "Ungültiger Zuweisungsziel!";
        public const string varAlreadyExists = "Eine Variable mit dem selben Namen existiert schon!";
        public const string varDefineInInit = "Eine variable kann nicht in seinem eigenen initialisierer gelesen werden";
        public const string funcNameExpected = "Funktions Name erwartet!";
        public const string argumentNameExpected = "Argumentname erwartet!";
        public const string parameterParenMissing = "Es wurde eine ')' nach einem Funktions Aufruf erwartet!";
        public const string tooManyArguments = "Eine Funktion kann nicht mehr als 255 argumente haben!";
        public const string returnTypeInvalid = "Ungültiger Rückgabetyp!";
        public const string returnTypeWrong = "Falscher Rückgabetyp!";
        public const string returnNotInFunc = "Eine Rückgabe-Anweisung kann nur in einer Funktion vorkommen!";
        public const string returnMissing = "Eine Funktion mit einem Rückgabe typ braucht eine Rückgabe Anweisung!";
        public const string expressionMissing = "Ausdruck erwartet!";
        public const string charTooLong = "Ein Zeichen kann nur einen zeichen groß sein! Benutzte eine Zeichenkette wenn du mehr willst!";
        public const string charUnterminated = "Unterminierter Zeichen!";
        public const string stringUnterminated = "Unterminierte Zeichenkette!";
        public const string groupingParenMissing = "Es wurde eine ')' nach einem Ausdruck erwartet!";
        public const string ifKommaMissing = "Komma nach wenn bedingung erwartet!";
        public const string ifDannMissing = "Komma nach wenn bedingung erwartet!";
        public const string ifConditionNotBool = "Die Bedingung einer wenn Anweisung muss ein Boolean sein!";
        public const string whileConditionNotBool = "Die Bedingung einer solange Anweisung muss ein Boolean sein!";
        public const string noArtikel = "Es wird ein Artikel vor einem Variablen Typ erwartet!";

        public static Func<string, string, string> unaryOpWrongType = (string op, string typ) => $"Der {op} operator nimmt nur {typ}!";
        public static Func<string, string, string> varWrongType = (string name, string typ) => $"Der variable {name} kann nur {typ} zugewiesen werden!";
        public static Func<string, string> varNotDefined = (string name) => $"Die Variable {name} wurde noch nicht definiert!";
        public static Func<int, int, string> wrongParamCount = (int expected, int count) => $"Es wurden {expected} argumente erwartet, aber {count} argumente gegeben!";
        public static Func<string, string, string> tokenMissingAtEnd = (string anweisung, string token) => $"am Ende {anweisung} wird ein {token} erwartet!";
        public static Func<string, string, string> tokenMissing = (string anweisung, string token) => $"Nach {anweisung} wird {token} erwartet!";
        public static Func<string, string, string> wrongArtikel = (string artikel, string typen) => $"der Artikel {artikel} passt nur {typen}!";
        public static Func<string, string, string> castInvalid = (string val, string typ) => $"man kann '{val}' nicht in {typ} umwandeln!";
    }
}
