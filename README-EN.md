# This repository has been moved [here](https://github.com/DDP-Projekt/Kompilierer). This version of DDP is now outdated!

# Welcome to "Die Deutsche Programmiersprache"
<img 
     src="https://user-images.githubusercontent.com/26361108/130243423-2e78015a-279a-4906-9804-c2f23a78f5b4.png" 
     alt="ddp icon"
     align="right"
/>

"Die Deutsche Programmiersprache" **(DDP)** is a (joke-)programming language by NotLe0n and bafto, which was designed so that programs written in it read like (almost) proper german.

There exist two implementations, which you can find here:
- [DDP in c#](https://github.com/NotLe0n/Die-Deutsche-Programmiersprache)
- [DDP in c++](https://github.com/bafto/DDP-cpp) (faster)

please report bugs in their respective repositories as an issue.

### For more information please visit the [Wiki](https://github.com/NotLe0n/Die-Deutsche-Programmiersprache/wiki)!
***

You can also try DDP out online:
- [DDP Playground](https://github.com/bafto/DDP_Playground)

DDP syntax highlighting:
- [DDP Highlighting](https://github.com/NotLe0n/DDP-highlighting)

***

A program written in DDP could look like this:
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
### Use DDP in C# 

(The DDP library has to be added to the project beforehand.)
```c#
string myTestString = @"Your code"

DDP.DDP.Ausführen(myTestString); // run code string.
DDP.DDP.DateiAusführen(pathToFile); // run code file.
```
