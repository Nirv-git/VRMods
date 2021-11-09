using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;
using System.Collections;
using System;

[assembly: MelonModInfo(typeof(DisableOneHandMovementUI.DisableOneHandMovementUIMod), "DisableOneHandMovementUI", "0.3.6", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]

namespace DisableOneHandMovementUI
{
    public class DisableOneHandMovementUIMod : MelonMod
    {
        private const string catagory = "DisableOneHandMovementUI";
        public static MelonPreferences_Entry<bool> disableMenuOnLoad;
        public static MelonPreferences_Entry<bool> debug;
        private static GameObject AM;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory(catagory, "DisableOneHandMovementUI Settings");
            disableMenuOnLoad = MelonPreferences.CreateEntry(catagory, nameof(disableMenuOnLoad), true, "Disable on Load");
            debug = MelonPreferences.CreateEntry(catagory, nameof(debug), false, "Debug"); //Because for some reason this wasn't working for someone! 


            var menu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("OneHanded UI", () => menu.Show());

            menu.AddLabel("Enable/Disable the One Handed Controller Movement UI");
            menu.AddSimpleButton("Disable", (() => Toggle(false)));
            menu.AddSimpleButton("Enable", (() => Toggle(true)));
            menu.AddSimpleButton("Close", () => menu.Hide());

            MelonCoroutines.Start(OnLoad());
        }

        public static IEnumerator OnLoad()
        {
            if (debug.Value) MelonLogger.Msg("OnLoad Started");
            while (GameObject.Find("UserInterface/ActionMenu") == null)
                yield return new WaitForSeconds(1f);
            AM = GameObject.Find("UserInterface/ActionMenu/Container");
            if (debug.Value) MelonLogger.Msg("UI/AM Found");
            if (disableMenuOnLoad.Value) Toggle(false);
        }

        private static void Toggle(bool value)
        {
            float scale = value ? 1f : 0.0001f;
            if (debug.Value) MelonLogger.Msg("Toggle: " + scale);
            if (AM is null) MelonLogger.Error("AM is null, report this bug" + scale);
            for (int i = 0; i < AM.transform.childCount; i++)
            {
                GameObject child = AM.transform.GetChild(i).gameObject;
                if (debug.Value) MelonLogger.Msg("Child: " + child.name);
                if (child.name.StartsWith("MoveMenu"))
                {
                    if (debug.Value) MelonLogger.Msg("-MoveMenu-");
                    for (int i1 = 0; i1 < child.transform.childCount; i1++)
                    {
                        GameObject child1 = child.transform.GetChild(i1).gameObject;
                        if (debug.Value) MelonLogger.Msg("Child1: " + child1.name);
                        if (child1.name.StartsWith("OneHandMoveMenu"))
                        {
                            //MelonLogger.Msg("-OneHandMoveMenu_Simple-");
                            MelonLogger.Msg($"{child.name}/{child1.name} childern set to: " + scale);
                            for (int i2 = 0; i2 < child1.transform.childCount; i2++)
                            {
                                GameObject child2 = child1.transform.GetChild(i2).gameObject;
                                if (debug.Value) MelonLogger.Msg("Child2: " + child2.name);
                                child2.transform.localScale = new Vector3(scale, scale, scale);
                                if (debug.Value) MelonLogger.Msg($"Scale - X:{child2.transform.localScale.x}, Y:{child2.transform.localScale.y}, Z:{child2.transform.localScale.z}");
                            }
                        }
                    }
                }
            }
        }
    }
}

