using Verse;
using RimWorld;
using System.Collections.Generic;
 
namespace SimpleFridge
{
    public class FridgeUtility
	{
		public static Dictionary<int, FridgeUtility> utilityCache = new Dictionary<int, FridgeUtility>(); //Int is the map ID
		public List<CompPowerTrader> fridgeCache = new List<CompPowerTrader>();
		public bool[] fridgeGrid;
		Map map;
		//If temperature is freezing or lower, 10% power. If 15C (60F) or higher, 100%
		static SimpleCurve powerCurve = new SimpleCurve
		{
			{ new CurvePoint(0, -0.1f), true },
			{ new CurvePoint(15, -1f), true }
		};
		
		public FridgeUtility(Map map)
		{
			this.map = map;
			if(!utilityCache.ContainsKey(map.uniqueID)) utilityCache.Add(map.uniqueID, this);
			else Log.Warning("[Simple Utilities: Fridge] Tried to register a map that already exists.");
			fridgeGrid = new bool[map.info.NumCells];
		}

		public bool GetAdjustedTemperature(IntVec3 c)
		{
			int index = c.z * map.info.sizeInt.x + c.x;
			return (index > -1 && index < fridgeGrid.Length && fridgeGrid[index]);
		}

		public void UpdateFridgeGrid(CompPowerTrader thing, Map map)
		{
			if (map?.info != null)
			{
				CellRect cells = GenAdj.OccupiedRect(thing.parent.positionInt, thing.parent.rotationInt, thing.parent.def.size);
				foreach (var cell in cells)
				{
					fridgeGrid[cell.z * map.info.sizeInt.x + cell.x] = thing.powerOnInt;
				}
			}
		}

		public void Tick()
		{
			foreach (var fridge in fridgeCache)
			{
				//Validate that this fridge is still legit
				Map map = fridge.parent.Map;
				if (map == null)
				{
					fridgeCache.Remove(fridge);
					Log.Warning("[Simple Utilities: Fridge] Fridge had no map assigned.");
					continue;
				}

				//Update power consumption
				fridge.powerOutputInt = fridge.Props.basePowerConsumption * powerCurve.Evaluate(fridge.parent.GetRoom().Temperature);

				//While we're at it, update the grid the current power status
				UpdateFridgeGrid(fridge, map);
			}
		}
	}
}