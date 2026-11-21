## Kérdéseink fontossági sorrendben

1. Melyik mezőt fúrja a robot: a robot előtti mezőt? A fúrás mozgatja-e a robotot? (Az 5. válasz szerint a robot „fúrás után mezőre kerül”.)
2. Mi marad a fúrt mezőből, és az aranyat tartalmazó kőzetből külön? Javaslatunk: üres kamra, illetve aranyrög.
3. A beomlás szempontjából a 8 szomszéd közül mi számít üresnek: az üres kamra és az aranyrög is?
4. Mikor vizsgáljuk a beomlást: csak fúrás után, a robot saját mezőjére, vagy minden lépés után az összes mezőre?
5. Van-e energiakorlát (kezdő energia, ami elfogyhat), vagy csak a felhasznált energiát kell számolni?
6. Mennyi energiába kerül egy fúrás, és ha van korlát, mennyi a kezdőérték?
7. Fogyaszt-e energiát a step, a turn vagy a mine is, vagy csak a fúrás?
8. Hová tehető támasz: csak az üres kamrába vagy az aranyrögre is?
9. Kerül-e energiába a támasz elhelyezése, és ha igen, mennyibe?
10. Átfúrható kőzetbe lépés fúrás nélkül hibát (E_BEDROCK_HIT) ad-e?
11. Mi történik, ha a robot üres kamrát vagy aranyrögöt próbál fúrni: hiba, vagy nincs hatása?
12. Átfúrható-e a támasszal ellátott mező?
13. A beomlásnál a térképen kívüli mező üresnek számít-e?
14. Szerepelhet-e támaszos mező már a bemeneti térképen?
15. Korlátozott-e a támaszok száma?

## A nyelv bővítése

### Cél

1. Típusos, futásidőben növekvő lista, amelyben annyi adatot lehet tárolni, amennyit a bánya megkövetel.

### Nyelvtan

```
procedure = step | turn | mine | print | listdecl | push | put
function = isThere | item | size

listdecl = "list" ident type
type = "int" | "bool"
push = "push" ident simpleval
put = "put" ident simpleval simpleval

item = "item" ident simpleval
size = "size" ident
```

A tokenizálás nem változik, a sor továbbra is szóközökkel darabolódik.

### Szemantika

- `list L int` vagy `list L bool` létrehoz egy üres, 0-tól indexelt listát. Ha az L név már foglalt, `E_TYPE_MISMATCH` a hiba.
- `push L v` a lista végére fűzi v-t. Ha v típusa más, mint a listáé: `E_TYPE_MISMATCH`.
- `put L i v` felülírja az i. elemet. Érvénytelen index: `E_INDEX_OUT_OF_RANGE`.
- `set x to item L i` az i. elemet adja. Érvénytelen index: `E_INDEX_OUT_OF_RANGE`.
- `set x to size L` az elemszámot adja.
- Nem létrehozott lista használata `E_UNINITIALIZED`.

### Naplózás

`push` esetén a napló a sor számát, a `push` szót és az értéket írja (`6 push true`), `put` esetén az indexet és az értéket. A `list`, `item` és `size` sorokban csak a sor száma szerepel, mint a `set` esetében.

### Visszafelé kompatibilitás

A `list`, `push`, `put`, `item`, `size`, `int`, `bool` szavak nem kerülnek a lefoglalt szavak közé. Az új utasítások olyan szóval kezdődnek, amivel I. fordulós utasítás nem kezdődhetett, a `set` jobb oldalán az új alakok olyan tokenszámúak és -alakúak, amilyenek az I. fordulós nyelvben érvénytelenek voltak. Így minden I. fordulós program ugyanazt a naplót adja, mint korábban.
