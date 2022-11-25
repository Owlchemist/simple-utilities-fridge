using HarmonyLib;
using Verse;
using RimWorld;
using System.Linq;
using static SimpleFridge.FridgeUtility;

namespace SimpleFridge
{
	//Change the perceived temperature
	[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.GetTemperatureForCell))]
	[HarmonyPriority(Priority.First)]
	public class Patch_GetTemperatureForCell
	{
		static public bool Prefix(Map map, IntVec3 c, ref float __result)
		{
			if (utilityCache.TryGetValue(map?.uniqueID ?? -1, out FridgeUtility fridgeUtility))
			{
				if (fridgeUtility.GetAdjustedTemperature(c))
				{
					__result = -10f;
					return false;
				}

			}
			
			return true;
		}
    }

	//This handles cache registration
	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PostSpawnSetup))]
	public class Patch_PostSpawnSetup
	{
		static public void Postfix(CompPowerTrader __instance)
		{
			if (__instance.parent.def.HasModExtension<Fridge>() && utilityCache.TryGetValue(__instance.parent.Map?.uniqueID ?? -1, out FridgeUtility fridgeUtility))
			{
				fridgeUtility.fridgeCache.Add(__instance);
				fridgeUtility.UpdateFridgeGrid(__instance, __instance.parent.Map);
			}
		}
    }

	//This handles cache deregistration
	[HarmonyPatch(typeof(CompPowerTrader), nameof(CompPowerTrader.PostDeSpawn))]
	public class Patch_PostDeSpawn
	{
		static public void Postfix(CompPowerTrader __instance, Map map)
		{
			if (utilityCache.TryGetValue(map?.uniqueID ?? -1, out FridgeUtility fridgeUtility) && fridgeUtility.fridgeCache.Contains(__instance))
			{
				__instance.powerOnInt = false; //Set the power off prior to the grid update
				fridgeUtility.UpdateFridgeGrid(__instance, map); //Update the bool grid
				fridgeUtility.fridgeCache.Remove(__instance); //Clean up the cache
			}
		}
    }

	//Update cache if a map is removed
	[HarmonyPatch(typeof(Game), nameof(Game.DeinitAndRemoveMap))]
	public class Patch_DeinitAndRemoveMap
	{
		static public void Postfix(Map map)
		{
			if (map != null && utilityCache.Remove(map.uniqueID)) Log.Message("[Simple Utilities: Fridge] Map removal detected.");
		}
	}


	//This handles fridge grid cache construction
	[HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
	public class Patch_ConstructComponents
	{
		static public void Prefix(Map __instance)
		{
			new FridgeUtility(__instance);
		}
    }


	//Every 600 ticks, check fridge room temperature and adjust its power curve
	[HarmonyPatch (typeof(TickManager), nameof(TickManager.DoSingleTick))]
	public class Patch_DoSingleTick
	{
		static int tick;

        static void Postfix()
        {
			if (++tick == 600) //about 10 seconds
			{
				tick = 0;
				foreach (var item in utilityCache.Values) item.Tick();
			}
        }
    }

	//Flush the cache on reload
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
	public class Patch_LoadGame
	{
        static void Prefix()
        {
			utilityCache.Clear();
        }
    }

	//Flush the cache on new games
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
	public class Patch_InitNewGame
	{
        static void Prefix()
        {
			utilityCache.Clear();
        }
    }
}