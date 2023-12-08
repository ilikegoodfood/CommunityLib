# CommunityLib
CommunityLib is a community library that contains tools for the modding of Shadows of Forbidden Gods.

These tools aim to make dependant mods more performant, easier to develop, and to allow for a greater degree of interoperability.

This Library will significantly increase the time it takes to generate a new game, but should be unnoticeable during play. Performance is likely to decrease on simple mods, but increase for larger mod lists of dependant mods, or for very complex dependant mods.

You should only have CommunityLib enabled if you have a mod enabled that is dependant on it, or you are developing a mod that is dependant on it.

*[See the project wiki for more details.](https://github.com/ilikegoodfood/CommunityLib/wiki)*

**This mod must be loaded before, higher in the load order, any mod that is dependant on it. It is recommended to place this mod directly under Core.**

## Features
It currently contains the follwoing features:

### Custom Hooks
Using the Harmony Library, the community library now implements a large, and rapidly growing, number of custom hooks. These hooks operate much like those already available in the ModKernal. These hooks cover a vast array of features, both from the base game and the Community Library itself.

To implement them in your mod, create aan instance of a class that inherits the `CommunityLib.Hooks` class, and call `RegisterHooks()` on the CommunityLib ModKernel. This must be done once when starting a new game, or when loading a game.

### Arbitrary Pathfinding
The Community Library contains a pathfinding solution that uses a delegate to determine location validity. This allows for arbitrary new pathing limitations to be implemented.

### Universal Agent AI
The Community Library now features its very own universal agent AI. By making use of intermediate data classes `AIChallenge` and `AiTask`, it is possible to introduce new challenges and rituals to agent types that are using this AI, along with custom profile, validity, and utility functions just for them. The `These classes also contain pre-built logical snippets, which can be individually enabled through the addition of Tags. This removes the need to repeatedly duplicate code for common base-game validity or utility checks.

Multiple mods can all manipulate the AI of a single agent that is making use of this AI.

#### UAEN Override AI
The basse games UAEN (Cordyceps' Drones, Deep Ones, Ghasts, Orc Upstarts, and Vampires) make use of hardcoded AI. All agents of those types now use the Universal Agent AI, which have been configured to closely, but not exactly, mimic their base game behaviour. The changes are small, resulting these agent types behaving slightly more intelligently.

### Orcs Expansion Control
A registry that allows control over which settlement types orcs can expand onto. This is useful for mods that add new settlement types, such as a new type of ruin, which orcs should reasonable be able to colonise.

### Mod Culture Data
A registry for culture specific overrides to the graphics for minor human settlement `Set_MinorHuman`. If data is provided, these graphics will be used in place of the vanilla option. A master toggle for this feature for this feature exists in the Community Library's mod config, but individual mods should have their own individual on-off options.

### Shipwrecks
Shipwrecks are Subsettlements that act much like ruins. They can be plundered (explored) multiple times, and can spawn when a city with a Dock is destroyed, or occasionally when a unit dies at sea, or The Bucanneer conduct naval actions. Unlike ruins, shipwrecks degrade over time, and whenever they are plundered, unless specific event outcomes are chosen.

By default, these are disabled through the mod configuration menu. Mods can force-enable them if they make specific use of shipwrecks. This means that different dependent mods can all use the same shipwreck subsettlement, event system, and spawn code, and thus aren't individually creating multiple unique shipwrecks for the same occurance.

### Utilities
The Community Library includes a number of helper classes and functions which can be used by other mods.

#### Cheat Codes
The Community Library offers a small number of useful cheat codes.

#### randStore
The randStore is a method for generating, storing, and using random values within the Universal Agent AI for profile, utility and validity values, and ensuring they remain consistent within the UI. It can also be used for other purposes.

#### Task_GoPerformChallengeAtLocation
This class operates in the same way as, and inherits from, the base game's `Task_GoPerformChallenge`, but it can also handle performing rituals at locations other than the agent's current location, and it can make use of safeMove.

#### Task_GoToWilderness
This class operates in the same way as Task_GoToLoation, but it will naviagte to the nearest unowned location. It can be configured to allow or disallow the destination being an ocean location, and can make use of safe move.




