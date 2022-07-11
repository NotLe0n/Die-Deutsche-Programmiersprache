# Dieser Repositorium ist [umgezogen](https://github.com/DDP-Projekt/Kompilierer). Diese Version von DDP ist nun veraltet!

# Willkommen zur Deutschen Programmiersprache!
<img 
     src="https://user-images.githubusercontent.com/26361108/130243423-2e78015a-279a-4906-9804-c2f23a78f5b4.png" 
     alt="ddp icon"
     align="right"
/>

Die Deutsche Programmiersprache **(DDP)** ist eine (Witz-)Programmiersprache von NotLe0n und bafto, die so entworfen wurde, dass in ihr geschriebene Programme so gelesen werden können wie (annähernd) korrektes Deutsch.

Momentan existieren zwei verschiedene Implementationen, die hier gefunden werden können:
- [DDP in c#](https://github.com/NotLe0n/Die-Deutsche-Programmiersprache)
- [DDP in c++](https://github.com/bafto/DDP-cpp) (schneller)

Bugs gerne in den Repositorien der jeweiligen Versionen als Issue einreichen.

### Bitte besuche das [Wiki](https://github.com/NotLe0n/Die-Deutsche-Programmiersprache/wiki) um mehr zu erfahren!

***

Man kann DDP auch online ausprobieren:
- [DDP Playground](https://github.com/bafto/DDP_Playground)

DDP Syntax-Hervorhebung für Visual Studio Code:
- [DDP Highlighting](https://github.com/NotLe0n/DDP-highlighting)

***

Ein Programm geschrieben in der Deutschen Programmiersprache könnte so aussehen:
```
// Fizz Buzz

für jede Zahl i von 1 bis 100, mache:
    wenn i modulo 3 gleich 0 ist und i modulo 5 gleich 0 ist, dann:
        schreibeZeile("FizzBuzz").
    sonst:
        wenn i modulo 3 gleich 0 ist, dann:
            schreibeZeile("Fizz").
        wenn aber i modulo 5 gleich 0 ist, dann:
            schreibeZeile("Buzz").
        sonst:
            schreibeZeile(i).
lese(). // damit sich die Konsole nicht sofort schließt
```
***
### Die Deutsche Programmiersprache in C# benutzen.

(Zuvor muss zuerst die DDP-Codesammlung/Library in das Projekt hinzugefügt werden.)
```c#
string meinTestString = @"Dein Code"

DDP.DDP.Ausführen(meinTestString); // Codestring ausführen.
DDP.DDP.DateiAusführen(pfadZurDatei); // Codedatei ausführen.
```

***
#### [english README](https://github.com/NotLe0n/Die-Deutsche-Programmiersprache/blob/master/README-EN.md)
***
