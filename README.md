# CommunityLib
A community library that contains tools for the modding of Shadows of Forbidden Gods. You should only have it enabled if you have a mod enabled that is dependant on it, or you are developing a mod that is dependant on it.

## Features
It currently contains the follwoing features:

### Cacher
The cacher is a collection of iterators that run once per turn, in the `onTurnStart` hook. They iterate over the locations, social groups and units in the game and sort them into a wide variety of 1D and 2D Dictionaries and Lists.

This data can be accessed by any mod that is depndant on the CommunityLib, allowing them to greatly reduce the amount of iteration they need to do on a mod-by-mod basis.

### UAEN Override AI (Game Version 0.12 and later)
The UAEN Override AI is a set of clones of the base game's hardcoded AI for the UAEN subtypes (Unit Agent Evil Neutral). These AI, unlike the base game's AI, are accompanie by a custom challenge type list, which mods that are dependant on the CommunityLib can access, read from and write to. By adding a challenge type to this list, the AI will consider it among the base game's options when making decisions.

The override AI will only run for each type of UAEN that there are custom challenge types for.

This mod must be loaded before, higher in the load order, any mod that is dependant on it. It is recommended to place this mod directly under Core.
