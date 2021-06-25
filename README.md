# Typen
- ## int
  `die Zahl x ist 69.`

- ## float
  `die Fließkommazahl x ist 6,5.`

- ## bool
  `der Boolean x ist wahr.`

- ## string
  `die Zeichenkette x ist "abc".`

- ## char
  `das Zeichen x ist 'd'.`

- ## type[]
- - ### int[]
    `die Zahlen x sind [3; 1; 4].`

- - ### float[]
    `die Fließkommazahlen x sind [6,5; 3,4; 0,0].`

- - ### bool[]
    `die Boolean x sind [wahr; falsch; wahr].`

- - ### string[]
    `die Zeichenketten x sind ["hi"; "coole Programmiersprache"; "sedn nuds"].`
        
- - ### char[]
    `die Zeichen x sind ['g'; '⊻'; '3'].`

- - ### indexierung
    `die Zahl y ist x an der Stelle 3.`

# Mathematische Operatoren
- ## =
  `x ist 2.`

- ## +
  `x ist 1 plus 1.`

- ## -
  `x ist 10 minus 1.`

- ## *
  `x ist 1 mal 1.`

- ## /
  `x ist 1 durch 1.`

- ## %
  `x ist 1 modulo 1.`

- ## exponent
  `x ist 1 hoch 2.`

- ## root
  `x ist n. wurzel aus 9.`

# Mathematische Funktionen
- ## Math.PI
  `x ist Pi.`
  
- ## Math.E
  `x ist e hoch 5.`
  
- ## Math.Tau
  `x ist tau.` 
  
- ## Math.Log
  `x ist ln 2.`

- ## Math.Abs
  `x ist |-2|.`
  
- ## Math.Min
  `x ist die kleinere zahl von y und z.`
  
- ## Math.Max
  `x ist die größere zahl von y und z.`
  
- ## Math.Clamp
  `x ist zwischen 2 und 4.`
  
- ## Math.Truncate
  `x ist y ohne Kommazahlen.`
  
- ## Math.Round
  `x ist 5.29 auf 1 stelle gerundet.`

- ## Trigonometrische Funktionen

- - ### Math.Sin
  `x ist der Sinus von 2,4.`

- - ### Math.Cos
  `x ist der Kosinus von 2,4.`

- - ### Math.Tan
  `x ist der Tangens von 2,4.`

- - ### Math.Asin
  `x ist der Arkussinus von 2,4.`
  
- - ### Math.Acin
  `x ist der Arkuskosinus von 2,4.`
  
- - ### Math.Atan
  `x ist der Arkustangens von 2,4.`
  
- - ### Math.Sinh
  `x ist der Hyperbelsinus von 2,4.`
  
- - ### Math.Cosh
  `x ist der Hyperbelkosinus von 2,4.`
  
- - ### Math.Tanh
  `x ist der Hyperbeltangens von 2,4.`

# Bool'sche Operatoren
- ## &&
  ```
  wenn i modulo 3 0 ist und i modulo 5 0 ist, dann:
      der boolean x ist wahr.
  ```

- ## ||
  ```
  wenn i modulo 3 0 ist oder i modulo 5 0 ist, dann:
      der boolean x ist wahr.
  ```

- ## !
  ```
  wenn nicht 3 plus 2 0 ist und nicht 1 plus 1 11 ist, dann:
            der boolean x ist wahr.
  ```

# Vergleichsoperatoren (existieren nur in der wenn anweisung)
- ## ==
  ```
  der boolean x ist falsch.
  ```

  ```
  wenn i modulo 3 0 ist, dann:
      der boolean x ist wahr.
  ```

- ## !=
  ```
  der boolean x ist falsch.

  wenn i modulo 3 nicht 0 ist, dann:
      der boolean x ist wahr.
  ```

- ## <
  ```
  der boolean x ist falsch.

  wenn i modulo 3 kleiner als 0 ist, dann:
     der boolean x ist wahr.
  ```

- ## >
  ```
  der boolean x ist falsch.

  wenn i modulo 3 größer als 0 ist, dann:
      der boolean x ist wahr.
  ```

- ## <=
  ```
  der boolean x ist falsch.

  wenn i modulo 3 kleiner als, oder 0 ist, dann:
        der boolean x ist wahr.
  ```

- ## >=
  ```
  der boolean x ist falsch.

  wenn i modulo 3 größer als, oder 0 ist, dann:
      der boolean x ist wahr.
  ```

# Bitweise operatoren
- ## <<
  `die Zahl x ist y um 1 bit nach links verschoben.`
  
- ## >>
  `die Zahl x ist y um 1 bit nach rechts verschoben.`

- ## & 
  `die Zahl x ist logisch y und z.`

- ## |
  `die Zahl x ist logisch y oder z.`

- ## ~
  `die Zahl x ist logisch nicht y.`

# Verzweigungen
- ## if
  ```
  wenn 4 minus 2 2 ist, dann:
      <anweisung>.
  ```
    
- ## else
  ```
  wenn 4 minus 2 2 ist, dann:
      <anweisung>.
  sonst:
      <anweisung>.
  ```

- ## else if
  ```
  wenn 4 minus 2 2 ist, dann:
      <anweisung>
  wenn aber 1 plus 1 2 ist, dann:
      <anweisung>.
  sonst:
      <anweisung>.
  ```

# Schleifen
- ## for
  ### Hochzählende
  ```
  für jede Zahl i von 1 bis 100, mache:
      <anweisung>.
  ```
  ### Runterzählende
    ```
  für jede Zahl i von 100 bis 1, mache:
      <anweisung>.
  ```

- ## while
  ```
  solange i 5 ist mache:
      <anweisung>.
  ```

- ## do-while
  ```
  mache:
      <anweisung>.
  solange i 5 ist.
  ```

# basis funktionen
- ## Console.Write()
  `schreibe().`

- ## Console.WriteLine()
  `schreibeZeile().`

# Funktionen
```
funktion foo() macht:
  <Anweisungen>.
```

# Kommentare
- `// das ist ein Kommentar!`
- `/* das ist ein mehrzeiliges Kommentar! */`

# Fizzbuzz
```
für jede Zahl i von 1 bis 100, mache:
    wenn i modulo 3 0 ist und i modulo 5 0 ist, dann:
        schreibeZeile("FizzBuzz").
    sonst:
        wenn i modulo 3 0 ist, dann:
            schreibeZeile("Fizz").
        wenn aber i modulo 5 0 ist, dann:
            schreibeZeile("Buzz").
        sonst:
            schreibeZeile(i).
```

# Fehlermeldungen:
- ## Syntax Error
  `"[zeile x] Sprich Deutsch!"`
- ## Divide by 0 Error
  `"[zeile x] bruh, durch 0 kann man nicht teilen!"`
- ## Typ Error
  `"[zeile x] mischen kann man nicht..."`
- ## Punkt error
  `"[zeile x] Satzzeichen gibts auch noch!"`
