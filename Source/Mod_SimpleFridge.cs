using Verse;
using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
 
namespace SimpleFridge
{
    public class Mod_SimpleFridge : Mod
	{
		public static Dictionary<ThingWithComps, CompPowerTrader> fridgeCache = new Dictionary<ThingWithComps, CompPowerTrader>();
		public static Dictionary<int, bool[]> fridgeGrid = new Dictionary<int, bool[]>();
		public static bool singleMapMode = false;
		public static bool[] activeFridgeGrid;
		public Mod_SimpleFridge(ModContentPack content) : base(content)
		{
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
		}
	}
}