# Rename & Hide Asteroids

A mod for **Oxygen Not Included: Spaced Out!** It lets you rename asteroids and hide the ones you don't need from the asteroid list on the right, so the list no longer takes up half the screen.

*Русская версия: [README.ru.md](README.ru.md)*

## What it does

### Renaming

Select an asteroid on the Starmap and click the pencil in the title of its panel, just like for rockets and Duplicants. The new name shows in the list on the right, on the Starmap and everywhere else in the game.

- The name is stored in the asteroid itself, so it stays even if you disable the mod.
- The characters `< > : " / \ | ? *` and a dot or space at the end are removed from the name: the game makes a timelapse folder from the asteroid's name, and Windows doesn't allow these characters in folder names.

### Hiding from the list

Hover over an asteroid row and click **Hide**. Hidden asteroids go under the **+ Hidden asteroids (N)** link at the bottom of the list: click it to see them, and click **Show** to bring an asteroid back to the list.

- Rockets landed on a hidden asteroid are hidden with it. Rockets in flight and in orbit stay in the list.
- While the camera is on a hidden asteroid, it shows under the link.
- The starting asteroid can't be hidden.
- Hotkeys (BACKQUOTE + number) work for hidden asteroids too.
- The list of hidden asteroids is saved with each colony.

### Alerts

A hidden asteroid with a red alert temporarily comes back under the link, with the game's sound and wobble, until the alert is over. The sound plays only once. Yellow alerts are off by default: you still see them when you open the list of hidden asteroids, and you can turn them on in the settings.

A red alert can happen even on an empty asteroid, for example during a meteor shower or when a rocket can't land. You can turn off the alerts you don't need for each asteroid in the game's own diagnostics (**Diagnostics → See All**).

## Settings

In the **Mods** menu, click **Options** next to the mod:

- **Hide asteroids** (on). When off, the mod only renames, and the list stays as in the base game. Hidden asteroids are remembered and come back when you turn it on again.
- **Show a hidden asteroid**: **On red alert** (on), **On yellow alert** (off). These work only while **Hide asteroids** is on.

Changes apply when you load a save, no game restart needed. The settings file is kept in the game's shared mod settings folder, so it survives mod updates.

## Compatibility

- Requires the **Spaced Out!** DLC: the asteroid list exists only there.
- Can be added to an existing game and disabled at any time: new names stay, and the list goes back to normal.
- Built to survive game updates: the mod uses the game's own panels and entry points, and the Hide / Show text sits exactly where the game puts its own status text.

## Languages

English and Russian. The Russian text lives in `translations/ru.po`; other languages can be added the same way.

## Credits

The settings window is made with [PLib](https://github.com/peterhaneve/ONIMods) by Peter Han (MIT license, see [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)).

This mod was made with the help of AI.

## Building from source

You need the .NET SDK ([dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)). Close the game before building.

1. Once per computer, create `GamePath.props` in the folder that holds the mod repositories (one level above this one) with the path to the game, the folder with `OxygenNotIncluded.exe`:

   ```xml
   <Project>
     <PropertyGroup>
       <GameDir>C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded</GameDir>
     </PropertyGroup>
   </Project>
   ```

   This file is not stored in git.

2. Build in the mod folder with `dotnet build` (or Ctrl+Shift+B in VS Code). PLib is merged into the mod library during the build with ILRepack; both are in the `lib` folder, so the build needs no internet. The finished mod is copied to `Documents\Klei\OxygenNotIncluded\mods\Local\RenameHideAsteroids`.

3. Start the game, enable **Rename & Hide Asteroids** in Mods and restart.

## License

MIT, see [LICENSE](LICENSE).
