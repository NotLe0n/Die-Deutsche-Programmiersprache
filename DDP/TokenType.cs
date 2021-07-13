namespace DDP
{
    public enum TokenType
    {
        IST,

        // Single-character tokens.
        L_KLAMMER, R_KLAMMER, KOMMA, PUNKT, BANG_MINUS, DOPPELPUNKT, TAB, STRICH,

        // Mathematische operatoren
        PLUS, MINUS, MAL, DURCH, MODULO, HOCH, WURZEL, LOG,

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

        // boolean
        WAHR, FALSCH,

        // Typen
        ZAHL, FLIEßKOMMAZAHL, BOOLEAN, ZEICHENKETTE, ZEICHEN,

        // Artikel
        DER, DIE, DAS,

        // Literals.
        IDENTIFIER, INT, FLOAT, STRING, CHAR,

        // funktionen
        FUNKTION, GIB, MACHT, ZURÜCK, VOM, TYP,

        // Amon Gus
        PRINT,

        // Konstante
        PI, E, TAU, 

        EOF
    }
}
