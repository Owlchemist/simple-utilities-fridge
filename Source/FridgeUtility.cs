using Verse;
using RimWorld;
using System.Collections.Generic;
 
namespace SimpleFridge
{
    //This essentially acts as a wannabe MapComponent, but since we don't have data to save or need ticking, this custom class is a slightly more lightweight approach
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
			else Log.Warning("[Simple Utilities: Fridge] Tried to register a map that already exists: " + map.uniqueID.ToString());
			fridgeGrid = new bool[map.info.NumCells];
		}

		public bool GetAdjustedTemperature(IntVec3 c)
		{
			int index = c.z * map.info.sizeInt.x + c.x;
			return (index > -1 && index < fridgeGrid.Length && fridgeGrid[index]);
		}

		public void UpdateFridgeGrid(CompPowerTrader thing)
		{
			foreach (var cell in GenAdj.OccupiedRect(thing.parent.positionInt, thing.parent.rotationInt, thing.parent.def.size))
			{
				fridgeGrid[cell.z * map.info.sizeInt.x + cell.x] = thing.powerOnInt;
			}
		}

		public void Tick()
		{
			for (int i = fridgeCache.Count; i-- > 0;)
			{
				var fridge = fridgeCache[i];

				//Validate that this fridge is still legit
				if (map != fridge.parent.Map)
				{
					fridgeCache.Remove(fridge);
					Log.Warning("[Simple Utilities: Fridge] Fridge had invalid map assigned.");
					continue;
				}

				//Update power consumption
				fridge.powerOutputInt = fridge.Props.basePowerConsumption * powerCurve.Evaluate(fridge.parent.GetRoom().Temperature);

				//While we're at it, update the grid the current power status
				UpdateFridgeGrid(fridge);
			}
		}
	}
}