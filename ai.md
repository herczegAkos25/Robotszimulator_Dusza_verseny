## Eszközök és modellek
- Claude Sonnet 5.5
- Gemini (Automatikus 3-modellek)

## Mire használtuk
- A feladat és a MineScript nyelv szabályainak áttekintése és értelemzése.
- A tesztek elkészítése és a háttérrendszer ellenőrzése.
- Hibakeresés
- Egyszerűsítés.
- Az AI a kód mekkora részét írta: 40%

## Hogyan ellenőriztük
- A kódot nem fogadtuk el ránézésre: az interpretert 60 saját teszttel ellenőriztük. A tesztek elvárt kimenetét a feladatleírás szabályaiból és a PDF-ben szereplő példanaplóból vezettük le, nem az interpreter saját kimenetéből.
- A blokkszerkesztő a felületet kézzel próbáltuk ki.
- Az AI által módosított fájlokat mindig Visual Studióban fordítottuk le és futtattuk, a hibákat visszaküldtük javításra.

## 5 konkrét eset
1. **Tesztkészlet:** az AI a specifikáció szabályaiból 60 tesztet és elvárt naplót írt. A futtatás után mind a 60 egyezett, így nem csak a példa, hanem a szélsőséges esetek is ellenőrizve lettek.
2. **Betöltés gomb hibája:** egy létrehozott betöltés gomb után a felület nem változott. Az AI négy okot talált (nem állította be az útvonalat, rossz metódust hívott, csak a térképet rajzolta újra, a program- és állapotjelzőket nem frissítette), és javított kódot adott. Kipróbáltuk, betöltés után a térkép, a program és a napló is frissül.
3. **Látvány terv készítése:**  az AI generált különböző látvány terveket, amikből inspirálódtunk. 
4. **A videós forgatókönyv szerinti átnézés (sikeres):** a szöveges forgatókönyv alapján kiderült, hogy a GUI nem írja ki a naplófájlt, a kézi vezérlés a nyomkövetés helyett a kiinduló helyzetből indul, és nincs „elé” beszúrás. Ellenőrzés: a nyomkövetés 2. lépéséből indított kézi vezérlés ott folytatta a robotot, ahol az állt.
5. **Léptetés rendszer:** az AI segítségével lefejlesztettük a léptetés rendszert, ami segíti a robot nyomon követését.

