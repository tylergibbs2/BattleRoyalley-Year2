using Microsoft.Xna.Framework;
using StardewValley;

namespace BattleRoyale.Patches
{
    class SlingshotPatch2 : Patch
    {
        protected override PatchDescriptor GetPatchDescriptor() => new(typeof(GameLocation), "UpdateWhenCurrentLocation");

        public static void Postfix(GameLocation __instance, GameTime time)
        {
            int i = 0;
            while (i < __instance.projectiles.Count)
            {
                if (__instance.projectiles[i].update(time, __instance))
                    __instance.projectiles.RemoveAt(i);
                else
                    i++;
            }
        }
    }
}
