using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using System;
using VRC.SDKBase;
using UnhollowerRuntimeLib;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(MirrorLayers.Main), "MirrorLayers", "0.3.1", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace MirrorLayers
{
    public class Main : MelonMod
    {

        public override void OnApplicationStart()
        {
            category.CreateEntry<bool>("MirrorQMButt", false, "MirrorLayers Button on QuickMenu");
            category.CreateEntry<bool>("MirrorBigButt", true, "MirrorLayers Button on Worlds menu");
            defaultMask = category.CreateEntry<int>("DefaultMask", -529441, "Default Mirror Layer Mask");
            setMaskOnJoin = category.CreateEntry<bool>("SetDefaultOnJoin", false, "Set Default Layer Mask On World Join");
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

            if (mirrorQMBtn != null) mirrorQMBtn.gameObject.SetActive(mirrorQMButt);
            if (mirrorBigBtn != null) mirrorBigBtn.gameObject.SetActive(mirrorBigButt);

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
            if (mirror is null) mirrorLayerMenu.AddSimpleButton($"Enable All", () => { SetSingleLayerOnMirrors(0, 5, mirror); });
            else mirrorLayerMenu.AddSimpleButton($"Enable All", () => { SetSingleLayerOnMirrors(0, 5, mirror); UpdateButtons(mirror.m_ReflectLayers); });
            if (mirror is null) mirrorLayerMenu.AddSimpleButton($"Disable All", () => { SetSingleLayerOnMirrors(0, 6, mirror); });
            else mirrorLayerMenu.AddSimpleButton($"Disable All", () => { SetSingleLayerOnMirrors(0, 6, mirror); UpdateButtons(mirror.m_ReflectLayers); });

            foreach (KeyValuePair<int, string> entry in layerList)
            {
                if (mirror is null)
                {
                    mirrorLayerMenu.AddLabel($"Layer #{entry.Key} ----------- \n{entry.Value}");
                    mirrorLayerMenu.AddSimpleButton($"Enable", () => { SetSingleLayerOnMirrors(entry.Key, 1, mirror); });
                    mirrorLayerMenu.AddSimpleButton($"Disable", () => { SetSingleLayerOnMirrors(entry.Key, 0, mirror); });
                }
                else
                { //If editing solo mirror, show current state on lables 
                    mirrorLayerMenu.AddLabel($"Layer #{entry.Key} {(IsLayerEnabled(mirror.m_ReflectLayers.value, entry.Key) ? "++++++++" : "-----------")} \n{entry.Value}", (button) => buttonList[entry.Key] = button.transform);
                    //mirrorLayerMenu.AddToggleButton($"Enable", b => {
                    //    if (b) 
                    //        SetSingleLayerOnMirrors(entry.Key, 1, mirror);
                    //    else
                    //        SetSingleLayerOnMirrors(entry.Key, 0, mirror);

                    //}, () => IsLayerEnabled(mirror.m_ReflectLayers.value, entry.Key));

                    mirrorLayerMenu.AddSimpleButton($"Enable", () => { SetSingleLayerOnMirrors(entry.Key, 1, mirror); UpdateButtons(mirror.m_ReflectLayers.value); });
                    mirrorLayerMenu.AddSimpleButton($"Disable", () => { SetSingleLayerOnMirrors(entry.Key, 0, mirror); UpdateButtons(mirror.m_ReflectLayers.value); });
                }

            }

            mirrorLayerMenu.AddSimpleButton($"View Mirrors", () => { ViewMirrorLayers(useBigMenu); });
            mirrorLayerMenu.AddSimpleButton($"Close", () => { mirrorLayerMenu.Hide(); });
            if (mirror is null) mirrorLayerMenu.AddSimpleButton($"Defaults Menu", () => { DefaultsMenu(useBigMenu); });
            else mirrorLayerMenu.AddSimpleButton($"Back", () => { EditMirrorLayersMenu(useBigMenu, null); });


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

        public void DefaultsMenu(bool useBigMenu)
        {
            ICustomShowableLayoutedMenu mirrorLayerMenu = null;
            mirrorLayerMenu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescriptionCustom.QuickMenu3Column) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column_Longer);

            mirrorLayerMenu.AddLabel("Set Default Layer Mask On World Join");
            mirrorLayerMenu.AddToggleButton("Set on Join", b => setMaskOnJoin.Value = b, () => setMaskOnJoin.Value);
            //mirrorLayerMenu.AddLabel($"Mask: {defaultMask.Value}");
            mirrorLayerMenu.AddSpacer();

            mirrorLayerMenu.AddSpacer();
            mirrorLayerMenu.AddSimpleButton($"Enable All", () => { defaultMask.Value = -1; UpdateButtons(defaultMask.Value); });
            mirrorLayerMenu.AddSimpleButton($"Disable All", () => { defaultMask.Value = 0; UpdateButtons(defaultMask.Value); });

            foreach (KeyValuePair<int, string> entry in layerList)
            {
                mirrorLayerMenu.AddLabel($"Layer #{entry.Key} {(IsLayerEnabled(defaultMask.Value, entry.Key) ? "++++++++" : "-----------")} \n{entry.Value}", (button) => buttonList[entry.Key] = button.transform);
                mirrorLayerMenu.AddSimpleButton($"Enable", () => { defaultMask.Value = defaultMask.Value | (1 << entry.Key); UpdateButtons(defaultMask.Value); });//Remove Layer
                mirrorLayerMenu.AddSimpleButton($"Disable", () => { defaultMask.Value = defaultMask.Value & ~(1 << entry.Key); UpdateButtons(defaultMask.Value); }); //Add Layer
            }

            mirrorLayerMenu.AddSimpleButton($"Close", () => { mirrorLayerMenu.Hide(); });
            mirrorLayerMenu.AddSimpleButton($"Back", () => { EditMirrorLayersMenu(useBigMenu, null); });

            mirrorLayerMenu.Show();
        }

        public string FindLayerString(int value)
        {
            string layerStr = "";
            foreach (KeyValuePair<int, string> entry in layerList)
            {
                layerStr += $"{entry.Key}:{( ((value & (1 << entry.Key)) != 0) ? "E":"D" )}{( (entry.Key != 11 && entry.Key != 21 && entry.Key != 31) ? ", " : "" )}";
                if (entry.Key == 11 || entry.Key == 21) layerStr += "\n";
            }
            return layerStr;
        }

        public bool IsLayerEnabled(int mirrorValue, int layer)
        {
            if ((mirrorValue & (1 << layer)) != 0)
                return true;
            else return false;
        }

        public void UpdateButtons(int mirrorValue)
        {
            foreach (KeyValuePair<int, string> entry in layerList)
            {
                buttonList[entry.Key].GetComponentInChildren<Text>().text = $"Layer #{entry.Key} {(IsLayerEnabled(mirrorValue, entry.Key) ? "++++++++" : "-----------")} \n{entry.Value}";
            }
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
            foreach (var vrcMirrorReflection in UnityEngine.Object.FindObjectsOfType<VRC_MirrorReflection>()) //Only active
            {
                if (mirror == null || mirror == vrcMirrorReflection)
                {
                    switch (state)
                    {
                        case 0: vrcMirrorReflection.m_ReflectLayers = vrcMirrorReflection.m_ReflectLayers.value & ~(1 << layer); break; //Remove Layer
                        case 1: vrcMirrorReflection.m_ReflectLayers = vrcMirrorReflection.m_ReflectLayers.value | (1 << layer); break; //Add Layer
                        case 5: vrcMirrorReflection.m_ReflectLayers = -1; break; //All
                        case 6: vrcMirrorReflection.m_ReflectLayers = 0; break; //None
                        case 10: vrcMirrorReflection.m_ReflectLayers = -1 & ~UiLayer & ~UiMenuLayer & ~PlayerLocalLayer & ~UiCamLayer; break; //Full
                        case 11: vrcMirrorReflection.m_ReflectLayers = PlayerLayer | MirrorReflectionLayer; break; // Opt
                        default: MelonLogger.Msg(ConsoleColor.Red, "SetSingleLayerOnMirrors hit default case in Switch - Something is wrong"); break;
                    }
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //MelonLogger.Msg($"re: {setMaskOnJoin}");
            if (!setMaskOnJoin.Value) return;
            //MelonLogger.Msg($"index: {buildIndex} sceneName: {sceneName}");
            switch (buildIndex)//Without switch this would run 3 times at world load
            {
                case 0: break;//App
                case 1: break;//ui
                case 2: break;//empty
                default:
                    MelonLogger.Msg("Setting all mirrors to default Mask");
                    foreach (var vrcMirrorReflection in UnityEngine.Object.FindObjectsOfTypeAll(Il2CppType.Of<VRC_MirrorReflection>())) //All Possible
                    {
                        try 
                        {
                            //MelonLogger.Msg(vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name); 
                            //MelonLogger.Msg(vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.parent?.name);
                            if (!(vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorFull" ||
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorCutoutSolo" ||
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorTransparentSolo" ||
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorCutout" ||
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorOpt" ||
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.name == "MirrorTransparent" )
                                &&
                                !(vrcMirrorReflection.TryCast<VRC_MirrorReflection>()?.gameObject?.transform?.parent?.name == "MirrorPrefab") )
                            {
                                vrcMirrorReflection.TryCast<VRC_MirrorReflection>().m_ReflectLayers = defaultMask.Value;
                            }
                            //else MelonLogger.Msg("Skipped as from PortableMirrorMod");

                        }
                        catch (System.Exception ex) { MelonLogger.Msg("Error in setting Mask" + ex.ToString()); }
                    }
                    break;
            }
        }


        MelonPreferences_Category category = MelonPreferences.CreateCategory("MirrorLayersMod", "Mirror Layers Menu");
        private bool mirrorQMButt;
        private bool mirrorBigButt;
        private Transform mirrorQMBtn;
        private Transform mirrorBigBtn;
        public static MelonPreferences_Entry<int> defaultMask;
        public static MelonPreferences_Entry<bool> setMaskOnJoin;

        private Dictionary<int, string>layerList = new Dictionary<int, string>() //http://vrchat.wikidot.com/worlds:layers
            {
                {0, "Default*"},
                {1, "TransparentFX"},
                {2, "Ignore Raycast"},
                {3, ""},
                {4, "Water"},
                {5, "UI*"},
                {6, ""},
                {7, ""},
                {8, "Interactive"},
                {9, "Player*"},
                {10, "PlayerLocal*"},
                {11, "Environment"},
                {12, "UiMenu*"},
                {13, "Pickup"},
                {14, "PickupNoEnvironment"},
                {15, "SteroLeft"},
                {16, "StereoRight"},
                {17, "Walkthrough"},
                {18, "MirrorReflection*"},
                {19, "reserved2(UI)*"},
                {20, "reserved3"},
                {21, "reserved4"},
                {22, "user0"},
                {23, "user1"},
                {24, "user2"},
                {25, "user3"},
                {26, "user4"},
                {27, "user5"},
                {28, "user6"},
                {29, "user7"},
                {30, "user8"},
                {31, "user9"}
             };

        private Dictionary<int, Transform> buttonList = new Dictionary<int, Transform>()
            {
                {0, new Transform ()},
                {1, new Transform ()},
                {2, new Transform ()},
                {3, new Transform ()},
                {4, new Transform ()},
                {5, new Transform ()},
                {6, new Transform ()},
                {7, new Transform ()},
                {8, new Transform ()},
                {9, new Transform ()},
                {10, new Transform ()},
                {11, new Transform ()},
                {12, new Transform ()},
                {13, new Transform ()},
                {14, new Transform ()},
                {15, new Transform ()},
                {16, new Transform ()},
                {17, new Transform ()},
                {18, new Transform ()},
                {19, new Transform ()},
                {20, new Transform ()},
                {21, new Transform ()},
                {22, new Transform ()},
                {23, new Transform ()},
                {24, new Transform ()},
                {25, new Transform ()},
                {26, new Transform ()},
                {27, new Transform ()},
                {28, new Transform ()},
                {29, new Transform ()},
                {30, new Transform ()},
                {31, new Transform ()}
             };

        private static int PlayerLayer = 1 << 9; // https://github.com/knah/VRCMods/blob/master/MirrorResolutionUnlimiter/UiExtensionsAddon.cs
        private static int PlayerLocalLayer = 1 << 10; //Mainly just here as a refernce now
        private static int UiLayer = 1 << 5;
        private static int UiMenuLayer = 1 << 12;
        private static int MirrorReflectionLayer = 1 << 18;
        private static int UiCamLayer = 1 << 19;
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