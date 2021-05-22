using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;



[assembly: MelonInfo(typeof(LocalCamera.Main), "TeleportCameraToYou", "1.0.1", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace LocalCamera
{
    public class Main : MelonMod
    {

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("TeleportCamToYou", "Teleport Camera To You");
            MelonPreferences.CreateEntry("TeleportCamToYou", "Distance", 0.1f, "Distance from you");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Teleport Camera To You", () =>
            {
                GameObject vrcam = GameObject.Find("TrackingVolume/PlayerObjects/UserCamera/ViewFinder");
                if (vrcam is null) return;
                VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
                Vector3 pos = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/HeadEffector").transform.position + (player.transform.forward * MelonPreferences.GetEntryValue<float>("TeleportCamToYou", "Distance"));
                vrcam.transform.position = pos;
                
            });
        }

    }
}

