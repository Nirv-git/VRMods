using MelonLoader;
using UnityEngine;
using System;
using System.Collections.Generic;
using VRC;
using VRC.Core;
using UIExpansionKit.API;
using VRChatUtilityKit.Utilities;

[assembly: MelonModInfo(typeof(HideCameraIndicators.HideCameraIndicatorsMod), "HideCameraIndicatorsMod", "0.3", "Nirvash")]
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
        public static MelonPreferences_Entry<bool> recolorCams;
        public static MelonPreferences_Entry<bool> hideCamTex;
        public static MelonPreferences_Entry<Color> camColor;
        public static MelonPreferences_Entry<bool> noGloss;


        public override void OnApplicationStart()
        {
            hideCams = MelonPreferences.CreateEntry(catagory, nameof(hideCams), false, "Hide Camera Indicators");
            shrinkCams = MelonPreferences.CreateEntry(catagory, nameof(shrinkCams), false, "Shrink Indicators to .25x, Do not hide entirely");
            hideNameplate = MelonPreferences.CreateEntry(catagory, nameof(hideNameplate), false, "Hide Camera Nameplates");

            hideCamTex = MelonPreferences.CreateEntry(catagory, nameof(hideCamTex), false, "Hide Camera Texture");
            recolorCams = MelonPreferences.CreateEntry(catagory, nameof(recolorCams), false, "Recolor Cameras");
            noGloss = MelonPreferences.CreateEntry(catagory, nameof(noGloss), false, "Less glossiness on Cameras");

            noUIXButt = MelonPreferences.CreateEntry(catagory, nameof(noUIXButt), true, "UIX Button in 'Camera' Quick Menu");

            camColor = MelonPreferences.CreateEntry(catagory, nameof(camColor), Color.black, "", "", true);
            colorList["Red"] = camColor.Value.r*100; colorList["Green"] = camColor.Value.g*100; colorList["Blue"] = camColor.Value.b*100;


            hideCams.OnValueChanged += UpdateAll;
            shrinkCams.OnValueChanged += UpdateAll;
            hideNameplate.OnValueChanged += UpdateAll;
            recolorCams.OnValueChanged += UpdateAll;
            hideCamTex.OnValueChanged += UpdateAll;
            noGloss.OnValueChanged += UpdateAll;

            NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Other User's Camera Indicators", () =>
            {
                CamSettingsMenu();
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
            var username = avatarObj?.transform?.root?.GetComponentInChildren<VRC.Player>()?.field_Private_APIUser_0.displayName;
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
            var username = player.field_Private_APIUser_0.displayName;
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
                //MelonLogger.Msg($"Processing: {username}");
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

                    if (!camObj?.transform?.root?.name.Contains("[Local]") ?? true) return;
                    //MelonLogger.Msg($"Processed Shared Mat for: {camObj?.transform?.root?.name}");
                    camObj.transform.Find("Indicator/RemoteShape/Camera_Lens/UserCamera_Lens_New").GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = hideCamTex.Value ? new Vector2(.1f, .1f) : new Vector2(1, 1);
                    camObj.transform.Find("Indicator/RemoteShape/Camera_Lens/UserCamera_Lens_New").GetComponent<MeshRenderer>().sharedMaterial.mainTextureOffset = hideCamTex.Value ? new Vector2(.2f, .3f) : new Vector2(0f, 0f);

                    camObj.transform.Find("Indicator/RemoteShape/Camera_Lens/UserCamera_Lens_New").GetComponent<MeshRenderer>().sharedMaterial.color = recolorCams.Value ? camColor.Value : new Color(.8f, .8f, .8f);
                    camObj.transform.Find("Indicator/RemoteShape/Camera_Lens/UserCamera_Lens_New").GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_GlossMapScale", noGloss.Value ? 0f : 1f);
                }
            }
            catch (System.Exception ex) { MelonLogger.Error($"ToggleCam:\n" + ex.ToString()); }
        }

        private static void CamSettingsMenu()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            menu.AddToggleButton("Hide Camera Indicators", (action) => hideCams.Value = !hideCams.Value, () => hideCams.Value);
            menu.AddToggleButton("Hide Camera Nameplate", (action) => hideNameplate.Value = !hideNameplate.Value, () => hideNameplate.Value);
            menu.AddToggleButton("Shrink Indicators, do not hide entirely", (action) => shrinkCams.Value = !shrinkCams.Value, () => shrinkCams.Value);

            menu.AddToggleButton("Hide Camera Texture", (action) => hideCamTex.Value = !hideCamTex.Value, () => hideCamTex.Value);
            menu.AddToggleButton("Less glossiness on Cameras", (action) => noGloss.Value = !noGloss.Value, () => noGloss.Value);
            menu.AddToggleButton("Recolor Cameras", (action) => recolorCams.Value = !recolorCams.Value, () => recolorCams.Value);
            
            menu.AddSimpleButton($"Close", () =>
            {
                menu.Hide();
            });
            menu.AddSpacer();
            menu.AddSimpleButton($"Color (R,G,B):\n{camColor.Value.r * 100}, {camColor.Value.g * 100}, {camColor.Value.b * 100}", () => ColorMenuAdj());
     
            menu.Show();
        }

        private static Dictionary<string, float> colorList = new Dictionary<string, float> {
                { "Red", 100 }, { "Green", 100 }, { "Blue", 100 } };
        private static void ColorMenuAdj()
        {
            ICustomShowableLayoutedMenu Menu = null;
            Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column4Row);
            foreach (KeyValuePair<string, float> entry in colorList)
            {
                string c = entry.Key;
                Menu.AddSimpleButton($"{(c)} -", () => {
                    colorList[c] = Clamp(entry.Value - 10, 0, 100);
                    Menu.Hide(); ColorMenuAdj();
                });
                Menu.AddSimpleButton($"{(c)} - 0/100", () => {
                    if (colorList[c] != 0f)
                        colorList[c] = 0f;
                    else
                        colorList[c] = 100f;
                    Menu.Hide(); ColorMenuAdj();
                });
                Menu.AddSimpleButton($"{(c)} +", () => {
                    colorList[c] = Clamp(colorList[c] + 10, 0, 100);
                    Menu.Hide(); ColorMenuAdj();
                });
            }
            Menu.AddSimpleButton($"<-Back", () => { CamSettingsMenu(); });
            Menu.AddLabel($"R:{colorList["Red"]}\nG:{colorList["Green"]}\nB:{colorList["Blue"]}");
            camColor.Value = new Color(colorList["Red"] / 100, colorList["Green"] / 100, colorList["Blue"] / 100);
            Menu.Show();
        }

        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}

namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        // QuickMenu3Columns = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 3, NumRows = 3 };
        // QuickMenu4Columns = new LayoutDescription { NumColumns = 4, RowHeight = 95, NumRows = 4 };
        // WideSlimList = new LayoutDescription { NumColumns = 1, RowHeight = 50, NumRows = 8 };
        public static LayoutDescription QuickMenu3Column4Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 4, NumRows = 4 };
    }
}