using Verse;
using RimWorld;
using static SimpleFridge.Mod_SimpleFridge;
 
namespace SimpleFridge
{
    public static class FridgeUtility
	{
		public static void UpdateFridgeGrid(CompPowerTrader thing)
		{
			Map map = thing?.parent.Map;
			if (map?.info != null)
			{
				CellRect cells = GenAdj.OccupiedRect(thing.parent.positionInt, thing.parent.rotationInt, thing.parent.def.size);
				foreach (var cell in cells)
				{
					fridgeGrid.TryGetValue(map.uniqueID)[cell.z * map.info.sizeInt.x + cell.x] = thing.powerOnInt;
				}
			}
		}
	}
}
