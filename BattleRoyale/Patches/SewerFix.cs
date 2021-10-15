using StardewValley.Locations;

namespace BattleRoyale.Patches
{
    class SewerFix : Patch
    {
        protected override PatchDescriptor GetPatchDescriptor() => new PatchDescriptor(typeof(Sewer), "MakeMapModifications");

        public static bool Prefix(Sewer __instance, bool force = false)
        {
            __instance.setMapTileIndex(31, 17, -1, "Buildings");
            __instance.setMapTileIndex(31, 16, -1, "Front");
            return false;
        }
    }
}
