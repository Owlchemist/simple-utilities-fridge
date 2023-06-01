using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.Planet;
using static SimpleFridge.FridgeUtility;

namespace SimpleFridge
{
	//Change the perceived temperature
	[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.GetTemperatureForCell))]
	[HarmonyPriority(Priority.First)]
	class Patch_GetTemperatureForCell
	{
		static bool Prefix(Map map, IntVec3 c, ref float __result)
		{
			if (utilityCache.TryGetValue(map?.uniqueID ?? -1, out FridgeUtility fridgeUtility) && fridgeUtility.GetAdjustedTemperature(c))
			{
				__result = -10f;
				return false;
			}
			
			return true;
		}
    }

	//This handles cache registration
	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PostSpawnSetup))]
	class Patch_PostSpawnSetup
	{
		static void Postfix(CompPowerTrader __instance)
		{
			Map map = __instance.parent.Map;
			if (map != null && __instance.parent.def.HasModExtension<Fridge>() && utilityCache.TryGetValue(map.uniqueID, out FridgeUtility fridgeUtility))
			{
				fridgeUtility.fridgeCache.Add(__instance);
				fridgeUtility.UpdateFridgeGrid(__instance);
			}
		}
    }

	//This handles cache deregistration
	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PostDeSpawn))]
	class Patch_PostDeSpawn
	{
		static void Postfix(CompPowerTrader __instance, Map map)
		{
			if (map != null && utilityCache.TryGetValue(map.uniqueID, out FridgeUtility fridgeUtility) && fridgeUtility.fridgeCache.Contains(__instance))
			{
				__instance.powerOnInt = false; //Set the power off prior to the grid update
				fridgeUtility.UpdateFridgeGrid(__instance); //Update the bool grid
				fridgeUtility.fridgeCache.Remove(__instance); //Clean up the cache
			}
		}
    }

	//Update cache if a map is removed
	[HarmonyPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap_NewTemp))]
	class Patch_DeinitAndRemoveMap
	{
		static void Postfix(Map map)
		{
			if (map != null && utilityCache.Remove(map.uniqueID) && Prefs.DevMode) Log.Message("[Simple Utilities: Fridge] Map removal detected.");
		}
	}


	//This handles fridge grid cache construction
	[HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
	class Patch_ConstructComponents
	{
		static void Postfix(Map __instance)
		{
			if (__instance.uniqueID != -1 && __instance.info != null) new FridgeUtility(__instance);
		}
    }

	//We're patching here because it's a point of entry with the minimal overhead, unlikely to be bypassed by other mods, and only periodically ticked every 1000th tick.
    [HarmonyPatch(typeof(GoodwillSituationManager), nameof(GoodwillSituationManager.RecalculateAll))]
	class Patch_GoodwillSituationManager_RecalculateAll
	{
        static void Postfix()
        {
            foreach (var item in utilityCache.Values) item.Tick();
        }
    }

	//Flush the cache on reload
    [HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
	class Patch_World_FinalizeInit
	{
        static void Postfix()
        {
			utilityCache = new System.Collections.Generic.Dictionary<int, FridgeUtility>();
        }
    }
}