using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using VRC.UserCamera;
using ConsoleColor = System.ConsoleColor;
using UnhollowerRuntimeLib;
using NearClipPlaneAdj.Components;
using System.Collections;

[assembly: MelonInfo(typeof(NearClipPlaneAdj.NearClipPlaneAdjMod), "NearClipPlaneAdj", "1.43", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace NearClipPlaneAdj
{

    public class NearClipPlaneAdjMod : MelonMod
    {
        public bool keybindsEnabled;
        public bool smallerDefault;
        public float oldNearClip;
        public bool raiseNearClip;
        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("NearClipAdj", "NearClipPlane Adjuster");
            MelonPreferences.CreateEntry<bool>("NearClipAdj", "Keyboard", true, "Keyboard Shortcuts: '[' - 0.0001, ']' - 0.05");
            MelonPreferences.CreateEntry<bool>("NearClipAdj", "SmallerDefault", false, "Smaller Default Nearclip on World Change - 0.001 vs 0.01");
            MelonPreferences.CreateEntry<bool>("NearClipAdj", "RaiseOnQuickMenu", false, "If using smaller Default Nearclip (0.001) raise to 0.01 when Quick Menu opens.");
            OnPreferencesSaved();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.05", (() => ChangeNearClipPlane(.05f, true)));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.01", (() => ChangeNearClipPlane(.01f, true)));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.001", (() => ChangeNearClipPlane(.001f, true)));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.0001", (() => ChangeNearClipPlane(.0001f, true)));
            MelonLogger.Msg("Near Plane Adjust Init");

            MelonLogger.Msg("Registering components...");
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonCoroutines.Start(OnLoad());
        }

        public override void OnPreferencesSaved()
        {
            keybindsEnabled = MelonPreferences.GetEntryValue<bool>("NearClipAdj", "Keyboard");
            smallerDefault = MelonPreferences.GetEntryValue<bool>("NearClipAdj", "SmallerDefault");
            raiseNearClip = MelonPreferences.GetEntryValue<bool>("NearClipAdj", "SmallerDefault");
        }

        public IEnumerator OnLoad()
        {
            MelonLogger.Msg("Adding QM listener...."); //From https://github.com/tetra-fox/QMFreeze/blob/master/QMFreeze/Mod.cs
            //string micPath = "/UserInterface/Canvas_QuickMenu(Clone)/Container/Window/MicButton";
            //while (GameObject.Find(micPath) == null)
            while (GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null)
                yield return new WaitForSeconds(1f);
            // MicControls is enabled no matter the QM page that's open, so let's use that to determine whether or not the QM is open
            // Unless you have some other mod that removes this button then idk lol
            EnableDisableListener listener = GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/MicButton").gameObject
                .AddComponent<EnableDisableListener>();
            listener.OnEnabled += delegate { QMopen(); };
            listener.OnDisabled += delegate { QMclosed(); };

            MelonLogger.Msg("Initialized QM listener!");
        }

        public void QMopen()
        { if (!raiseNearClip) return;
            var screenCamera = GetScreenCam();
            if (screenCamera is null) return;
            float clipValue = screenCamera.nearClipPlane;
            if (clipValue < .0011f && clipValue > .0009f)
                ChangeNearClipPlane(.01f, false);
            oldNearClip = clipValue;
        }

        public void QMclosed()
        { if (!raiseNearClip) return;
            var screenCamera = GetScreenCam();
            if (screenCamera is null) return;
            float clipValue = screenCamera.nearClipPlane;
            if (clipValue != oldNearClip)
                ChangeNearClipPlane(oldNearClip, false);
        }

        private Camera GetScreenCam() 
        {
            VRCVrCamera vrCamera = VRCVrCamera.field_Private_Static_VRCVrCamera_0;
            if (!vrCamera) return null;
            Camera screenCamera = vrCamera.field_Public_Camera_0; //Camera screenCamera = vrCamera.screenCamera;
            if (!screenCamera) return null;
            return screenCamera;
        }

        private void ChangeNearClipPlane(float value, bool printMsg)
        {
            var screenCamera = GetScreenCam();
            if (screenCamera is null) return;
            float oldvalue = screenCamera.nearClipPlane;
            screenCamera.nearClipPlane = value;
            if(printMsg) MelonLogger.Msg($"Near plane previous: {oldvalue}, Now: {value} {(keybindsEnabled ? "- Keyboard Hotkeys: '[' - 0.0001, ']' - 0.05" : "" )}");
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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //MelonLogger.Msg($"index: {buildIndex} sceneName: {sceneName}");
            //Return the clipping distance to a safe, close value on load
            switch (buildIndex)//Without switch this would run 3 times at world load
            {
                case 0: //App
                    break;
                case 1: //ui
                    break;
                case 2: //empty
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
            ChangeNearClipPlane(znear, true);
            oldNearClip = znear;
            MelonLogger.Msg("Near plane adjusted after world load");
        }


        public override void OnUpdate()
        {
            if (!keybindsEnabled) return;

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, "NearClip set to smallest value on keypress: 0.0001");
                ChangeNearClipPlane(.0001f, true);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                MelonLogger.Msg(ConsoleColor.DarkCyan, "NearClip set to largest value on keypress: 0.05");
                ChangeNearClipPlane(.05f, true);
            }
        }


    }
}
