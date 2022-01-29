using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using VRC.UserCamera;
using ConsoleColor = System.ConsoleColor;
using UnhollowerRuntimeLib;
using NearClipPlaneAdj.Components;
using System.Collections;
using System.Linq;
using System.Net;
using System;

[assembly: MelonInfo(typeof(NearClipPlaneAdj.NearClipPlaneAdjMod), "NearClipPlaneAdj", "1.63", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("ActionMenuApi")]

namespace NearClipPlaneAdj
{
    public class NearClipPlaneAdjMod : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Entry<bool> changeClipOnLoad;
        public static MelonPreferences_Entry<bool> keybindsEnabled;
        public static MelonPreferences_Entry<bool> smallerDefault;
        public static MelonPreferences_Entry<bool> raiseNearClip;
        public static MelonPreferences_Entry<bool> changeUIcam;
        public static MelonPreferences_Entry<bool> AMAPI_en;
        public static MelonPreferences_Entry<bool> amapi_ModsFolder;
        public static MelonPreferences_Entry<bool> UIX_butts_QM;
        public static MelonPreferences_Entry<bool> defaultChangeBlackList;

        public static string blackList;
        public static float oldNearClip;
        public static float lastSetNearClip;
        public static Camera UIcamera;

        private static GameObject n05, n01, n001, n0001, QMbutt;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("NearClipPlaneAdj", ConsoleColor.DarkYellow);

            MelonPreferences.CreateCategory("NearClipAdj", "NearClipPlane Adjuster");
            changeClipOnLoad = MelonPreferences.CreateEntry<bool>("NearClipAdj", "changeClipOnLoad", true, "Change NearClip on world load");
            UIX_butts_QM = MelonPreferences.CreateEntry("NearClipAdj", "UIX_butts_QM", true, "Place buttons in Settings QM instead of Settings Big Menu");
            keybindsEnabled = MelonPreferences.CreateEntry<bool>("NearClipAdj", "Keyboard", true, "Keyboard Shortcuts: '[' - 0.0001, ']' - 0.05");

            changeUIcam = MelonPreferences.CreateEntry<bool>("NearClipAdj", "changeUIcam", true, "Change UI Camera Nearclip");
            smallerDefault = MelonPreferences.CreateEntry<bool>("NearClipAdj", "SmallerDefault", false, "Smaller Default Nearclip on World Change - 0.001 vs 0.01");
            raiseNearClip = MelonPreferences.CreateEntry<bool>("NearClipAdj", "RaiseOnQuickMenu", false, "If using smaller Default Nearclip (0.001) raise to 0.01 when Quick Menu opens.");
            
            AMAPI_en = MelonPreferences.CreateEntry<bool>("NearClipAdj", "AMAPI_en", true, "Action Menu API Support (Requires Restart)");
            amapi_ModsFolder = MelonPreferences.CreateEntry("NearClipAdj", "amapi_ModsFolder", false, "Place Action Menu in 'Mods' Sub Menu instead of 'Config' menu (Restert Required)");
            defaultChangeBlackList = MelonPreferences.CreateEntry("NearClipAdj", "defaultChangeBlackList", true, "Check a blacklist for worlds to not /auto/ change the NearClip on");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.05", (() => ChangeNearClipPlane(.05f, true)), (butt) => { n05 = butt; butt.SetActive(!UIX_butts_QM.Value); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.01", (() => ChangeNearClipPlane(.01f, true)), (butt) => { n01 = butt; butt.SetActive(!UIX_butts_QM.Value); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.001", (() => ChangeNearClipPlane(.001f, true)), (butt) => { n001 = butt; butt.SetActive(!UIX_butts_QM.Value); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Nearplane-0.0001", (() => ChangeNearClipPlane(.0001f, true)), (butt) => { n0001 = butt; butt.SetActive(!UIX_butts_QM.Value); });

            var clips = new float[] {
                .05f,
                .01f,
                .001f,
                .0001f, };

            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column6Row);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UiElementsQuickMenu).AddSimpleButton("Near Clipping Plane Distance", () => Menu.Show(), (butt) => { QMbutt = butt; butt.SetActive(UIX_butts_QM.Value); });
            Menu.AddLabel("Near Clipping Plane Distance");
            foreach (var clip in clips)
            {
                Menu.AddSimpleButton($"Nearplane-{clip}", (() => ChangeNearClipPlane(clip, true)));
            }
            Menu.AddSimpleButton("Close", () => Menu.Hide());

            Logger.Msg("Registering components...");
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonCoroutines.Start(OnLoad());

            if (MelonHandler.Mods.Any(m => m.Info.Name == "ActionMenuApi") && AMAPI_en.Value)
            {
                CustomActionMenu.InitUi();
            }
            else Logger.Msg("ActionMenuApi is missing, or setting is toggled off in Mod Settings - Not adding controls to ActionMenu");

            if(defaultChangeBlackList.Value) GetBlackList();
        }
        private void GetBlackList()
        {
            try
            {
                string url = "https://raw.githubusercontent.com/Nirv-git/VRMods/main/NearClippingPlaneAdjuster/blacklist";
                WebClient client = new WebClient();
                blackList = client.DownloadString(url);
            }
            catch (Exception ex) { Logger.Error($"GetBlackList error\n" + ex.ToString()); }
        }

        public override void OnPreferencesSaved()
        {
            if (!n05?.Equals(null) ?? false) n05.SetActive(!UIX_butts_QM.Value);
            if (!n01?.Equals(null) ?? false) n01.SetActive(!UIX_butts_QM.Value);
            if (!n001?.Equals(null) ?? false) n001.SetActive(!UIX_butts_QM.Value);
            if (!n0001?.Equals(null) ?? false) n0001.SetActive(!UIX_butts_QM.Value);
            if (!QMbutt?.Equals(null) ?? false) QMbutt.SetActive(UIX_butts_QM.Value);
        }

        public IEnumerator OnLoad()
        {
            Logger.Msg("Adding QM listener...."); //From https://github.com/tetra-fox/QMFreeze/blob/master/QMFreeze/Mod.cs
            while (GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null)
                yield return new WaitForSeconds(1f);
            EnableDisableListener listener = GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/MicButton").gameObject
                .AddComponent<EnableDisableListener>();
            listener.OnEnabled += delegate { QMopen(); };
            listener.OnDisabled += delegate { QMclosed(); };

            string vrCamPath = "_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Neck/Camera (eye)/StackedCamera : Cam_InternalUI";
            string desktopCamPath = "_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Neck/Camera (head)/Camera (eye)/StackedCamera : Cam_InternalUI";
            
            try
            {
            if (GameObject.Find(vrCamPath) != null)
                UIcamera = GameObject.Find(vrCamPath).GetComponent<Camera>();
            else
                UIcamera = GameObject.Find(desktopCamPath).GetComponent<Camera>();
            }
            catch (System.Exception ex) { Logger.Error($"Error finding UI Camera\n" + ex.ToString()); }

            Logger.Msg("Initialized QM listener + UICam Found!");
        }

        public void QMopen()
        {
            if (!raiseNearClip.Value) return;
            var screenCamera = GetScreenCam();
            if (screenCamera is null) return;
            float clipValue = screenCamera.nearClipPlane;
            if (clipValue < .0011f && clipValue > .0009f)
                ChangeNearClipPlane(.01f, false);
            oldNearClip = clipValue;
        }

        public void QMclosed()
        {
            if (!raiseNearClip.Value) return;
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

        public void ChangeNearClipPlane(float value, bool printMsg)
        {
            var screenCamera = GetScreenCam();
            if (screenCamera is null) return;
            float oldvalue = screenCamera.nearClipPlane;
            screenCamera.nearClipPlane = value;
            if (printMsg) Logger.Msg($"Near plane previous: {oldvalue}, Now: {value} {(keybindsEnabled.Value ? "- Keyboard Hotkeys: '[' - 0.0001, ']' - 0.05" : "")}");
            ChangePhotoCameraNearField(value);
            oldNearClip = screenCamera.nearClipPlane;

            try
            {
                if (changeUIcam.Value)
                        UIcamera.nearClipPlane = value;
            }
            catch (System.Exception ex) { Logger.Error($"Error changing UI Camera nearclip\n" + ex.ToString()); }
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
                    if (changeClipOnLoad.Value) MelonCoroutines.Start(SetNearClipPlane(0.01f));
                    break;
            }

        }

        System.Collections.IEnumerator SetNearClipPlane(float znear)
        {
            yield return new WaitForSecondsRealtime(15); //Wait 15 seconds after world load before setting the clipping value. Waiting for the next/first frame does not work
            if (defaultChangeBlackList.Value && RoomManager.field_Internal_Static_ApiWorld_0 != null && !blackList.Equals(null))
            { //Check if world is blacklisted from auto change
                if (blackList.Contains(RoomManager.field_Internal_Static_ApiWorld_0.id)) //Lazy way to check, but shouldn't be that bad?
                {
                    Logger.Msg(ConsoleColor.Yellow,"Not auto adjusting NearClipping plane, world blacklisted");
                    yield break;
                }
            }
            
            if (smallerDefault.Value) znear = 0.001f;
            ChangeNearClipPlane(znear, true);
            oldNearClip = znear;
            Logger.Msg("Near plane adjusted after world load");
        }


        public override void OnUpdate()
        {
            if (!keybindsEnabled.Value) return;

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                Logger.Msg(ConsoleColor.DarkCyan, "NearClip set to smallest value on keypress: 0.0001");
                ChangeNearClipPlane(.0001f, true);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                Logger.Msg(ConsoleColor.DarkCyan, "NearClip set to largest value on keypress: 0.05");
                ChangeNearClipPlane(.05f, true);
            }
        }
    }
}

namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu1Column6Row = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 6, NumRows = 6 };
    }
}