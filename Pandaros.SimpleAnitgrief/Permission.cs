using BlockEntities.Implementations;
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
    }
}
