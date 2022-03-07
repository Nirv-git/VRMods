using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using System.Collections;
using System;


[assembly: MelonModInfo(typeof(DisableControllerOverlay.DisableControllerOverlayMod), "DisableControllerOverlay", "0.9.6", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]

namespace DisableControllerOverlay
{
    public class DisableControllerOverlayMod : MelonMod
    {
        public static MelonLogger.Instance Logger;
        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<bool> disableToolTipsOnLoad;
        public static MelonPreferences_Entry<bool> disableToolTips;
        public static MelonPreferences_Entry<bool> disableMeshOnly;

        public bool loadOnce = false;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("DisableControllerOverlay", ConsoleColor.DarkYellow);

            cat = MelonPreferences.CreateCategory("DisableControllerOverlay", "DisableControllerOverlay");
            disableToolTipsOnLoad = MelonPreferences.CreateEntry("DisableControllerOverlay", nameof(disableToolTipsOnLoad), true, "Disable Controller Tooltips on Game Load");
            disableToolTips = MelonPreferences.CreateEntry("DisableControllerOverlay", nameof(disableToolTips), true, "Disable Controller Tooltips");
            disableMeshOnly = MelonPreferences.CreateEntry("DisableControllerOverlay", nameof(disableMeshOnly), true, "Disable Controller Mesh Only");

            disableToolTips.OnValueChanged += disableChange;
            disableMeshOnly.OnValueChanged += disableChange;

            var toolTipsMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column5Row);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UiElementsQuickMenu).AddSimpleButton("Controller Tooltips", () => toolTipsMenu.Show());
            toolTipsMenu.AddLabel("Controller Tooltips");
            toolTipsMenu.AddToggleButton("Disable Controller Tooltips", (action) =>
            {
                disableToolTips.Value = !disableToolTips.Value;
            }, () => disableToolTips.Value);
            toolTipsMenu.AddToggleButton("Disable Controller Mesh Only", (action) =>
            {
                disableMeshOnly.Value = !disableMeshOnly.Value;
            }, () => disableMeshOnly.Value);
            toolTipsMenu.AddToggleButton("Disable on Game Load", (action) =>
            {
                disableToolTipsOnLoad.Value = !disableToolTipsOnLoad.Value;
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
            if (!disableToolTipsOnLoad.Value) yield break;
            Logger.Msg("Waiting for QM first open");
            //while (GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)") == null)
            while (GameObject.Find("/UserInterface/Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null) //Why wait for the MicButton, because I use this in other mods so I only need to fix one thing if it breaks in the future! Also you can't open the camera without going through the QM
                yield return new WaitForSeconds(1f); //Also because before if we checked for the Controller container to init, and people started their controllers after starting the game, it wouldn't disable their overlays, cause they hadn't been created yet. I assume now people would have their controllers on before opening the QM
            Logger.Msg("QM Opened | Controllers Disabled");
            disableToolTips.Value = true;
            ToggleToolTips(false);
        }



        public void disableChange(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return; //If setting gets updated, enable all Tooltip options, then disable what we currently want to
            ToggleToolTips(true);
            if (disableToolTips.Value) ToggleToolTips(false);
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
                        //Logger.Msg(i +"-"+ child.name);

                        if (child.name.StartsWith("ControllerUI"))// && child.name.EndsWith("(Clone)"))
                        {
                            if (disableMeshOnly.Value || value == true)
                            {
                                Logger.Msg(child.name + " - Mesh objects set to: " + value);
                                foreach (var meshRen in child.GetComponentsInChildren<MeshRenderer>(true))
                                {
                                    //Logger.Msg(meshRen.name);
                                    meshRen.enabled = value;
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
                else Logger.Msg("Controller not found - " + c);
            }
        }
    }
}




namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu1Column5Row = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 5, NumRows = 5 };
    }
}

