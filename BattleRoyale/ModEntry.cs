using BattleRoyale.Patches;
using BattleRoyale.UI;
using BattleRoyale.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BattleRoyale
{
    class ModEntry : Mod, IAssetLoader, IAssetEditor
    {
        public static Game BRGame { get; private set; }
        public static ModConfig Config { get; private set; }
        public static IModEvents Events { get; private set; }

        public static Leaderboard Leaderboard { get; private set; }

        public static IMultiplayerHelper Multiplayer { get; private set; }

        private readonly Dictionary<int, string> customTooltips = new()
        {
            { 529, "Increases knockback by 50%." }  // Amethyst Ring,
        };

        private readonly Dictionary<string, string> modifiedAssets = new()
        {
            { "Minigames/TitleButtons", "Assets/title.png" },
            { "TileSheets/furniture", "Assets/furniture.png" },
            { "Maps/springobjects", "Assets/springobjects.png" },
            { "Characters/Farmer/hats", "Assets/hats.png" },
            { "Maps/Desert", "Assets/Maps/Desert.tmx" },
            { "Maps/BusStop", "Assets/Maps/BusStop.tmx" },
            { "Maps/Mine", "Assets/Maps/Mine.tmx" },
            { "Maps/Town", "Assets/Maps/Town.tmx" },
            { "Maps/Mountain", "Assets/Maps/Mountain.tmx" }
        };

        public static string SaveFile = "GameHost_204797717";

        public static bool DisplayNames = true;

        public ModEntry()
        {
            Harmony h = new("ilyaki.battleroyale_ctor");

            {
                List<MethodBase> toPatch = new();
                toPatch.AddRange(h.GetPatchedMethods().Where(x => x.DeclaringType.Assembly != Assembly.GetAssembly(typeof(Game1))));

                foreach (MethodBase method in toPatch)
                {
                    Console.WriteLine($"Removing patch {method.DeclaringType.Name}.{method.Name}");
                    var patches = Harmony.GetPatchInfo(method);

                    h.Unpatch(method, HarmonyPatchType.All);
                }
            }

            foreach (var m2 in typeof(PatchProcessor).GetMethods().Where(x => x.Name == "Unpatch"))
                h.Patch(m2, new HarmonyMethod(GetType().GetMethod(nameof(P_F))));
        }

        public static bool P_F()
        {
            return false;
        }

        public static bool IsOnlyMod()
        {
            foreach (Assembly assm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assm == Assembly.GetExecutingAssembly())
                    continue;

                Type[] assmTypes = Array.Empty<Type>();
                try
                {
                    assmTypes = assm.GetTypes();
                }
                catch (ReflectionTypeLoadException) { }

                foreach (Type type in assmTypes)
                {
                    if (type.IsSubclassOf(typeof(Mod)))
                        return false;
                }
            }

            return true;
        }

        public override void Entry(IModHelper helper)
        {
            if (!IsOnlyMod())
            {
                Game1.quit = true;

                //Console.Clear();
                Monitor.Log("Please uninstall all other mods to play Battle Royalley.", LogLevel.Warn);
                Monitor.Log("Press any key to continue...", LogLevel.Warn);
                Console.ReadKey();
                return;
            }

            Multiplayer = helper.Multiplayer;
            Events = helper.Events;

            var config = helper.ReadConfig<ModConfig>();
            if (config == null)
            {
                config = new();
                helper.WriteConfig(config);
            }

            BRGame = new(helper, Monitor);
            Leaderboard = new();
            Config = config;

            Patch.PatchAll("ilyaki.battleroyale");
            NetworkMessage.SetupMessages();

            try
            {
                //Remove player limit
                var multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                multiplayer.playerLimit = config.PlayerLimit;//250 is the Galaxy limit (it's multiplied by two for some reason, so set 125)
            }
            catch (Exception)
            {
                Monitor.Log("Error setting player limit. The max is 125", LogLevel.Error);
            }


            helper.ConsoleCommands.Add("br_start", "Start the game. Alternative to pressing Right control", (c, a) =>
            {
                if (Game1.IsServer)
                    BRGame.Play();
            });

            helper.ConsoleCommands.Add("br_spectate", "Toggle spectating", (c, a) =>
            {
                NetworkMessage.Send(
                    NetworkUtils.MessageTypes.TOGGLE_SPECTATE,
                    NetworkMessageDestination.ALL,
                    new() { Game1.player.UniqueMultiplayerID, !BRGame.isSpectating }
                );
            });

            Events.Input.ButtonPressed += (o, e) =>
            {
                e.Button.TryGetKeyboard(out Keys pressedKey);
                switch (pressedKey)
                {
                    case Keys.RightControl:
                        if (Game1.IsServer && e.Button.TryGetKeyboard(out Keys key) && key == Keys.RightControl)
                            BRGame.Play();
                        break;
                    case Keys.RightAlt:
                        var loc = Game1.player.currentLocation;
                        int x = Game1.player.getTileX();
                        int y = Game1.player.getTileY();


                        Monitor.Log($"tile location={loc.Name}, pos=({x},{y})", LogLevel.Error);

                        Monitor.Log($"my id : {Game1.player.UniqueMultiplayerID}", LogLevel.Error);

                        Monitor.Log($"precise position = {Game1.player.Position}", LogLevel.Error);
                        break;
                    case Keys.F1:
                        DisplayNames = !DisplayNames;
                        break;
                    case Keys.Tab:
                        Leaderboard.TryShow();
                        break;
                    case Keys.None:
                        break;
                    case Keys.Back:
                        break;
                    case Keys.Enter:
                        break;
                    case Keys.CapsLock:
                        break;
                    case Keys.Escape:
                        break;
                    case Keys.Space:
                        break;
                    case Keys.PageUp:
                        break;
                    case Keys.PageDown:
                        break;
                    case Keys.End:
                        break;
                    case Keys.Home:
                        break;
                    case Keys.Left:
                        break;
                    case Keys.Up:
                        break;
                    case Keys.Right:
                        break;
                    case Keys.Down:
                        break;
                    case Keys.Select:
                        break;
                    case Keys.Print:
                        break;
                    case Keys.Execute:
                        break;
                    case Keys.PrintScreen:
                        break;
                    case Keys.Insert:
                        break;
                    case Keys.Delete:
                        break;
                    case Keys.Help:
                        break;
                    case Keys.D0:
                        break;
                    case Keys.D1:
                        break;
                    case Keys.D2:
                        break;
                    case Keys.D3:
                        break;
                    case Keys.D4:
                        break;
                    case Keys.D5:
                        break;
                    case Keys.D6:
                        break;
                    case Keys.D7:
                        break;
                    case Keys.D8:
                        break;
                    case Keys.D9:
                        break;
                    case Keys.A:
                        break;
                    case Keys.B:
                        break;
                    case Keys.C:
                        break;
                    case Keys.D:
                        break;
                    case Keys.E:
                        break;
                    case Keys.F:
                        break;
                    case Keys.G:
                        break;
                    case Keys.H:
                        break;
                    case Keys.I:
                        break;
                    case Keys.J:
                        break;
                    case Keys.K:
                        break;
                    case Keys.L:
                        break;
                    case Keys.M:
                        break;
                    case Keys.N:
                        break;
                    case Keys.O:
                        break;
                    case Keys.P:
                        break;
                    case Keys.Q:
                        break;
                    case Keys.R:
                        break;
                    case Keys.S:
                        break;
                    case Keys.T:
                        break;
                    case Keys.U:
                        break;
                    case Keys.V:
                        break;
                    case Keys.W:
                        break;
                    case Keys.X:
                        break;
                    case Keys.Y:
                        break;
                    case Keys.Z:
                        break;
                    case Keys.LeftWindows:
                        break;
                    case Keys.RightWindows:
                        break;
                    case Keys.Apps:
                        break;
                    case Keys.Sleep:
                        break;
                    case Keys.NumPad0:
                        break;
                    case Keys.NumPad1:
                        break;
                    case Keys.NumPad2:
                        break;
                    case Keys.NumPad3:
                        break;
                    case Keys.NumPad4:
                        break;
                    case Keys.NumPad5:
                        break;
                    case Keys.NumPad6:
                        break;
                    case Keys.NumPad7:
                        break;
                    case Keys.NumPad8:
                        break;
                    case Keys.NumPad9:
                        break;
                    case Keys.Multiply:
                        break;
                    case Keys.Add:
                        break;
                    case Keys.Separator:
                        break;
                    case Keys.Subtract:
                        break;
                    case Keys.Decimal:
                        break;
                    case Keys.Divide:
                        break;
                    case Keys.F2:
                        break;
                    case Keys.F3:
                        break;
                    case Keys.F4:
                        break;
                    case Keys.F5:
                        break;
                    case Keys.F6:
                        break;
                    case Keys.F7:
                        break;
                    case Keys.F8:
                        break;
                    case Keys.F9:
                        break;
                    case Keys.F10:
                        break;
                    case Keys.F11:
                        break;
                    case Keys.F12:
                        break;
                    case Keys.F13:
                        break;
                    case Keys.F14:
                        break;
                    case Keys.F15:
                        break;
                    case Keys.F16:
                        break;
                    case Keys.F17:
                        break;
                    case Keys.F18:
                        break;
                    case Keys.F19:
                        break;
                    case Keys.F20:
                        break;
                    case Keys.F21:
                        break;
                    case Keys.F22:
                        break;
                    case Keys.F23:
                        break;
                    case Keys.F24:
                        break;
                    case Keys.NumLock:
                        break;
                    case Keys.Scroll:
                        break;
                    case Keys.LeftShift:
                        break;
                    case Keys.RightShift:
                        break;
                    case Keys.LeftControl:
                        break;
                    case Keys.LeftAlt:
                        break;
                    case Keys.BrowserBack:
                        break;
                    case Keys.BrowserForward:
                        break;
                    case Keys.BrowserRefresh:
                        break;
                    case Keys.BrowserStop:
                        break;
                    case Keys.BrowserSearch:
                        break;
                    case Keys.BrowserFavorites:
                        break;
                    case Keys.BrowserHome:
                        break;
                    case Keys.VolumeMute:
                        break;
                    case Keys.VolumeDown:
                        break;
                    case Keys.VolumeUp:
                        break;
                    case Keys.MediaNextTrack:
                        break;
                    case Keys.MediaPreviousTrack:
                        break;
                    case Keys.MediaStop:
                        break;
                    case Keys.MediaPlayPause:
                        break;
                    case Keys.LaunchMail:
                        break;
                    case Keys.SelectMedia:
                        break;
                    case Keys.LaunchApplication1:
                        break;
                    case Keys.LaunchApplication2:
                        break;
                    case Keys.OemSemicolon:
                        break;
                    case Keys.OemPlus:
                        break;
                    case Keys.OemComma:
                        break;
                    case Keys.OemMinus:
                        break;
                    case Keys.OemPeriod:
                        break;
                    case Keys.OemQuestion:
                        break;
                    case Keys.OemTilde:
                        break;
                    case Keys.OemOpenBrackets:
                        break;
                    case Keys.OemPipe:
                        break;
                    case Keys.OemCloseBrackets:
                        break;
                    case Keys.OemQuotes:
                        break;
                    case Keys.Oem8:
                        break;
                    case Keys.OemBackslash:
                        break;
                    case Keys.ProcessKey:
                        break;
                    case Keys.Attn:
                        break;
                    case Keys.Crsel:
                        break;
                    case Keys.Exsel:
                        break;
                    case Keys.EraseEof:
                        break;
                    case Keys.Play:
                        break;
                    case Keys.Zoom:
                        break;
                    case Keys.Pa1:
                        break;
                    case Keys.OemClear:
                        break;
                    case Keys.ChatPadGreen:
                        break;
                    case Keys.ChatPadOrange:
                        break;
                    case Keys.Pause:
                        break;
                    case Keys.ImeConvert:
                        break;
                    case Keys.ImeNoConvert:
                        break;
                    case Keys.Kana:
                        break;
                    case Keys.Kanji:
                        break;
                    case Keys.OemAuto:
                        break;
                    case Keys.OemCopy:
                        break;
                    case Keys.OemEnlW:
                        break;
                    default:
                        break;
                }

                if (e.Button is SButton.MouseLeft)
                {
                    if (Leaderboard.AlreadyDisplaying())
                        Leaderboard.receiveLeftClick((int)e.Cursor.GetScaledAbsolutePixels().X, (int)e.Cursor.GetScaledAbsolutePixels().Y);
                }
            };

            Events.Input.ButtonReleased += (o, e) =>
            {
                if (e.Button.TryGetKeyboard(out Keys key) && key == Keys.Tab)
                    Leaderboard.TryRemove();
            };

            Events.Multiplayer.ModMessageReceived += NetworkMessage.OnMessageReceive;

            Events.GameLoop.UpdateTicked += (o, e) => BRGame?.Update();
            Events.GameLoop.UpdateTicked += (o, e) => HitShaker.Update(Game1.currentGameTime);
            Events.GameLoop.UpdateTicked += (o, e) => SpectatorMode.Update();

            Events.Display.RenderingActiveMenu += (o, e) =>
            {
                if (Game1.activeClickableMenu is Billboard || Game1.activeClickableMenu is SpecialOrdersBoard)
                    Game1.activeClickableMenu = null;
            };

            Events.GameLoop.SaveLoaded += (o, e) =>
            {
                BRGame = new(helper, Monitor);

                if (Game1.player != Game1.MasterPlayer)
                    return;

                MapUtils.PrepareLobby();

                Leaderboard.InitPlayer(Game1.player);

                for (int i = 0; i < Game1.player.Items.Count; i++)
                    Game1.player.Items[i] = null;

                Game1.player.Name = "";
                Game1.player.favoriteThing.Value = "";

                NetworkUtils.WarpFarmer(Game1.player, new("Mountain", 117, 30));
                Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewFarmhand);
            };

            Events.Player.Warped += (o, e) =>
            {
                if (e.NewLocation != null && (e.NewLocation is Woods || e.NewLocation.Name == "BugLand"))
                    e.NewLocation.characters.Clear();
            };

            Events.Display.RenderingHud += (o, e) =>
            {
                if (!SpectatorMode.InSpectatorMode)
                    Storm.Draw(Game1.spriteBatch);
            };


            Events.Display.Rendered += (o, e) =>
            {
                foreach (IClickableMenu menu in Game1.onScreenMenus.ToList())
                {
                    if (menu is VictoryRoyale royale && royale.GetTimeSince() == null)
                        Game1.onScreenMenus.Remove(menu);
                    if (menu is SpectateToolbar && !SpectatorMode.InSpectatorMode)
                        Game1.onScreenMenus.Remove(menu);
                }

                if (SpectatorMode.InSpectatorMode)//Spectator mode can only see the storm when it is drawn above everything
                {
                    Storm.Draw(Game1.spriteBatch);

                    if (Game1.activeClickableMenu == null)
                    {
                        string message = "Spectating";
                        if (SpectatorMode.Following != null)
                            message += $" {SpectatorMode.Following.Name}";

                        SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, message, (Game1.uiViewport.Width / 2) - SpriteText.getWidthOfString(message) / 2, 16, "", 1f, -1);

                        if (Game1.displayHUD)
                            return;

                        foreach (IClickableMenu menu in Game1.onScreenMenus.ToList())
                        {
                            if (SpectatorMode.Following == null && menu is SpectateToolbar)
                                continue;

                            if (menu != Game1.chatBox && menu is not Toolbar)
                            {
                                menu.update(Game1.currentGameTime);
                                menu.draw(Game1.spriteBatch);
                            }
                        }
                    }
                }
            };

            helper.ConsoleCommands.Add("br_kick", "Kick a player. Usage: br_kick <player ID>", (a, b) =>
            {
                if (!Game1.IsServer)
                    Monitor.Log("Need to be the server host", LogLevel.Info);
                else if (b.Length != 1)
                    Monitor.Log("Need 1 argument", LogLevel.Info);
                else if (!long.TryParse(b[0], out long x))
                    Monitor.Log("Not a valid number", LogLevel.Info);
                else
                {
                    try
                    {
                        var f = Game1.getOnlineFarmers().First(p => p != Game1.player && p.UniqueMultiplayerID == x);
                        NetworkUtils.KickPlayer(f, "You have been kicked by the host.");
                    }
                    catch (Exception)
                    {
                        Monitor.Log($"Could not find player with id {x}", LogLevel.Info);
                    }
                }
            });

            helper.ConsoleCommands.Add("br_setNumberOfPlayerSlots", "Sets the number of player slots. Usage: br_setNumberOfPlayerSlots <number of slots>", (a, b) =>
            {
                if (!Game1.IsServer)
                    Monitor.Log("Need to be the server host", LogLevel.Info);
                else if (b.Length != 1)
                    Monitor.Log("Need 1 argument", LogLevel.Info);
                else if (!int.TryParse(b[0], out int n))
                    Monitor.Log("Not a valid number", LogLevel.Info);
                else
                {
                    n = Math.Abs(n);
                    var emptyCabins = Game1.getFarm().buildings.Where(z => z.daysOfConstructionLeft.Value <= 0 && z.indoors.Value is Cabin).ToArray();

                    if (n > emptyCabins.Length)
                    {
                        for (int i = 0; i < n - emptyCabins.Length; i++)
                        {
                            var blueprint = new BluePrint("Log Cabin");
                            var building = new Building(blueprint, new Vector2(-10000, 0));
                            Game1.getFarm().buildings.Add(building);

                            try
                            {
                                foreach (var warp in building.indoors.Value.warps)
                                {
                                    //warp.TargetName = "Forest";
                                    Helper.Reflection.GetField<NetString>(warp, "targetName", true).SetValue(new NetString("Forest"));
                                    warp.TargetX = 100;
                                    warp.TargetY = 20;
                                }
                            }
                            catch (Exception) { }
                        }

                        Monitor.Log($"Added {n - emptyCabins.Length} player slots", LogLevel.Info);
                    }
                    else if (n < emptyCabins.Length)
                    {
                        for (int i = 0; i < emptyCabins.Length - n; i++)
                        {
                            Game1.getFarm().buildings.Remove(emptyCabins[i]);
                        }

                        Monitor.Log($"Removed {emptyCabins.Length - n} player slots", LogLevel.Info);
                    }
                    else
                    {
                        Monitor.Log($"There are already {n} player slots", LogLevel.Info);
                    }
                }
            });
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            foreach (string name in modifiedAssets.Keys)
            {
                if (asset.AssetNameEquals(name))
                    return true;
            }
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            foreach (string name in modifiedAssets.Keys)
            {
                if (asset.AssetNameEquals(name))
                    return Helper.Content.Load<T>(modifiedAssets[name], ContentSource.ModFolder);
            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/ObjectInformation");
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/ObjectInformation"))
            {
                IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;

                foreach (var kvp in customTooltips)
                {
                    int itemId = kvp.Key;
                    string tooltip = kvp.Value;

                    string[] fields = data[itemId].Split('/');
                    fields[5] = tooltip;
                    data[itemId] = string.Join("/", fields);
                }
            }
        }
    }
}