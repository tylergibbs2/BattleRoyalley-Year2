﻿using BattleRoyale.Utils;
using StardewValley;
using System.Collections.Generic;

namespace BattleRoyale.Network
{
    class OnJoin : NetworkMessage
    {
        public OnJoin()
        {
            MessageType = NetworkUtils.MessageTypes.ON_JOIN;
        }

        public override void Receive(Farmer source, List<object> data)
        {
            for (int i = 0; i < Game1.player.items.Count; i++)
            {
                if (!(Game1.player.items[i] is StardewValley.Objects.Hat))
                    Game1.player.items[i] = null;
            }

            DelayedAction.functionAfterDelay(() =>
            {
                if (!ModEntry.BRGame.InProgress)
                    return;

                var oldLocation = Game1.player.currentLocation;
                var oldPosition = new xTile.Dimensions.Location(
                    (int)Game1.player.Position.X - Game1.viewport.Width / 2,
                    (int)Game1.player.Position.Y - Game1.viewport.Height / 2
                );

                SpectatorMode.EnterSpectatorMode(oldLocation, oldPosition);
            }, 1000);
        }
    }
}
