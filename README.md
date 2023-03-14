# CommunityLib
CommunityLib is a community library that contains tools for the modding of Shadows of Forbidden Gods.

These tools aim to make dependant mods more performant, easier to develop, and to allow for a greater degree of interoperability.

This Library will significantly increase the time it takes to generate a new game, but should be unnoticeable during play. Performance is likely to decrease on simple mods, but increase for larger mod lists of dependant mods, or for very complex dependant mods.

You should only have CommunityLib enabled if you have a mod enabled that is dependant on it, or you are developing a mod that is dependant on it.

*[See the project wiki for more details.](https://github.com/ilikegoodfood/CommunityLib/wiki)*

**This mod must be loaded before, higher in the load order, any mod that is dependant on it. It is recommended to place this mod directly under Core.**

**NOTE:** If in a game and wishing to start a new game, please quit to desktop and relaunch the game before doing so. Some users have found that mods do not function properly if this step is not taken.

## Features
It currently contains the follwoing features:

### Helper Functions
The Community Library includes a number of helper classes and functions which can be used by other mods.
#### Task_GoPerformChallengeAtLocation
This class operates in the same way as the base game's `Task_GoPerformChallenge`, but it can also handle performing rituals at locations other than the agent's current location, and it can make use of safeMove.

### Universal Agent AI (Documention is WIP)
The Community Library now features its very own universal agent AI. By making use of a data class called AIChallenge, it is possible to introduce new challenges and rituals to agent types that are using this AI, along with custom profile, validity, and utility functions just for them. The AIChallenge also allows the use of tags (`AIChallenge.challengeTags`) to access a large collection of pre-built validity and utility checks, removing the need to repeeatedly clone common code chunks from the core game.

Multiple mods can all manipulate the AI of a single agent that is making use of this AI.

### UAEN Override AI
The basse games UAEN (Deep Ones, Ghasts, Orc Upstarts, and Vampires) make use of hardcoded AI. All agents of those types now use the Universal Agent AI, which have been configured to closely, but not exactly, mimic their base game behaviour. The changes are small, resulting these agent types behaving slightly more intelligently.

### Custom Hooks (Documention is WIP)
Using the Harmony Library, the community library now implements a large, and rapidly growing, number of custom hooks. These hooks operate much like those already available in the ModKernal.

To implement them in your mod, create aan instance of a class that inherits the `CommunityLib.Hooks` class, and call `RegisterHooks()` on the CommunityLib ModKernel. This must be done once when starting a new game, or when loading a game.
