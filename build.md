## Környezet
- Platform: Windows 10/11 (x64). A program WPF-es, ezért csak Windowson fordítható és futtatható.
- Fejlesztői környezet: Visual Studio 2026.
- Keretrendszer: .NET 10 (`net10.0-windows`), nyelv: C#, UI: WPF.
- Futtatáshoz a gépen telepítve kell lennie a **.NET 10 Desktop Runtime**-nak (x64). Visual Studio 2026 telepítésével ez általában megvan. Ha nincs, a program nem indul el. A parancssori fordításhoz a .NET 10 SDK kell.
- Külső csomag: `WPF-UI` 4.3.0 (NuGet). Az első fordításkor a NuGet automatikusan letölti, ehhez internetkapcsolat kell. Lefordított változat nincs a beadott mappában, a `robot.exe` a fordítás után jön létre.

## Fordítás
Visual Studióból:
1. Nyisd meg a `Robotszimulator.slnx` fájlt.
2. Build > Rebuild Solution (Debug vagy Release).

Parancssorból (a projekt mappájában):

    dotnet build Robotszimulator.csproj -c Release

A kimenet neve `robot.exe` (a `Robotszimulator.csproj`-ban `AssemblyName` = `robot`):
- Debug: `bin\Debug\net10.0-windows\robot.exe`
- Release: `bin\Release\net10.0-windows\robot.exe`

## Futtatás
- Grafikus felület (IDE), a megadott fájlokkal:

      robot.exe input_001.txt output_001.txt

  Az output fájlba a program minden új futtatáskor felülírja a naplót, nyomkövetésnél az aktuális lépésig.
  
- Csendes mód (felület nélkül, a napló kiírása után kilép):

      robot.exe -q input_001.txt output_001.txt

- Argumentum nélkül indítva vagy a `robot.exe`-re dupla kattintás után a Betöltés gombbal lehet fájlt választani.

## Mappaszerkezet
- `Core\` – nyelv-értelmező (interpreter), robot, térkép, naplózás, blokkprogram modell
- `UI\` – segédablakok (blokk hozzáadása/szerkesztése, mentés, beállítások, indítóképernyő)
- `Képek\` – a térkép és a robot képei, a fordítás a kimeneti mappába másolja őket
- `MainWindow.*` – főablak (szimulátor, blokk- és szöveges szerkesztő, nyomkövetés, kézi vezérlés)
- `App.xaml.cs` – indítás, parancssori paraméterek, csendes mód
