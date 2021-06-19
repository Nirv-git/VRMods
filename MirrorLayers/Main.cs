using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using System;
using VRC.SDKBase;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(MirrorLayers.Main), "MirrorLayers", "0.2", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace MirrorLayers
{
    public class Main : MelonMod
    {

        public override void OnApplicationStart()
        {
            category.CreateEntry<bool>("MirrorQMButt", false, "MirrorLayers Button on QuickMenu");
            category.CreateEntry<bool>("MirrorBigButt", true, "MirrorLayers Button on QuickMenu");
            OnPreferencesSaved();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Edit Mirror Layers", () => { EditMirrorLayersMenu(false, null); },
                (button) => { mirrorQMBtn = button.transform; button.gameObject.SetActive(mirrorQMButt); });

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.WorldMenu).AddSimpleButton("Edit Mirror Layers", () => { EditMirrorLayersMenu(true, null); },
                (button) => { mirrorBigBtn = button.transform; button.gameObject.SetActive(mirrorBigButt); });
        }

        public override void OnPreferencesSaved()
        {
            mirrorQMButt = category.GetEntry<bool>("MirrorQMButt").Value;
            mirrorBigButt = category.GetEntry<bool>("MirrorBigButt").Value;
            if(mirrorQMBtn != null) mirrorQMBtn.gameObject.SetActive(mirrorQMButt);
            if(mirrorBigBtn != null) mirrorBigBtn.gameObject.SetActive(mirrorBigButt);

        }

        public void EditMirrorLayersMenu(bool useBigMenu, VRC_MirrorReflection mirror)
        {
            ICustomShowableLayoutedMenu mirrorLayerMenu = null;
            mirrorLayerMenu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescriptionCustom.QuickMenu3Column) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column_Longer);

            if (mirror is null) mirrorLayerMenu.AddLabel("Enable/Disable Layers on All Mirrors in a world");
            else mirrorLayerMenu.AddLabel("Enable/Disable Layers on specific mirror:"); 
            if (mirror is null) mirrorLayerMenu.AddSimpleButton($"All to Full", () => { SetSingleLayerOnMirrors(0, 10, mirror); });
            else mirrorLayerMenu.AddLabel($"{mirror.gameObject.name}");
            if (mirror is null) mirrorLayerMenu.AddSimpleButton($"All to Optimized", () => { SetSingleLayerOnMirrors(0, 11, mirror); });
            else mirrorLayerMenu.AddLabel($"{GetHierarchyPath(mirror.gameObject.transform)}");

            mirrorLayerMenu.AddSimpleButton($"View Mirrors", () => { ViewMirrorLayers(useBigMenu); });
            mirrorLayerMenu.AddSimpleButton($"Enable All", () => { SetSingleLayerOnMirrors(0, 5, mirror); });
            mirrorLayerMenu.AddSimpleButton($"Disable All", () => { SetSingleLayerOnMirrors(0, 6, mirror); });

            foreach (KeyValuePair<int, string> entry in layerList)
            {
                mirrorLayerMenu.AddLabel($"Layer #{entry.Key} ----------- \n{entry.Value}");
                mirrorLayerMenu.AddSimpleButton($"Enable", () => { SetSingleLayerOnMirrors(entry.Key, 1, mirror); });
                mirrorLayerMenu.AddSimpleButton($"Disable", () => { SetSingleLayerOnMirrors(entry.Key, 0, mirror); });
            }

            mirrorLayerMenu.AddSimpleButton($"View Mirrors", () => { ViewMirrorLayers(useBigMenu); });
            mirrorLayerMenu.AddSimpleButton($"Close", () => { mirrorLayerMenu.Hide(); });
            mirrorLayerMenu.Show();
        }

        public void ViewMirrorLayers(bool useBigMenu)
        {
            ICustomShowableLayoutedMenu mirrorLayerView = null;
            mirrorLayerView = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.WideSlimList);
            mirrorLayerView.AddLabel("All mirrors in world\n Layer format is 'Layer:E/D' E - Enabled, D-Disabled");
            foreach (var vrcMirrorReflection in UnityEngine.Object.FindObjectsOfType<VRC_MirrorReflection>())
            {
                mirrorLayerView.AddSimpleButton($"Edit Mirror: {vrcMirrorReflection.gameObject.name}", () => { EditMirrorLayersMenu(useBigMenu, vrcMirrorReflection); });
                mirrorLayerView.AddLabel($"Path: {GetHierarchyPath(vrcMirrorReflection.gameObject.transform)}");
                mirrorLayerView.AddLabel($"{FindLayerString(vrcMirrorReflection.m_ReflectLayers.value)}");
            }
            mirrorLayerView.AddSimpleButton($"Back", () => { EditMirrorLayersMenu(useBigMenu, null); });
            mirrorLayerView.AddSimpleButton($"Close", () => { mirrorLayerView.Hide(); });
            mirrorLayerView.Show();
        }

        public string FindLayerString(int value)
        {
            string layerStr = "";
            foreach (KeyValuePair<int, string> entry in layerList)
            {
                layerStr += $"{entry.Key}:{( ((value & (1 << entry.Key)) != 0) ? "E":"D" )} {( (entry.Key != 11 && entry.Key != 21) ? ", " : "" )}";
                if (entry.Key == 11) layerStr += "\n";
            }
            return layerStr;
        }

        public string GetHierarchyPath(Transform self)
        {
            string path = self.gameObject.name;
            Transform parent = self.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static void SetSingleLayerOnMirrors(int layer, int state, VRC_MirrorReflection mirror)
        {
            foreach (var vrcMirrorReflection in UnityEngine.Object.FindObjectsOfType<VRC_MirrorReflection>())
            {
                if (mirror == null || mirror == vrcMirrorReflection)
                {
                    switch (state)
                    {
                        case 0: vrcMirrorReflection.m_ReflectLayers = vrcMirrorReflection.m_ReflectLayers.value & ~(1 << layer); break; //Remove Layer
                        case 1: vrcMirrorReflection.m_ReflectLayers = vrcMirrorReflection.m_ReflectLayers.value | (1 << layer); break; //Add Layer
                        case 5: vrcMirrorReflection.m_ReflectLayers = -1; break; //Remove Layer
                        case 6: vrcMirrorReflection.m_ReflectLayers = 0; break; //Add Layer
                        case 10: vrcMirrorReflection.m_ReflectLayers = -1 & ~UiLayer & ~UiMenuLayer & ~PlayerLocalLayer; break; //Full
                        case 11: vrcMirrorReflection.m_ReflectLayers = PlayerLayer | MirrorReflectionLayer; break; // Opt
                        default: MelonLogger.Msg(ConsoleColor.Red, "SetSingleLayerOnMirrors hit default case in Switch - Something is wrong"); break;
                    }
                }
            }
        }


        MelonPreferences_Category category = MelonPreferences.CreateCategory("MirrorLayersMod", "Mirror Layers Menu");
        private bool mirrorQMButt;
        private bool mirrorBigButt;
        private Transform mirrorQMBtn;
        private Transform mirrorBigBtn;

        private Dictionary<int, string>layerList = new Dictionary<int, string>() //http://vrchat.wikidot.com/worlds:layers
            {
                {0, "Default"},
                {1, "TransparentFX"},
                {2, "Ignore Raycast"},
                {3, ""},
                {4, "Water"},
                {5, "UI"},
                {6, ""},
                {7, ""},
                {8, "Interactive"},
                {9, "Player"},
                {10, "PlayerLocal"},
                {11, "Environment"},
                {12, "UiMenu"},
                {13, "Pickup"},
                {14, "PickupNoEnvironment"},
                {15, "SteroLeft"},
                {16, "StereoRight"},
                {17, "Walkthrough"},
                {18, "MirrorReflection"},
                {19, "reserved2"},
                {20, "reserved3"},
                {21, "reserved4"}
             };

        private static int PlayerLayer = 1 << 9; // https://github.com/knah/VRCMods/blob/master/MirrorResolutionUnlimiter/UiExtensionsAddon.cs
        private static int PlayerLocalLayer = 1 << 10; //Mainly just here as a refernce now
        private static int UiLayer = 1 << 5;
        private static int UiMenuLayer = 1 << 12;
        private static int MirrorReflectionLayer = 1 << 18;
        //public static int reserved2 = 1 << 19;
        //private static int optMirrorMask = PlayerLayer | MirrorReflectionLayer;
        //private static int fullMirrorMask = -1 & ~UiLayer & ~UiMenuLayer & ~PlayerLocalLayer;
    }
}

namespace UIExpansionKit.API
{ 
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu3Column = new LayoutDescription { NumColumns = 3, RowHeight = 420 / 11, NumRows = 11 }; //8 was 380
        public static LayoutDescription QuickMenu3Column_Longer = new LayoutDescription { NumColumns = 3, RowHeight = 475 / 14, NumRows = 14 };
    }
}