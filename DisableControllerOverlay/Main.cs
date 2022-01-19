using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using System.Collections;
using System;


[assembly: MelonModInfo(typeof(DisableControllerOverlay.DisableControllerOverlayMod), "DisableControllerOverlay", "0.9.5", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]

namespace DisableControllerOverlay
{
    public class DisableControllerOverlayMod : MelonMod
    {
        public static MelonLogger.Instance Logger;
        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<bool> disableToolTipsOnLoad;
        public static MelonPreferences_Entry<bool> disableMeshOnly;

        public bool loadOnce = false;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("DisableControllerOverlay", ConsoleColor.DarkYellow);

            cat = MelonPreferences.CreateCategory("DisableControllerOverlay", "DisableControllerOverlay");
            disableToolTipsOnLoad = MelonPreferences.CreateEntry("DisableControllerOverlay", nameof(disableToolTipsOnLoad), false, "Disable Controller Tooltips on Game Load");
            disableMeshOnly = MelonPreferences.CreateEntry("DisableControllerOverlay", nameof(disableMeshOnly), true, "Disable Controller Mesh Only");

            var toolTipsMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.WideSlimList);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UiElementsQuickMenu).AddSimpleButton("Controller Tooltips", () => toolTipsMenu.Show());
            toolTipsMenu.AddLabel("Enable/Disable the Controller Tooltips");
            toolTipsMenu.AddSimpleButton("Enable Tooltips", (() => ToggleToolTips(true)));
            toolTipsMenu.AddSimpleButton("Disable Tooltips", (() => ToggleToolTips(false)));
            toolTipsMenu.AddToggleButton("Disable Controller Mesh Only", (action) =>
            {
                disableMeshOnly.Value = !disableMeshOnly.Value;
                cat.SaveToFile();
            }, () => disableMeshOnly.Value);
            toolTipsMenu.AddToggleButton("Disable on Game Load", (action) =>
            {
                disableToolTipsOnLoad.Value = !disableToolTipsOnLoad.Value;
                cat.SaveToFile();
            }, () => disableToolTipsOnLoad.Value);
            toolTipsMenu.AddSimpleButton("Close", () => toolTipsMenu.Hide());
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (loadOnce) return;
            switch (buildIndex)//Without switch this would run 3 times at world load
            {
                case 0: break;//App
                case 1: break;//ui
                case 2: break;//empty
                default:
                    loadOnce = true;
                    MelonCoroutines.Start(OnLoad());
                    break;
            }
        }

        public IEnumerator OnLoad()
        {
            while (GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)") == null)
                yield return new WaitForSeconds(1f);
            if (disableToolTipsOnLoad.Value) ToggleToolTips(false);
        }


        private void ToggleToolTips(bool value)
        {
            var cons = new string[] { 
                "_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)",
                "_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (right)" };

            foreach (var c in cons)
            {
                GameObject Con = GameObject.Find(c);
                if (Con != null)
                {
                    for (int i = 0; i < Con.transform.childCount; i++)
                    {
                        GameObject child = Con.transform.GetChild(i).gameObject;
                        if (child.name.StartsWith("ControllerUI") && child.name.EndsWith("(Clone)"))
                        {
                            if (disableMeshOnly.Value || value == true)
                            {
                                Logger.Msg(child.name + " - Mesh root objects set to: " + value);
                                var root = child.transform.Find("root").transform;
                                for (int r = 0; r < root.childCount; r++)
                                {
                                    root.GetChild(r).gameObject.SetActive(value);
                                }
                            }
                            if (!disableMeshOnly.Value || value == true)
                            {
                                Logger.Msg(child.name + " - Set to: " + value);
                                child.SetActive(value);
                            }
                        }
                    }
                }
            }
        }
    }
}

