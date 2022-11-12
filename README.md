# CommunityLib
A community library that contains tools for the modding of Shadows of Forbidden Gods

This mod must be loaded before, higher in the load order, any mod that is dependant on it.

## Cacher (WIP)
The only tool currently in developement is a cacher.  
At the start of each turn, it iterates over the game's data and sorts it hierarchically into dictionaries, for quick and easy access by all dependant mods.
The goal of this system to is to vastly reduce the number of times that mods have to iterate over the game's data structures, instead allowing them to access precisely  what they want, when they want it, as cheaply as possible.

### Constraints
#### Once Per Turn
The system only runs at the start of each turn, so any changes to the data structure that occur during a turn will not be represented until the start of the following turn.

#### Interface to Type Casting
Due to the use of reflection to collect the data, almost all results are extracted as IList or IDictionary interfaces of unknown types. In order to resolve this, the result must be cast to the expected type.

```csharp
// Example Code
# using CommunityLib

public class exampleMod : Assets.Code.Modding.ModKernel
{
    public void exampleMethod()
    {
        List<UAE_Warlord> = CommunityLib.ModCore.cache.commandableUnitsBySocialGroupByType[exampleOrcSocialGroup][typeof(UAE_Warlord)] as List<UAE_Warlord>;
    }
}

```
This step must be performed manually and accurately. Failure to do so will result in a runtime crash.
