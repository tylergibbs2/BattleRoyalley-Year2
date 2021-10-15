﻿using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace BattleRoyale.Patches
{
    class DesertBusListener : Patch
    {
        protected override PatchDescriptor GetPatchDescriptor() => new PatchDescriptor(typeof(Desert), "playerReachedBusDoor");

        public static bool Prefix(Desert __instance)
        {
            Game1.activeClickableMenu = new DialogueBox("Head to the right of the road");
            return false;
        }
    }
}
