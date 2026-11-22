## Kérdéseink fontossági sorrendben

1. Mennyi a robot kezdő energiája (ha van energiakorlát), és hány egységet fogyaszt a fúrás, a támasz elhelyezése, a lépés, a fordulás és a bányászás? Ahol nem fogy energia, ott 0-t kérünk.
2. Melyik mezőt fúrja a robot: a robot előtti mezőt? A fúrás mozgatja-e a robotot? (Az 5. válasz szerint a robot „fúrás után mezőre kerül”.)
3. Mi marad a fúrt mezőből, és az aranyat tartalmazó kőzetből külön? Javaslatunk: üres kamra, illetve aranyrög, amit a mine here-rel kell kibányászni.
4. Az I. fordulóban az aranyrög (@) szabadon álló mező volt. Az új térképeken előfordulhat még ilyen, vagy az arany mindig kőzetbe zárva van?
5. A beomlás szempontjából a 8 szomszéd közül mi számít üresnek: az üres kamra és az aranyrög is?
6. Mikor vizsgáljuk a beomlást: csak fúrás után, a robot saját mezőjére, vagy minden lépés után az összes mezőre?
7. Hová tehető támasz: csak az üres kamrába vagy az aranyrögre is?
8. Átfúrható-e a támasszal ellátott mező?
9. Szerepelhet-e támaszos mező már a bemeneti térképen?
10. Korlátozott-e a támaszok száma?

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
- `put L i v` felülírja az i. elemet v értékkel. Érvénytelen index: `E_INDEX_OUT_OF_RANGE`.
- `set x to item L i` az i. elemet adja. Érvénytelen index: `E_INDEX_OUT_OF_RANGE`.
- `set x to size L` az elemszámot adja.
- Nem létrehozott lista használata `E_UNINITIALIZED`.

### Naplózás

`push` esetén a napló a sor számát, a `push` szót és az értéket írja (`6 push true`), `put` esetén az indexet és az értéket. A `list`, `item` és `size` sorokban csak a sor száma szerepel, mint a `set` esetében.

### Visszafelé kompatibilitás

A `list`, `push`, `put`, `item`, `size`, `int`, `bool` szavak nem kerülnek a lefoglalt szavak közé. Az új utasítások olyan szóval kezdődnek, amivel I. fordulós utasítás nem kezdődhetett, a `set` jobb oldalán az új alakok olyan tokenszámúak és -alakúak, amilyenek az I. fordulós nyelvben érvénytelenek voltak. Így minden I. fordulós program ugyanazt a naplót adja, mint korábban.
