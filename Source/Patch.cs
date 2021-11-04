using HarmonyLib;
using Verse;
using RimWorld;
using System.Linq;
using static SimpleFridge.Mod_SimpleFridge;

namespace SimpleFridge
{
	//Change the perceived temperature
	[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.GetTemperatureForCell))]
	public class Patch_GetTemperatureForCell
	{
		static public bool Prefix(Map map, IntVec3 c, ref float __result)
		{
			if (map?.info != null && fridgeGrid.TryGetValue(map).ElementAtOrDefault(c.z * map.info.sizeInt.x + c.x))
			{
				__result = -10f;
				return false;
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
			if (__instance.parent.def.HasModExtension<Fridge>())
			{
				fridgeCache.Add(__instance.parent, __instance);
				FridgeUtility.UpdateFridgeGrid(__instance);
			}
		}
    }

	//This handles cache deregistration
	[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DeSpawn))]
	public class Patch_PostDeSpawn
	{
		static public void Prefix(ThingWithComps __instance)
		{
			CompPowerTrader comp;
			if (fridgeCache.TryGetValue(__instance, out comp))
			{
				comp.powerOnInt = false;
				FridgeUtility.UpdateFridgeGrid(comp);
				fridgeCache.Remove(__instance);
			}
		}
    }

	//This handles fridge grid cache construction
	[HarmonyPatch(typeof(Map), nameof(Map.ConstructComponents))]
	public class Patch_ConstructComponents
	{
		static public void Prefix(Map __instance)
		{
			if (!fridgeGrid.ContainsKey(__instance)) fridgeGrid.Add(__instance, new bool[__instance.info.NumCells]);
		}
    }


	//Every 600 ticks, check fridge room temperature and adjust its power curve
	[HarmonyPatch (typeof(GameComponentUtility), nameof(GameComponentUtility.GameComponentTick))]
	public class Patch_GameComponentTick
	{
		static int tick;
		//If temperature is freezing or lower, 10% power. If 15C (60F) or higher, 100%
		static SimpleCurve powerCurve = new SimpleCurve
		{
			{ new CurvePoint(0, -0.1f), true },
			{ new CurvePoint(15, -1f), true }
		};

        static void Postfix()
        {
			if (++tick == 600) //about 10 seconds
			{
				tick = 0;
				foreach (var fridge in fridgeCache)
				{
					//Update power consumption
					fridge.Value.powerOutputInt = fridge.Value.Props.basePowerConsumption * powerCurve.Evaluate(fridge.Key.GetRoom().Temperature);

					//While we're at it, update the grid the current power status
					FridgeUtility.UpdateFridgeGrid(fridge.Value);
				}
			}
        }
    }

	//Flush the cache on reload
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
	public class Patch_LoadGame
	{
        static void Prefix()
        {
            fridgeCache.Clear();
			fridgeGrid.Clear();
        }
    }
}