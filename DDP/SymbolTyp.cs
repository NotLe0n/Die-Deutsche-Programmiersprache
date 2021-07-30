namespace DDP
{
    public enum SymbolTyp
    {
        // Einzelne Charaktere
        L_KLAMMER, R_KLAMMER, KOMMA, PUNKT, BANG_MINUS, DOPPELPUNKT,

        // Artikel
        DER, DIE, DAS,

        // Typen
        ZAHL, KOMMAZAHL, BOOLEAN, ZEICHENKETTE, ZEICHEN, ZAHLEN, KOMMAZAHLEN, ZEICHENKETTEN, 

        // Literals.
        IDENTIFIER, INT, FLOAT, STRING, CHAR, WAHR, FALSCH, INTARR, FLOATARR, STRINGARR, CHARARR, BOOLEANARR, 

        // Mathematische operatoren
        IST, SIND, PLUS, MINUS, MAL, DURCH, MODULO, HOCH, WURZEL, LOG, BETRAG, AN, STELLE,

        // Trigonometrische Funktionen
        SINUS, KOSINUS, TANGENS, ARKUSSINUS, ARKUSKOSINUS, ARKUSTANGENS, HYPERBELSINUS, HYPERBELKOSINUS, HYPERBELTANGENS,

        // Bitweise operatoren
        LOGISCH, KONTRA, UM, BIT, NACH, LINKS, RECHTS, VERSCHOBEN,

        // Vergleichs operatoren
        NICHT, UNGLEICH, GLEICH,
        GRÖßER, GRÖßER_GLEICH,
        KLEINER, KLEINER_GLEICH,
        ALS,

        // logische operatoren
        UND, ODER,

        // verzweigungen
        WENN, SONST, ABER, DANN,

        // schleifen
        FÜR, SOLANGE, MACHE, JEDE, VON, BIS, SCHRITTGRÖßE, MIT,

        // funktionen
        FUNKTION, GIB, MACHT, ZURÜCK, VOM, TYP,

        // Konstante
        PI, E, TAU, PHI,

        EOF
    }
}
