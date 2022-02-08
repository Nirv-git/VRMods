using UIExpansionKit.API;
using MelonLoader;
using VRC.UserCamera;
using UnityEngine;
using System.Collections;

[assembly: MelonInfo(typeof(CameraResChanger.CamResChangerMod), "CameraResChanger", "1.40", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CameraResChanger
{
    public class CamResChangerMod : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<bool> LargerSizes;
        public static MelonPreferences_Entry<bool> AdjustSizeAtLaunch;
        public static MelonPreferences_Entry<string> DefaultSize;

        private Transform _16k, _14k, _12k;
        private bool _enableLarger;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("CameraResChanger");

            cat = MelonPreferences.CreateCategory("CamResChange", "Camera Res Changer");
            LargerSizes = MelonPreferences.CreateEntry("CamResChange", nameof(LargerSizes), true, "Enable Sizes > 8k (Needs 'Lag Free Screenshots') - This may break/work with new VRC versions, test when the game updates");
            AdjustSizeAtLaunch = MelonPreferences.CreateEntry("CamResChange", nameof(AdjustSizeAtLaunch), false, "Adjust Camera res at game launch");
            DefaultSize = MelonPreferences.CreateEntry("CamResChange", nameof(DefaultSize), "4k", "Camera res for Adjust at Launch");
            ExpansionKitApi.RegisterSettingAsStringEnum("CamResChange",
                nameof(DefaultSize),
                new[]
                {
                    ("2k","Default"),
                    ("4k","4k"),
                    ("6k","6k"),
                    ("8k","8k")
                });

            OnPreferencesSaved();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("16k Res\n-Experimental-", () => ChangeCamRes(8640, 15360), (button) => { _16k = button.transform; button.gameObject.SetActive(_enableLarger); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("14k Res\n-Experimental-", () => ChangeCamRes(7560, 13440), (button) => { _14k = button.transform; button.gameObject.SetActive(_enableLarger); }); //use *.875 of 16k
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("12k Res\n-Experimental-", () => ChangeCamRes(6480, 11520), (button) => { _12k = button.transform; button.gameObject.SetActive(_enableLarger); }); //use *.75 of 16k
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("8k Res", () => ChangeCamRes(4320, 7680));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("6k Res", () => ChangeCamRes(3240, 5760));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("4k Res", () => ChangeCamRes(2160, 3840));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Default Res", () => ChangeCamRes(1080, 1920));

            if (AdjustSizeAtLaunch.Value) MelonCoroutines.Start(WaitForQM());
            Logger.Msg("Camera Res Changer Init");
        }

        public override void OnPreferencesSaved()
        {
            if (MelonHandler.Mods.Find(m => m.Info.Name == "Lag Free Screenshots") != null && LargerSizes.Value) //Broke as of 1046-outputs black image //Fixed since 1102
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
            Logger.Msg("Set Camera Res to: " + H + "x" + W);
        }

        public static IEnumerator WaitForQM()
        {
            while (GameObject.Find("/UserInterface/Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null) //Why wait for the MicButton, because I use this in other mods so I only need to fix one thing if it breaks in the future! Also you can't open the camera without going through the QM
                yield return new WaitForSeconds(1f);
            switch (DefaultSize.Value)
            {
                case "2k": ChangeCamRes(1080, 1920); break;
                case "4k": ChangeCamRes(2160, 3840); break;
                case "6k": ChangeCamRes(3240, 5760); break;
                case "8k": ChangeCamRes(4320, 7680); break;
                default : Logger.Error("DefaultSize Switch Error - Setting to 1080p"); ChangeCamRes(1080, 1920); break;
            }           
        }
    }
}
