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
			if (map != null)
			{
				CellRect cells = thing.parent.OccupiedRect();
				foreach (var cell in cells)
				{
					fridgeGrid[map][cell.z * map.info.sizeInt.x + cell.x] = thing.PowerOn;
				}
			}
		}
	}
}
