using MelonLoader;
using UnityEngine;
using System;
using System.Collections.Generic;
using VRC;
using VRC.Core;
using UIExpansionKit.API;
using VRChatUtilityKit.Utilities;

[assembly: MelonModInfo(typeof(HideCameraIndicators.HideCameraIndicatorsMod), "HideCameraIndicatorsMod", "0.1", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace HideCameraIndicators
{
    public class HideCameraIndicatorsMod : MelonMod
    {
        private static Dictionary<string, GameObject> camList = new Dictionary<string, GameObject>();
        private static List<string> toRemove = new List<string>();
        private static GameObject UIXButt;

        private const string catagory = "HideCameraIndicatorsMod";

        public static MelonPreferences_Entry<bool> hideCams;
        public static MelonPreferences_Entry<bool> shrinkCams;
        public static MelonPreferences_Entry<bool> hideNameplate;
        public static MelonPreferences_Entry<bool> noUIXButt;

        public override void OnApplicationStart()
        {
            hideCams = MelonPreferences.CreateEntry(catagory, nameof(hideCams), false, "Hide Camera Indicators");
            shrinkCams = MelonPreferences.CreateEntry(catagory, nameof(shrinkCams), false, "Shrink Indicators to .25x, Do not hide entirely");
            noUIXButt = MelonPreferences.CreateEntry(catagory, nameof(noUIXButt), true, "UIX Button in 'Camera' Quick Menu");

            hideNameplate = MelonPreferences.CreateEntry(catagory, nameof(hideNameplate), false, "Hide Camera Nameplates");

            hideCams.OnValueChanged += UpdateAll;
            shrinkCams.OnValueChanged += UpdateAll;
            hideNameplate.OnValueChanged += UpdateAll;

            NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Other User's Camera Indicators", () =>
            {
                var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
                menu.AddToggleButton("Hide Camera Indicators", (action) => hideCams.Value = !hideCams.Value, () => hideCams.Value);
                menu.AddToggleButton("Hide Camera Nameplate", (action) => hideNameplate.Value = !hideNameplate.Value, () => hideNameplate.Value);
                menu.AddToggleButton("Shrink Indicators, do not hide entirely", (action) => shrinkCams.Value = !shrinkCams.Value, () => shrinkCams.Value);
                menu.AddSpacer();
                menu.AddSimpleButton($"Close", () =>
                {
                    menu.Hide();
                });
                menu.Show();
            }, (button) => { UIXButt = button; UIXButt.SetActive(noUIXButt.Value); });

        }

        public override void OnPreferencesSaved()
        {
            UIXButt.SetActive(noUIXButt.Value);
        }

        public static void OnAvatarInstantiated(VRCAvatarManager player, ApiAvatar avatar, GameObject gameObject)
        {
            var avatarObj = player?.prop_GameObject_0;
            var cam = avatarObj?.transform?.root?.Find("UserCameraIndicator")?.gameObject;
            var username = avatarObj?.transform?.root?.GetComponentInChildren<VRCPlayer>()?.prop_String_1;
            if(avatarObj is null || cam is null || username is null)
            {
                MelonLogger.Msg($"avatarObj is null: {avatarObj is null}, cam is null: {cam is null}, username is null: {username is null}");
                return;
            }
            
            //MelonLogger.Msg($"Added Camera Indicator from: {username}");
            if (camList.ContainsKey(username))
                camList[username] = cam;
            else
                camList.Add(username, cam);

            ToggleCam(username);
        }

        public static void OnPlayerLeft(Player player)
        {
            var username = player?._vrcplayer?.prop_String_1;
            //MelonLogger.Msg($"Player left: {username}");
            if (username != null && camList.ContainsKey(username))
                camList.Remove(username);
        }

        private static void UpdateAll(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            //MelonLogger.Msg($"{camList.Count}");
            foreach (KeyValuePair<string, GameObject> user in camList)
            {
                //MelonLogger.Msg($"{user.Key}");
                ToggleCam(user.Key);
            }
            foreach (var s in toRemove)
                camList.Remove(s);
            toRemove.Clear();
        }

        private static void ToggleCam(string username)
        {
            try
            {
                if (camList.TryGetValue(username, out GameObject camObj))
                {
                    if (camObj?.Equals(null) ?? true)
                    {
                        MelonLogger.Msg($"Camera Object is null in Dict for: {username}, removing from Dict");
                        toRemove.Add(username);
                        return;
                    }

                    camObj.transform.Find("Camera Nameplate/Container").localScale = hideNameplate.Value ? new Vector3(.00001f, .00001f, .00001f) : new Vector3(1f, 1f, 1f);

                    if (hideCams.Value)
                        camObj.transform.Find("Indicator/RemoteShape/Camera_Lens").localScale = shrinkCams.Value ? new Vector3(0.05f, 0.05f, 0.05f) : new Vector3(0.00001f, 0.00001f, 0.00001f);
                    else 
                        camObj.transform.Find("Indicator/RemoteShape/Camera_Lens").localScale = new Vector3(.2f, .2f, .2f);
                }
                //MelonLogger.Msg($"Processed: {username}");
            }
            catch (System.Exception ex) { MelonLogger.Error($"ToggleCam:\n" + ex.ToString()); }
        }
     
    }
}

