using UIExpansionKit.API;
using MelonLoader;
using VRC.UserCamera;
using UnityEngine;

[assembly: MelonInfo(typeof(CameraResChanger.CamResChangerMod), "CameraResChanger", "1.37", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CameraResChanger
{

    public class CamResChangerMod : MelonMod
    {
        private Transform _16k, _14k, _12k;
        private bool _enableLarger;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("CamResChange", "Camera Res Changer");
            MelonPreferences.CreateEntry<bool>("CamResChange", "LargerSizes", false, "Enable Sizes > 8k (Needs 'Lag Free Screenshots') - This may break/work with new VRC versions, test when the game updates");

            OnPreferencesSaved();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("16k Res\n-Experimental-", () => ChangeCamRes(8640, 15360), (button) => { _16k = button.transform; button.gameObject.SetActive(_enableLarger); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("14k Res\n-Experimental-", () => ChangeCamRes(7560, 13440), (button) => { _14k = button.transform; button.gameObject.SetActive(_enableLarger); }); //use *.875 of 16k
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("12k Res\n-Experimental-", () => ChangeCamRes(6480, 11520), (button) => { _12k = button.transform; button.gameObject.SetActive(_enableLarger); }); //use *.75 of 16k

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("8k Res", () => ChangeCamRes(4320, 7680));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("6k Res", () => ChangeCamRes(3240, 5760));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("4k Res", () => ChangeCamRes(2160, 3840));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Default Res", () => ChangeCamRes(1080, 1920));
            MelonLogger.Msg("Camera Res Changer Init");
        }
        public override void OnPreferencesSaved()
        {
            if (MelonHandler.Mods.Find(m => m.Info.Name == "Lag Free Screenshots") != null && MelonPreferences.GetEntryValue<bool>("CamResChange", "LargerSizes")) //Broke as of 1046-outputs black image //Fixed since 1102
                _enableLarger = true;
            else
                _enableLarger = false;

            if (_16k != null) _16k.gameObject.SetActive(_enableLarger);
            if (_14k != null) _14k.gameObject.SetActive(_enableLarger);
            if (_12k != null) _12k.gameObject.SetActive(_enableLarger);
        }

        private static void ChangeCamRes(int H, int W)
        {
            var cameraController = UserCameraController.field_Internal_Static_UserCameraController_0;
            if (cameraController == null) return;
            cameraController.field_Public_Int32_3 = H;
            cameraController.field_Public_Int32_2 = W;
            //           cameraController.photoHeight = H;
            //           cameraController.photoWidth = W;
            MelonLogger.Msg("Set Camera Res to: " + H + "x" + W);
        }

        
    }
}
