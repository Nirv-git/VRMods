using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using VRC.UserCamera;
using ConsoleColor = System.ConsoleColor;

[assembly: MelonInfo(typeof(NearClipPlaneAdj.NearClipPlaneAdjMod), "NearClipPlaneAdj", "1.35", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace NearClipPlaneAdj
{

    public class NearClipPlaneAdjMod : MelonMod
    {
        public bool keybindsEnabled;
        public bool smallerDefault;
        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("NearClipAdj", "NearClipPlane Adjuster");
            MelonPreferences.CreateEntry<bool>("NearClipAdj", "Keyboard", true, "Keyboard Shortcuts: '[' - 0.0001, ']' - 0.05");
            MelonPreferences.CreateEntry<bool>("NearClipAdj", "SmallerDefault", false, "Smaller Default Nearclip on World Change - 0.001 vs 0.01");
            OnPreferencesSaved();

            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.SettingsMenu, "Nearplane-0.05", (() => ChangeNearClipPlane(.05f)));
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.SettingsMenu, "Nearplane-0.01", (() => ChangeNearClipPlane(.01f)));
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.SettingsMenu, "Nearplane-0.001", (() => ChangeNearClipPlane(.001f)));
            ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.SettingsMenu, "Nearplane-0.0001", (() => ChangeNearClipPlane(.0001f)));
            MelonLogger.Msg("Near Plane Adjust Init");
        }

        public override void OnPreferencesSaved()
        {
            keybindsEnabled = MelonPreferences.GetEntryValue<bool>("NearClipAdj", "Keyboard");
            smallerDefault = MelonPreferences.GetEntryValue<bool>("NearClipAdj", "SmallerDefault");

        }

        private void ChangeNearClipPlane(float value)
        {
            VRCVrCamera vrCamera = VRCVrCamera.field_Private_Static_VRCVrCamera_0;
            if (!vrCamera)
                return;
            Camera screenCamera = vrCamera.field_Public_Camera_0;
            //Camera screenCamera = vrCamera.screenCamera;
            if (!screenCamera)
                return;
            float oldvalue = screenCamera.nearClipPlane;
            screenCamera.nearClipPlane = value;
            MelonLogger.Msg($"Near plane previous: {oldvalue}, Now: {value} {(keybindsEnabled ? "- Keyboard Hotkeys: '[' - 0.0001, ']' - 0.05" : "" )}");
            ChangePhotoCameraNearField(value);
        }

        private void ChangePhotoCameraNearField(float value)
        {
            var cameraController = UserCameraController.field_Internal_Static_UserCameraController_0;
            if (cameraController == null)
                return;
            Camera cam = cameraController.field_Public_GameObject_1.GetComponent<Camera>();
            //Camera cam = cameraController.photoCamera.GetComponent<Camera>();
            if (cam != null)
                cam.nearClipPlane = value;

        }

        public override void OnLevelWasLoaded(int level)
        {
            //Return the clipping distance to a safe, close value on load
            switch (level)//Without switch this would run 3 times at world load
            {
                case 0: //App
                case 1: //ui
                    break;
                default:
                    MelonCoroutines.Start(SetNearClipPlane(0.01f));
                    break;
            }
        }

        System.Collections.IEnumerator SetNearClipPlane(float znear)
        {
            yield return new WaitForSecondsRealtime(15); //Wait 15 seconds after world load before setting the clipping value. Waiting for the next/first frame does not work
            if (smallerDefault) znear = 0.001f;
            ChangeNearClipPlane(znear);
            MelonLogger.Msg("Near plane adjusted after world load");
        }


        public override void OnUpdate()
        {
            if (!keybindsEnabled) return;

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, "NearClip set to smallest value on keypress: 0.0001");
                ChangeNearClipPlane(.0001f);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, "NearClip set to largest value on keypress: 0.05");
                ChangeNearClipPlane(.05f);
            }
        }


    }
}
