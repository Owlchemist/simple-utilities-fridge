using Verse;
using HarmonyLib;
 
namespace SimpleFridge
{
	[StaticConstructorOnStartup]
    public class HarmonyPatches
	{
		static HarmonyPatches()
		{
			new Harmony("owlchemist.fridgeutilities").PatchAll();
		}
	}
	public class Fridge : DefModExtension {}
}