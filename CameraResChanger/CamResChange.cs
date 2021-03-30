using UIExpansionKit.API;
using MelonLoader;
using VRC.UserCamera;

[assembly: MelonInfo(typeof(CameraResChanger.CamResChangerMod), "CameraResChanger", "1.34", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CameraResChanger
{

    public class CamResChangerMod : MelonMod
    {

        public override void OnApplicationStart()
        {
            MelonPrefs.RegisterCategory("CamResChange", "Camera Res Changer");
            //MelonPrefs.RegisterBool("CamResChange", "LargerSizes", true, "Enabled sizes larger than 8k (Requires 'Lag Free Screenshots' and may break in some VRC versions. Requires Restart)");
            //if (MelonHandler.Mods.Find(m => m.Info.Name == "Lag Free Screenshots") != null && ModPrefs.GetBool("CamResChange", "LargerSizes")) //Broke as od 1046-outputs black image
            //{
            //    ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "16k Res", () => ChangeCamRes(8640, 15360));
            //    ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "14k Res", () => ChangeCamRes(7560, 13440)); //use *.875 of 16k
            //    ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "12k Res", () => ChangeCamRes(6480, 11520)); //use *.75 of 16k
            //}
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "8k Res", () => ChangeCamRes(4320, 7680));
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "6k Res", () => ChangeCamRes(3240, 5760));
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "4k Res", () => ChangeCamRes(2160, 3840));
            //ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "2k Res", () => ChangeCamRes(1440, 2560));//Really 1080p is often considered 2k
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.CameraQuickMenu, "Default Res", () => ChangeCamRes(1080, 1920));
            MelonLogger.Log("Camera Res Changer Init");
        }

        private static void ChangeCamRes(int H, int W)
        {
            var cameraController = UserCameraController.field_Internal_Static_UserCameraController_0;
            if (cameraController == null) return;
            cameraController.field_Public_Int32_3 = H;
            cameraController.field_Public_Int32_2 = W;
            //           cameraController.photoHeight = H;
            //           cameraController.photoWidth = W;
            MelonLogger.Log("Set Camera Res to: " + H + "x" + W);
        }

        
    }
}
