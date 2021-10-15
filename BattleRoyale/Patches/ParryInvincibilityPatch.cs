using StardewValley.Monsters;

namespace BattleRoyale.Patches
{
    class ParryInvincibilityPatch : Patch
    {
        protected override PatchDescriptor GetPatchDescriptor() => new PatchDescriptor(typeof(Monster), "handleParried");

        public static bool Prefix()
        {
            return false;
        }
    }
}
