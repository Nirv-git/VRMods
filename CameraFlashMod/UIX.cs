using System;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRCSDK2;

namespace CameraFlashMod
{
    class UIX
    {
        static Transform butt = null;
        public static GameObject UIXButt;

        public static void SetupUI()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu).AddSimpleButton("Camera Flash Settings", () =>
            {
                LightDetailsMenu();
            }, (button) => { UIXButt = button; UIXButt.SetActive(Main.UIXbuttEn.Value); });
        }

        private static void LightDetailsMenu()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu2ColumnWide);

            menu.AddSimpleButton($"LightType:\n{Main.lightType.Value}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"LightType: {Main.lightType.Value}");
                menu2.AddSimpleButton($"Spot", () =>
                {
                    Main.lightType.Value = LightType.Spot;
                    Main.UpdateLight();
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"Point", () =>
                {
                    Main.lightType.Value = LightType.Point;
                    Main.UpdateLight();
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"Directional", () =>
                {
                    Main.lightType.Value = LightType.Directional;
                    Main.UpdateLight();
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Range(Spot|Point):\n{Main.lightRange.Value}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.lightRange.Value += 2f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.lightRange.Value += .5f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"20", () =>
                {
                    Main.lightRange.Value = 20f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.lightRange.Value -= .5f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.lightRange.Value -= 2f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.lightRange.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Spot Angle: \n{Utils.NumFormat(Main.lightSpotAngle.Value)}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.lightSpotAngle.Value += 5f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.lightSpotAngle.Value += 2f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"120", () =>
                {
                    Main.lightSpotAngle.Value = 120f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.lightSpotAngle.Value -= 2f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.lightSpotAngle.Value -= 5f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.lightSpotAngle.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            //Color
            menu.AddSimpleButton($"Color (R,G,B):\n{Main.lightColor.Value.r}, {Main.lightColor.Value.g}, {Main.lightColor.Value.b}", () =>
            {
                ColorMenuAdj();
            });

            menu.AddSimpleButton($"Light Intensity:\n{Utils.NumFormat(Main.lightIntensity.Value)}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.lightIntensity.Value += .25f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.lightIntensity.Value += .1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"1", () =>
                {
                    Main.lightIntensity.Value = 1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.lightIntensity.Value -= .1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.lightIntensity.Value -= .25f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.lightIntensity.Value)}";
                    Main.UpdateLight();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddToggleButton("Enabled", (action) =>
            {
                Main.ToggleLight();
            }, () => (!Main.flash?.Equals(null) ?? false));

            menu.AddSimpleButton($"Close", () => menu.Hide());

            menu.AddSimpleButton($"-Reset to-\n-Defaults-", () =>
            {///Update all these 
                Main.lightType.Value = LightType.Spot;
                Main.lightRange.Value = 20;
                Main.lightSpotAngle.Value = 120;
                Main.lightColor.Value = Color.white;
                Main.lightIntensity.Value = 1;
                LightDetailsMenu();
                Main.UpdateLight();
            });
            menu.Show();
        }

        public static Dictionary<string, float> colorList = new Dictionary<string, float> {
                { "Red", 100 },
                { "Green", 100 },
                { "Blue", 100 },
            };
        public static void ColorMenuAdj()
        {
            ICustomShowableLayoutedMenu Menu = null;
            Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column5Row);

            foreach (KeyValuePair<string, float> entry in colorList)
            {
                string c = entry.Key;
                Menu.AddSimpleButton($"{(c)} -", () => {
                    colorList[c] = Utils.Clamp(entry.Value - 10, 0, 100);
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
                    colorList[c] = Utils.Clamp(colorList[c] + 10, 0, 100);
                    Menu.Hide(); ColorMenuAdj();
                });
            }

            Menu.AddSimpleButton($"All -", () => {
                colorList["Red"] = Utils.Clamp(colorList["Red"] - 10, 0, 100);
                colorList["Green"] = Utils.Clamp(colorList["Green"] - 10, 0, 100);
                colorList["Blue"] = Utils.Clamp(colorList["Blue"] - 10, 0, 100);
                Menu.Hide(); ColorMenuAdj();
            });
            Menu.AddSpacer();
            Menu.AddSimpleButton($"All +", () => {
                colorList["Red"] = Utils.Clamp(colorList["Red"] + 10, 0, 100);
                colorList["Green"] = Utils.Clamp(colorList["Green"] + 10, 0, 100);
                colorList["Blue"] = Utils.Clamp(colorList["Blue"] + 10, 0, 100);
                Menu.Hide(); ColorMenuAdj();
            });

            Menu.AddSimpleButton($"<-Back\nR:{colorList["Red"]}\nG:{colorList["Green"]}\nB:{colorList["Blue"]}", () => { LightDetailsMenu(); });
            Menu.AddSimpleButton("Color Shortcuts", () => { ColorMenu(); });
            Menu.AddSimpleButton("Save/Load Colors", () => { StoredColorsMenu(); });

            Main.lightColor.Value = new Color(Utils.Clamp(colorList["Red"] / 100, 0, 1), Utils.Clamp(colorList["Green"] / 100, 0, 1), Utils.Clamp(colorList["Blue"] / 100, 0, 1));
            Main.UpdateLight();
            Menu.Show();  
        }
        public static void ColorMenu()
        {
            var colorsList = new Dictionary<string, Color> {
                { "Black", Color.black },
                { "Grey", Color.grey },
                { "White", Color.white },//
                { "Red", Color.red },
                { "Green", Color.green },
                { "Blue", Color.blue },//
                { "Cyan", Color.cyan },
                { "Magenta", Color.magenta },
                { "Yellow", Color.yellow }//
            };

            ICustomShowableLayoutedMenu Menu = null;
            Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column4Row);

            foreach (KeyValuePair<string, Color> entry in colorsList)
            {
                if (entry.Key == "x") { Menu.AddSpacer(); continue; }//If desc is x, then skip
                Menu.AddSimpleButton(entry.Key, () => {
                    colorList["Red"] = entry.Value.r * 100;
                    colorList["Green"] = entry.Value.g * 100;
                    colorList["Blue"] = entry.Value.b * 100;
                    ColorMenuAdj();
                });
            }
            //
            Menu.AddSimpleButton("<-Back", () => { ColorMenuAdj(); });
            Menu.Show();
        }

        private static void StoredColorsMenu()
        {//type T|F - Pos|Rot

            var storedMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnsSlim);

            foreach (KeyValuePair<int, System.Tuple<float, float, float>> slot in SaveSlots.GetSavedColors())
            {
                string label =  $"Slot: {slot.Key}\nR:{Utils.NumFormat(slot.Value.Item1)}\nG:{Utils.NumFormat(slot.Value.Item2)}\nB:{Utils.NumFormat(slot.Value.Item3)}";
                storedMenu.AddLabel(label);
                storedMenu.AddSimpleButton($"Load", () =>
                {
                    colorList["Red"] = slot.Value.Item1;
                    colorList["Green"] = slot.Value.Item2;
                    colorList["Blue"] = slot.Value.Item3;
                    storedMenu.Hide();
                    ColorMenuAdj(); //Return to previous on load
                });
                storedMenu.AddSimpleButton($"Save", () =>
                {
                    SaveSlots.Store(slot.Key, new System.Tuple<float, float, float>(colorList["Red"], colorList["Green"], colorList["Blue"]));
                    storedMenu.Hide();
                    StoredColorsMenu();
                });
            }
            storedMenu.AddSimpleButton("<-Back", (() =>
            {
                ColorMenuAdj();
            }));
            string current = $"Current:\nR:{Utils.NumFormat(colorList["Red"])}\nG:{Utils.NumFormat(colorList["Green"])}\nB:{Utils.NumFormat(colorList["Blue"])}";
            storedMenu.AddLabel(current);

            storedMenu.Show();
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
        public static LayoutDescription QuickMenu3ColumnsSlim = new LayoutDescription { NumColumns = 6, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu1ColumnWideSlim = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu2ColumnWideSlim = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu2ColumnWide = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu3Column4Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu3Column5Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 5, NumRows = 5 };
    }
}