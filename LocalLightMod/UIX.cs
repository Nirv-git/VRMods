using System;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRCSDK2;

namespace LocalLightMod
{
    class UIX
    {
        static Transform butt = null;

        public static void SetupUI()
        { 
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Light Menu", () => LightTypeMenu());
        }

        private static void LightTypeMenu()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);

            menu.AddLabel($"Local Light Menu");
            menu.AddSimpleButton($"Light Config", () =>
            {
                LightDetailsMenu();
            });
            menu.AddSimpleButton($"Create", () =>
            {
                Main.CreateLight();
            });
            menu.AddSimpleButton($"Update Active Light", () =>
            {
                Main.UpdateLight(Main.activeLight);
            });
            menu.AddSimpleButton($"Update Specific Light", () =>
            {
                SelectSpecific();
            });
            menu.AddSimpleButton($"Delete All Lights", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel("-Confirm-\nDelete All Lights");
                menu2.AddSimpleButton($"Yes", () =>
                {
                    Main.CleanupVisObjects();
                    LightTypeMenu();
                });
                menu2.AddSimpleButton($"No", () =>
                {
                    LightTypeMenu();
                });
                menu2.Show();
            });
            menu.AddSimpleButton($"Save/Load Light Options", () =>
            {
                SavedPrefSlots();
            });

            menu.AddSimpleButton($"Close", () =>
            {
                menu.Hide();
            });

            menu.Show();
        }


        private static void LightDetailsMenu()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu2ColumnWideSlim);

            menu.AddToggleButton("Pickup Snaps to Hand", (action) =>
            {
                Main.Config.pickupOrient = !Main.Config.pickupOrient;
            }, () => Main.Config.pickupOrient);

            menu.AddToggleButton("Pickupable", (action) =>
            {
                Main.Config.pickupable = !Main.Config.pickupable;
            }, () => Main.Config.pickupable);

            menu.AddSimpleButton($"LightType:\n{Main.Config.lightType}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"LightType: {Main.Config.lightType}");
                menu2.AddSimpleButton($"Directional", () =>
                {
                    Main.Config.lightType = LightType.Directional;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"Spot", () =>
                {
                    Main.Config.lightType = LightType.Spot;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"Point", () =>
                {
                    Main.Config.lightType = LightType.Point;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Range(Spot|Point):\n{Main.Config.lightRange}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.Config.lightRange += 2f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}";
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.Config.lightRange += .5f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}";
                });
                menu2.AddSimpleButton($"10", () =>
                {
                    Main.Config.lightRange = 10f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}";
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.Config.lightRange -= .5f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}";
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.Config.lightRange -= 2f;
                    butt.GetComponentInChildren<Text>().text = $"Range(Spot|Point): {Utils.NumFormat(Main.Config.lightRange)}";
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Spot Angle: \n{Utils.NumFormat(Main.Config.lightSpotAngle)}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.Config.lightSpotAngle += 5f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}";
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.Config.lightSpotAngle += 2f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}";
                });
                menu2.AddSimpleButton($"30", () =>
                {
                    Main.Config.lightSpotAngle = 30f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}";
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.Config.lightSpotAngle -= 2f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}";
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.Config.lightSpotAngle -= 5f;
                    butt.GetComponentInChildren<Text>().text = $"Spot Angle: {Utils.NumFormat(Main.Config.lightSpotAngle)}";
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            //Color
            menu.AddSimpleButton($"Color (R,G,B):\n{Main.Config.lightColor.r}, {Main.Config.lightColor.g}, {Main.Config.lightColor.b}", () =>
            {
                ColorMenuAdj();
            });

            menu.AddSimpleButton($"Light Intensity:\n{Utils.NumFormat(Main.Config.lightIntensity)}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.Config.lightIntensity += .25f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}";
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.Config.lightIntensity += .1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}";
                });
                menu2.AddSimpleButton($"1", () =>
                {
                    Main.Config.lightIntensity = 1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}";
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.Config.lightIntensity -= .1f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}";
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.Config.lightIntensity -= .25f;
                    butt.GetComponentInChildren<Text>().text = $"Light Intensity: {Utils.NumFormat(Main.Config.lightIntensity)}";
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            //shadows
            menu.AddSimpleButton($"Shadows:\n{Main.Config.lightShadows}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Shadows: {Main.Config.lightShadows}");
                menu2.AddSimpleButton($"Hard", () =>
                {
                    Main.Config.lightShadows = LightShadows.Hard;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"Soft", () =>
                {
                    Main.Config.lightShadows = LightShadows.Soft;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"None", () =>
                {
                    Main.Config.lightShadows = LightShadows.None;
                    LightDetailsMenu();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Shadow Cust Res:\n{Main.Config.lightShadowCustRes}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Shadow Cust Res: {Main.Config.lightShadowCustRes}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.Config.lightShadowCustRes = Main.Config.lightShadowCustRes < 8192 ? Main.Config.lightShadowCustRes * 2 : Main.Config.lightShadowCustRes;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Cust Res: {Main.Config.lightShadowCustRes}";
                });
                menu2.AddSimpleButton($"2048", () =>
                {
                    Main.Config.lightShadowCustRes = 2048;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Cust Res: {Main.Config.lightShadowCustRes}";
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.Config.lightShadowCustRes = Main.Config.lightShadowCustRes > 2 ? Main.Config.lightShadowCustRes / 2 : Main.Config.lightShadowCustRes;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Cust Res: {Main.Config.lightShadowCustRes}";
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Shadow Str:\n{Utils.NumFormat(Main.Config.lightShadowStr)}", () =>
            {
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel($"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}", (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    Main.Config.lightShadowStr += .2f;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}";
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    Main.Config.lightShadowStr += .1f;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}";
                });
                menu2.AddSimpleButton($"1", () =>
                {
                    Main.Config.lightShadowStr = 1f;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}";
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    Main.Config.lightShadowStr -= .1f;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}";
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    Main.Config.lightShadowStr -= .2f;
                    butt.GetComponentInChildren<Text>().text = $"Shadow Str: {Utils.NumFormat(Main.Config.lightShadowStr)}";
                }); 
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    LightDetailsMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Name:\n{Main.Config.name}", () =>
            {
                SetName(true);
            });
            menu.AddToggleButton("Hide Mesh", (action) =>
            {
                Main.Config.hideMeshRender = !Main.Config.hideMeshRender;
            }, () => Main.Config.hideMeshRender);

            //create 
            menu.AddSimpleButton($"-Create-", () =>
            {
                Main.CreateLight();
            });
            menu.AddSimpleButton($"-Update Active Light-", () =>
            {
                Main.UpdateLight(Main.activeLight);
            });
            menu.AddSimpleButton($"<-Back", () =>
            {
                LightTypeMenu();
            });
            menu.AddSimpleButton($"-Reset to-\n-Defaults-", () =>
            {
                Main.Config.name = "DefaultName";
                Main.Config.pickupOrient = false;
                Main.Config.pickupable = true;
                Main.Config.lightType = LightType.Point;
                Main.Config.lightRange = 10;
                Main.Config.lightSpotAngle = 30;
                Main.Config.lightColor = Color.white;
                Main.Config.lightIntensity = 1;
                Main.Config.lightBounceIntensity = 1;
                Main.Config.lightShadows = LightShadows.None;
                Main.Config.lightShadowStr = 1;
                Main.Config.lightShadowCustRes = 2048;
                LightDetailsMenu();
            });
            menu.Show();
        }


        public static void SetName(bool cas)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz-_ ".ToCharArray();
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column10Row);

            menu.AddSimpleButton("<-Back", () => { LightDetailsMenu(); });
            menu.AddLabel(Main.Config.name, (button) => butt = button.transform); ;
            menu.AddSimpleButton("BackSpace", () => {
                if(Main.Config.name.Length > 0) Main.Config.name = Main.Config.name.Remove(Main.Config.name.Length - 1, 1);
                butt.GetComponentInChildren<Text>().text = Main.Config.name;
            });

            foreach (char c in chars)
            {
                var s = cas ? c.ToString().ToUpper() : c.ToString();
                menu.AddSimpleButton(s, () => {
                    Main.Config.name += s;
                    butt.GetComponentInChildren<Text>().text = Main.Config.name;
                });
            }

            menu.AddSimpleButton("Toggle Case", () => { SetName(!cas); });
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
            Menu.AddSimpleButton($"-Update Active Light-", () =>
            {
                Main.UpdateLight(Main.activeLight);
            });

            Menu.AddSimpleButton($"All -", () => {
                colorList["Red"] = Utils.Clamp(colorList["Red"] - 10, 0, 100);
                colorList["Green"] = Utils.Clamp(colorList["Green"] - 10, 0, 100);
                colorList["Blue"] = Utils.Clamp(colorList["Blue"] - 10, 0, 100);
                Menu.Hide(); ColorMenuAdj();
            });
            Menu.AddSimpleButton($"All +", () => {
                colorList["Red"] = Utils.Clamp(colorList["Red"] + 10, 0, 100);
                colorList["Green"] = Utils.Clamp(colorList["Green"] + 10, 0, 100);
                colorList["Blue"] = Utils.Clamp(colorList["Blue"] + 10, 0, 100);
                Menu.Hide(); ColorMenuAdj();
            });
            Menu.AddSimpleButton($"<-Back\nR:{colorList["Red"]}\nG:{colorList["Green"]}\nB:{colorList["Blue"]}", () => { LightDetailsMenu(); });
            Menu.AddSimpleButton("Color Shortcuts", () => { ColorMenu(); });
            Menu.AddSimpleButton("Save/Load Colors", () => { StoredColorsMenu(); });

            Main.Config.lightColor = new Color(Utils.Clamp(colorList["Red"] / 100, 0, 1), Utils.Clamp(colorList["Green"] / 100, 0, 1), Utils.Clamp(colorList["Blue"] / 100, 0, 1));
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
        private static void SelectSpecific()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu5ColumnWide);

            float i = 0;
            foreach (var obj in Main.lightList) 
            {
                if (!obj?.Equals(null) ?? false)
                {
                    i++;
                    menu.AddSimpleButton("-Set as Active-\n" + Main.LightDetailsString(obj), () => Main.activeLight = obj);//$"{obj.name}");
                    menu.AddSimpleButton($"Update with Current Settings", () =>
                    {
                        Main.UpdateLight(obj);
                        SelectSpecific();//LAZY
                    });
                    menu.AddSimpleButton($"Load Light's Settings", () =>
                    {
                        Main.LoadLightSettings(obj);
                        SelectSpecific();
                    });
                    menu.AddToggleButton("Pickupable", (action) =>
                    {
                        obj.GetOrAddComponent<VRC_Pickup>().pickupable = !obj.GetOrAddComponent<VRC_Pickup>().pickupable;
                    }, () => obj.GetOrAddComponent<VRC_Pickup>().pickupable);
                    menu.AddSimpleButton($"Delete", () =>
                    {
                        Main.CleanupOneObject(obj);
                        SelectSpecific();
                    });

                }
            }
            menu.AddSimpleButton("<-Back", (() =>
            {
                LightTypeMenu();
            }));
            menu.AddLabel(CurrentConfig());

            menu.Show();

        }

        private static void SavedPrefSlots()
        {//pickupOrient;pickupable;lightType;lightRange;lightSpotAngle;lightColor;lightIntensity;lightBounceIntensity;lightShadows;name;
            var storedMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu6Columns);
            foreach (KeyValuePair<int, (bool, bool, LightType, float, float, Color, float, float, LightShadows, float, string, bool)> slot in SaveSlots.GetSavedPrefs())
            {
                
                string label = $"Slot: {slot.Key}\n{slot.Value.Item11}\n{slot.Value.Item3}\nR:{slot.Value.Item6.r} G:{slot.Value.Item6.g} B:{slot.Value.Item6.b}\nInten:{slot.Value.Item7}\nHidden:{slot.Value.Item12}";//\nUp:{Utils.NumberFormat(slot.Value.Item1)}\nForward:{Utils.NumberFormat(slot.Value.Item2)}\nSide:{Utils.NumberFormat(slot.Value.Item3)}"
                storedMenu.AddLabel(label);
                storedMenu.AddSimpleButton($"Load", () =>
                {
                    Main.Config.name = slot.Value.Item11;
                    Main.Config.pickupOrient = slot.Value.Item1;
                    Main.Config.pickupable = slot.Value.Item2;
                    Main.Config.lightType = slot.Value.Item3;
                    Main.Config.lightRange = slot.Value.Item4;
                    Main.Config.lightSpotAngle = slot.Value.Item5;
                    Main.Config.lightColor = slot.Value.Item6;
                    Main.Config.lightIntensity = slot.Value.Item7;
                    Main.Config.lightBounceIntensity = slot.Value.Item8;
                    Main.Config.lightShadows = slot.Value.Item9;
                    Main.Config.lightShadowStr = slot.Value.Item10;
                    Main.Config.hideMeshRender = slot.Value.Item12;

                    SavedPrefSlots();
                });
                storedMenu.AddSimpleButton($"Save", () =>
                {
                    SaveSlots.StorePrefs(slot.Key, (Main.Config.pickupOrient, Main.Config.pickupable, Main.Config.lightType, Main.Config.lightRange, Main.Config.lightSpotAngle, Main.Config.lightColor, Main.Config.lightIntensity, Main.Config.lightBounceIntensity, Main.Config.lightShadows, Main.Config.lightShadowStr, Main.Config.name, Main.Config.hideMeshRender));
                    SavedPrefSlots();
                });

                if (slot.Key == 4)
                {
                    storedMenu.AddSimpleButton("<-Back", (() =>
                    {
                        LightTypeMenu();
                    }));
                    string current = CurrentConfig();
                    storedMenu.AddLabel(current);
                    storedMenu.AddSimpleButton($"-Update Active Light-", () =>
                    {
                        Main.UpdateLight(Main.activeLight);
                    });
                }
            }

            storedMenu.Show();
        }

        private static string CurrentConfig()
        {
            return $"Current:\n{Main.Config.name}\n{Main.Config.lightType}\nR:{Main.Config.lightColor.r} G:{Main.Config.lightColor.g} B:{Main.Config.lightColor.b}\nInten:{Utils.NumFormat(Main.Config.lightIntensity)}\nHidden:{Main.Config.hideMeshRender}"; // Up:{Utils.NumberFormat(melonPref1.Value)}\nForward:{Utils.NumberFormat(melonPref2.Value)}\nSide:{Utils.NumberFormat(melonPref3.Value)}";
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
        public static LayoutDescription QuickMenu6Columns = new LayoutDescription { NumColumns = 6, RowHeight = 380 / 3, NumRows = 3 };
        public static LayoutDescription QuickMenu1ColumnWideSlim = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu2ColumnWideSlim = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu5ColumnWide = new LayoutDescription { NumColumns = 5, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu3Column4Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu3Column5Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 5, NumRows = 5 };
        public static LayoutDescription QuickMenu3Column10Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 10, NumRows = 10 };

    }
}