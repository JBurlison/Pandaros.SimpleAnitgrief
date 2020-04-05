using BlockEntities.Implementations;
using NetworkUI;
using NetworkUI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.SimpleAnitgrief
{
    [ModLoader.ModManager]
    public class Permission
    {
        static List<Players.Player> _warnedPlayers = new List<Players.Player>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, "Pandaros.SimpleAntigrief.Permission.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (ServerManager.BlockEntityTracker.BannerTracker.TryGetClosest(userData.Position, out BannerTracker.Banner existingBanner, ServerManager.ServerSettings.Colony.ExclusiveRadius))
            {
                if (userData.RequestOrigin.Type == BlockChangeRequestOrigin.EType.Player &&
                    userData.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Steam &&
                    !PermissionsManager.HasPermission(userData.RequestOrigin.AsPlayer, new PermissionsManager.Permission("god")) &&
                    !existingBanner.Colony.Owners.Contains(userData.RequestOrigin.AsPlayer))
                {
                    Chatting.Chat.Send(userData.RequestOrigin.AsPlayer, "You may not build near this colony. Maybe you can ask one of the owners for an invite! The owners are: " + string.Join(", ", existingBanner.Colony.Owners.Select(o => o.Name)));
                    userData.InventoryItemResults.Clear();
                    userData.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
                    userData.CallbackConsumedResult = EServerChangeBlockResult.CancelledByCallback;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit,  "Pandaros.SimpleAntigrief.ColonyManager.Permission.OnHit")]
        public static void OnHit(NPC.NPCBase npc, ModLoader.OnHitData data)
        {
            if ((data.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
                 data.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile) && !npc.Colony.Owners.Contains((Players.Player)data.HitSourceObject))
            {
                var p = (Players.Player)data.HitSourceObject;

                if (_warnedPlayers.Contains(p))
                {
                    ServerManager.Disconnect(p);
                }
                else
                {
                    NetworkMenu menu = new NetworkMenu();
                    menu.LocalStorage.SetAs("header", "WARNING");
                    menu.Width = 800;
                    menu.Height = 600;
                    menu.ForceClosePopups = true;
                    menu.Items.Add(new Label(new LabelData("WARNING: Killing colonists that do not belong to you will result in a kick. This is your first and ONLY warning.", UnityEngine.Color.black)));
                    NetworkMenuManager.SendServerPopup(p, menu);
                }

                data.HitDamage = 0;
                data.ResultDamage = 0;
            }
        }
    }
}
