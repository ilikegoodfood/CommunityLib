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
The compiler should give you a type conversion error if you have failed to do so.

### Code Example
This is an example of code that gets all the player controlled warlords belonging to a specific orc social group

```csharp
// Example Code
# using CommunityLib

public class exampleMod : Assets.Code.Modding.ModKernel
{
    // A method that pulls data from the CommunityLib cache.
    public void exampleMethod()
    {
        // Always check to see if a key is present before getting it's value.
        // The version of Unity's C# framework that is packaged with the game does not implement IDictionary.TryGetValue(key), thus this 'if contains key get value' structure is neccesary.
        List<UAE_Warlord> warlords;
        if (CommunityLib.ModCore.cache.commandableUnitsBySocialGroupByType.Contains(exampleOrcSocialGroup) && CommunityLib.ModCore.cache.commandableUnitsBySocialGroupByType[exampleOrcSocialGroup].Contains(typeof(UAE_Warlord)))
        {
            // Always cast the returned list to the expected type. If you use the wrong types here, you will generate runtime errors that will get past the compiler.
            warlords = CommunityLib.ModCore.cache.commandableUnitsBySocialGroupByType[exampleOrcSocialGroup][typeof(UAE_Warlord)] as List<UAE_Warlord>;
        {
        
        // Always chek that you got a value back before operating on it.
        if (warlords != null)
        {
            // Do stuff with player controlled warlords here...
        }
    }
    
        // An example method that demonstartes the difference between a cache dictionary and it's exclusive counterpart.
    public void exampleMethodExclusive()
    {
        List<HolyOrder> holyOrders;
        if (CommunityLib.ModCore.cache.socialGroupsByType.Contains(typeof(HolyOrder)))
        {
            holyOrders = CommunityLib.ModCore.cache.socialGroupsByType[typeof(HolyOrder)] as List<HolyOrder>;
            // The list value socialGroupsByType[typeof(HolyOrder)] includes everything that is of type HolyOrder or any of its subTypes. In the unmodded game, this will include human holy orders, witch's holy orders, and orphanim's holy orders.
        {
        
        if (holyOrders != null)
        {
            // Do stuff with holyOders here...
        }
        
        List<HolyOrder> humanHolyOrders;
        if (CommunityLib.ModCore.cache.socialGroupsByTypeExclusive.Contains(typeof(HolyOrder)))
        {
            // The list value socialGroupsByTypeExclusive[typeof(HolyOrder)] includes everything that is of exactly type HolyOrder, not include any of it's subTypes. In the unmodded game, this will only contain human holy orders.
            holyOrders = CommunityLib.ModCore.cache.socialGroupsByTypeExclusive[typeof(HolyOrder)] as List<HolyOrder>;
        {
        
        if (humanHolyOrders != null)
        {
            // Do stuff with holyOders here...
        }
    }
}

```
