using StardewValley.TerrainFeatures;

namespace BattleRoyale.Patches
{
    class KeepGrassInLobby : Patch
    {
        protected override PatchDescriptor GetPatchDescriptor() => new PatchDescriptor(typeof(Grass), "seasonUpdate");

        public static bool Prefix(Grass __instance)
        {
            if (__instance.currentLocation.Name != "Mountain")
                return true;

            return __instance.currentTileLocation.X < 95;
        }
    }
}
