using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using System.Collections.Generic;
using VRCSDK2;
using System;

[assembly: MelonInfo(typeof(LocalCube.Main), "LocalCube", "0.3.5", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace LocalCube
{
    public class Main : MelonMod
    {

        MelonPreferences_Category category = MelonPreferences.CreateCategory("LocalCubeMod", "Local Cube");
        private bool cubeRepawn, cubeQMButt, cubeBigButt;
        private Transform cubeQMbtn, cubeBigbtn;
        private bool useStandard;
        private float colorR, colorG, colorB;
        private float customCubeHeight, customCubeWidth, customCubeLength, cubeHeightOffset;
        private static GameObject localCube, wallFront, wallBack, wallRight, wallLeft, wallTop, wallBottom;
        private int cubeState = 0;
        private bool wallFront_Active, wallBack_Active, wallRight_Active, wallLeft_Active, wallTop_Active, wallBottom_Active;
        private bool wallsPickupable, pickupOrient, destroyCol;
        Material mat;

        public override void OnApplicationStart()
        {
            category.CreateEntry<bool>("CubeQMButt", false, "Cube Button on QuickMenu");
            category.CreateEntry<bool>("CubeBigButt", true, "Cube Button on World Big Menu");
            category.CreateEntry<bool>("CubeRepawn", false, "Cube Respawns on world join");

            category.CreateEntry<float>("CubeColorR", 0, "Color: Red (0-100)");
            category.CreateEntry<float>("CubeColorG", 0, "Color: Green (0-100)");
            category.CreateEntry<float>("CubeColorB", 0, "Color: Blue (0-100)");

            category.CreateEntry<float>("CustomCubeHeight", 3f, "Custom Cube Height");
            category.CreateEntry<float>("customCubeWidth", 2f, "Custom Cube Width");
            category.CreateEntry<float>("customCubeLength", 4f, "Custom Cube Length");

            category.CreateEntry<float>("CubeHeightOffset", 10f, "Cube Height Offset (0-100) 0=None, 100=-1/2 Height");
            category.CreateEntry<bool>("UseStandard", false, "Use Standard Shader vs Unlit Color");
            category.CreateEntry<bool>("WallsPickupable", false, "Can Pickup Walls");
            category.CreateEntry<bool>("PickupOrient", false, "Pickups snap to hand");
            category.CreateEntry<bool>("DestroyCol", false, "Destroy Collidors on box", "", true);

            OnPreferencesSaved();

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Local Cube", () => { CubeMenu(false); },
                (button) => { cubeQMbtn = button.transform; button.gameObject.SetActive(cubeQMButt); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.WorldMenu).AddSimpleButton("Local Cube", () => { CubeMenu(true); },
                (button) => { cubeBigbtn = button.transform; button.gameObject.SetActive(cubeBigButt); });
        }

        public override void OnPreferencesSaved()
        {
            cubeRepawn = category.GetEntry<bool>("CubeRepawn").Value;
            cubeQMButt = category.GetEntry<bool>("CubeQMButt").Value;
            cubeBigButt = category.GetEntry<bool>("CubeBigButt").Value;

            useStandard = category.GetEntry<bool>("UseStandard").Value;
            colorR = Clamp(category.GetEntry<float>("CubeColorR").Value/100, 0, 1);
            colorG = Clamp(category.GetEntry<float>("CubeColorG").Value/100, 0, 1);
            colorB = Clamp(category.GetEntry<float>("CubeColorB").Value/100, 0, 1);

            customCubeHeight = category.GetEntry<float>("CustomCubeHeight").Value;
            customCubeWidth = category.GetEntry<float>("customCubeWidth").Value;
            customCubeLength = category.GetEntry<float>("customCubeLength").Value;
            cubeHeightOffset = Clamp(category.GetEntry<float>("CubeHeightOffset").Value, 0, 1);
            wallsPickupable = category.GetEntry<bool>("WallsPickupable").Value;
            pickupOrient = category.GetEntry<bool>("PickupOrient").Value;
            destroyCol = category.GetEntry<bool>("DestroyCol").Value;

            if (cubeQMbtn != null) cubeQMbtn.gameObject.SetActive(cubeQMButt);
            if (cubeBigbtn != null) cubeBigbtn.gameObject.SetActive(cubeBigButt);

            if (localCube != null) {
                for (int i = 0; i < localCube.transform.childCount; ++i)
                {
                    GameObject _wall = localCube.transform.GetChild(i).gameObject;
                    try
                    {
                        //MelonLogger.Msg($"re: {_wall.gameObject.name}");
                        if (_wall.gameObject != null && !_wall.gameObject.Equals(null))
                        {
                            _wall.gameObject.GetOrAddComponent<VRC_Pickup>().pickupable = wallsPickupable;
                            _wall.gameObject.GetOrAddComponent<VRC_Pickup>().orientation = pickupOrient ? VRC_Pickup.PickupOrientation.Any : VRC_Pickup.PickupOrientation.Grip;
                            _wall.gameObject.GetOrAddComponent<MeshRenderer>().material.color = new Color(colorR, colorG, colorB);
                        }
                    }
                    catch (Exception ex) { MelonLogger.Error("Catch-Apply Settings" + ex.ToString()); }
                }
            }

        }

        public int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonLogger.Msg($"re: {cubeRepawn} state: {cubeState}");
            if (!cubeRepawn || cubeState == 0) return;
            //MelonLogger.Msg($"index: {buildIndex} sceneName: {sceneName}");
            switch (buildIndex)//Without switch this would run 3 times at world load
            {
                case 0: break;//App
                case 1: break;//ui
                case 2: break;//empty
                default:
                    MelonLogger.Msg("Cube Respawn");
                    MelonCoroutines.Start(RespawnCube());
                    break;
            }
        }
        System.Collections.IEnumerator RespawnCube()
        {
            while (VRCPlayer.field_Internal_Static_VRCPlayer_0 == null) yield return null;
            yield return new WaitForSecondsRealtime(1);//Make wait for local player
            switch (cubeState)
            {
                case 1: CreateCube(5, 5, 5, .1f); break;
                case 2: CreateCube(15, 15, 15, .05f); break;
                case 3: CreateCube(30, 30, 30, .05f); break;
                case 10: CreateCube(customCubeHeight, customCubeWidth, customCubeLength, cubeHeightOffset); break;
                default: MelonLogger.Msg($"RespawnCube hit default case - Something is wrong"); break;
            }
        }

        public void CubeMenu(bool useBigMenu)
        {

            ICustomShowableLayoutedMenu cubeMenu = null;
            cubeMenu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescriptionCustom.QuickMenu3Column4Row) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column4Row);
            cubeMenu.AddSimpleButton($"Destroy Cube", () => { DestroyCube(); cubeState = 0; });
            cubeMenu.AddSimpleButton($"Create Custom Cube", () => { CreateCube(customCubeHeight, customCubeWidth, customCubeLength, cubeHeightOffset); cubeState = 10; });
            cubeMenu.AddSimpleButton($"Settings", () => { SettMenu(useBigMenu); });
            ///
            cubeMenu.AddSimpleButton($"Create Small Cube", () => { CreateCube(5, 5, 5, .1f); cubeState = 1; }); 
            cubeMenu.AddSimpleButton($"Create Medium Cube", () => { CreateCube(15, 15, 15, .05f); cubeState = 2; }); 
            cubeMenu.AddSimpleButton($"Create Large Cube", () => { CreateCube(30, 30, 30, .05f); cubeState = 3; }); 
            /////
            cubeMenu.AddSimpleButton($"Wall Visibility Menu", () => { WallMenu(useBigMenu); });
            cubeMenu.AddSimpleButton($"Color Menu", () => { ColorMenu(useBigMenu); });
            cubeMenu.AddSimpleButton($"Color Menu Adjust", () => { ColorMenuAdj(useBigMenu); });
            /////
            cubeMenu.AddSimpleButton($"--Exit--", () => { cubeMenu.Hide(); });
            cubeMenu.AddToggleButton("Pickupable", (action) =>
            {
                category.GetEntry<bool>("WallsPickupable").Value = !category.GetEntry<bool>("WallsPickupable").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("WallsPickupable").Value);
            cubeMenu.AddToggleButton("Pickups snap to hand", (action) =>
            {
                category.GetEntry<bool>("PickupOrient").Value = !category.GetEntry<bool>("PickupOrient").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("PickupOrient").Value);
            //cubeMenu.AddSpacer();

            cubeMenu.Show();
        }

        public void SettMenu(bool useBigMenu)
        {
            ICustomShowableLayoutedMenu Menu = null;
            Menu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);

            Menu.AddToggleButton("Cube Button on QuickMenu", (action) =>
            {
                category.GetEntry<bool>("CubeQMButt").Value = !category.GetEntry<bool>("CubeQMButt").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("CubeQMButt").Value);
            Menu.AddToggleButton("Cube Button on World Big Menu", (action) =>
            {
                category.GetEntry<bool>("CubeBigButt").Value = !category.GetEntry<bool>("CubeBigButt").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("CubeBigButt").Value);
            Menu.AddSpacer();
            ///
            Menu.AddSimpleButton($"-", () => {
                category.GetEntry<float>($"CubeHeightOffset").Value = Clamp(category.GetEntry<float>($"CubeHeightOffset").Value - 10, 0, 100);
                OnPreferencesSaved(); Menu.Hide(); SettMenu(useBigMenu);
            });
            Menu.AddSimpleButton($"Cube Height Offset:{(category.GetEntry<float>("CubeHeightOffset").Value)}\n0=None, 100=-1/2 Height", () => { });
            Menu.AddSimpleButton($"+", () => {
                category.GetEntry<float>($"CubeHeightOffset").Value = Clamp(category.GetEntry<float>($"CubeHeightOffset").Value + 10, 0, 100);
                OnPreferencesSaved(); Menu.Hide(); SettMenu(useBigMenu);
            });
            ///
            Menu.AddSimpleButton("Settings Menu\n----\nBack", () => { CubeMenu(useBigMenu); });
            Menu.AddToggleButton("Cube Respawns on world join", (action) =>
            {
                category.GetEntry<bool>("CubeRepawn").Value = !category.GetEntry<bool>("CubeRepawn").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("CubeRepawn").Value);
            Menu.AddToggleButton("Use Standard Shader vs Unlit Color", (action) =>
            {
                category.GetEntry<bool>("UseStandard").Value = !category.GetEntry<bool>("UseStandard").Value;
                OnPreferencesSaved();
            }, () => category.GetEntry<bool>("UseStandard").Value);


            Menu.Show();
        }

        public void WallMenu(bool useBigMenu)
        {
            ICustomShowableLayoutedMenu wallMenu = null;
            wallMenu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.QuickMenu3Columns) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu3Columns);
            
            wallMenu.AddSimpleButton($"Toggle\nTop", () => { wallTop.gameObject.SetActive(!wallTop.gameObject.active); });
            wallMenu.AddSimpleButton($"Toggle\nFront", () => { wallFront.gameObject.SetActive(!wallFront.gameObject.active); });
            wallMenu.AddSimpleButton($"Toggle\nBottom", () => { wallBottom.gameObject.SetActive(!wallBottom.gameObject.active); });

            //
            wallMenu.AddSimpleButton($"Toggle\nLeft", () => { wallLeft.gameObject.SetActive(!wallLeft.gameObject.active); });
            wallMenu.AddSimpleButton($"Toggle\nBack", () => { wallBack.gameObject.SetActive(!wallBack.gameObject.active); });
            wallMenu.AddSimpleButton($"Toggle\nRight", () => { wallRight.gameObject.SetActive(!wallRight.gameObject.active); });
            //
            wallMenu.AddSimpleButton("Wall Visibility Menu\n----\nBack", () => { CubeMenu(useBigMenu); });

            wallMenu.AddSimpleButton($"All On", () =>
            {
                if (localCube != null)
                    for (int i = 0; i < localCube.transform.childCount; ++i)
                        if (localCube.transform.GetChild(i).gameObject != null && !localCube.transform.GetChild(i).gameObject.Equals(null)) localCube.transform.GetChild(i).gameObject.SetActive(true);
            });
            wallMenu.AddSimpleButton($"All Off", () => 
            {
                if (localCube != null)
                    for (int i = 0; i < localCube.transform.childCount; ++i)
                        if (localCube.transform.GetChild(i).gameObject != null && !localCube.transform.GetChild(i).gameObject.Equals(null)) localCube.transform.GetChild(i).gameObject.SetActive(false);
            });

            wallMenu.Show();
        }

        public void ColorMenu(bool useBigMenu)
        {
            var colorList = new Dictionary<string, Color> { 
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
            Menu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescriptionCustom.QuickMenu3Column4Row) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column4Row);

            foreach (KeyValuePair<string, Color> entry in colorList)
            {
                if (entry.Key == "x") { Menu.AddSpacer(); continue; }//If desc is x, then skip
                Menu.AddSimpleButton(entry.Key, () => {
                    category.GetEntry<float>("CubeColorR").Value = entry.Value.r * 100;
                    category.GetEntry<float>("CubeColorG").Value = entry.Value.g * 100;
                    category.GetEntry<float>("CubeColorB").Value = entry.Value.b * 100;
                    OnPreferencesSaved();
                });
            }
            //
            Menu.AddSimpleButton("Color Menu\n----\nBack", () => { CubeMenu(useBigMenu); });
            Menu.Show();
        }

        public void ColorMenuAdj(bool useBigMenu)
        {
            ICustomShowableLayoutedMenu Menu = null;
            Menu = useBigMenu ? ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescriptionCustom.QuickMenu3Column4Row) : ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column4Row);

            string[] colorList = { "Red", "Green", "Blue" };
            for (int i = 0; i < colorList.GetLength(0); i++)
            {
                string c = colorList[i].Substring(0, 1);
                Menu.AddSimpleButton($"{(colorList[i])} -", () => {
                    category.GetEntry<float>($"CubeColor{c}").Value = Clamp(category.GetEntry<float>($"CubeColor{c}").Value - 10, 0, 100);
                    OnPreferencesSaved();
                    Menu.Hide(); ColorMenuAdj(useBigMenu);
                });
                Menu.AddSimpleButton($"{(colorList[i])} -0-", () => {
                    category.GetEntry<float>($"CubeColor{c}").Value = 0f;
                    OnPreferencesSaved();
                    Menu.Hide(); ColorMenuAdj(useBigMenu);
                });
                Menu.AddSimpleButton($"{(colorList[i])} +", () => {
                    category.GetEntry<float>($"CubeColor{c}").Value = Clamp(category.GetEntry<float>($"CubeColor{c}").Value + 10, 0, 100);
                    OnPreferencesSaved();
                    Menu.Hide(); ColorMenuAdj(useBigMenu);
                });
            }

            Menu.AddSimpleButton("Color Adj\n----\nBack", () => { CubeMenu(useBigMenu); });
            Menu.AddLabel($"R:{category.GetEntry<float>("CubeColorR").Value}\nG:{category.GetEntry<float>("CubeColorG").Value}\nB:{category.GetEntry<float>("CubeColorB").Value}");

            Menu.Show();
        }


        private void CreateCube(float H, float W, float L, float Offset)
        {
            if (wallFront != null) { UnityEngine.Object.Destroy(wallFront); wallFront = null; }
            if (wallBack != null) { UnityEngine.Object.Destroy(wallBack); wallBack = null; }
            if (wallRight != null) { UnityEngine.Object.Destroy(wallRight); wallRight = null;}
            if (wallLeft != null) { UnityEngine.Object.Destroy(wallLeft); wallLeft = null;}
            if (wallTop != null) { UnityEngine.Object.Destroy(wallTop); wallTop = null;}
            if (wallBottom != null) { UnityEngine.Object.Destroy(wallBottom); wallBottom = null;}

            if (localCube == null) localCube = new GameObject("LocalCube");

            VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            if(useStandard){ mat =  new Material(Shader.Find("Standard")); mat.SetFloat("_Glossiness", 0f); }
            else mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = new Color(colorR, colorG, colorB);

            ToggleWall(ref wallFront, player, W, H, L/2, 0, H/2*(1-Offset), 0, 0, "Local-WallFront", ref mat); wallFront_Active = true;
            ToggleWall(ref wallBack, player, W, H, -L/2, 0, H/2*(1-Offset), 0, 0, "Local-WallBack", ref mat); wallBack_Active = true;
            ToggleWall(ref wallRight, player, L, H, 0, W/2, H/2*(1-Offset), 0, 90, "Local-WallRight", ref mat); wallRight_Active = true;
            ToggleWall(ref wallLeft, player, L, H, 0, -W/2, H/2*(1-Offset), 0, 90, "Local-WallLeft", ref mat); wallLeft_Active = true;
            ToggleWall(ref wallTop, player, W, L, 0, 0, H/2+H/2*(1-Offset), 90, 0, "Local-WallTop", ref mat); wallTop_Active = true;
            ToggleWall(ref wallBottom, player, W, L, 0, 0, (-H/2)+H/2*(1-Offset), 90, 0, "Local-WallBottom", ref mat); wallBottom_Active = true;

        }

        private void ToggleWall(ref GameObject wall, VRCPlayer player, float width, float Height, float forward, float right, float up, int rotLeft, int rotUp, string name, ref Material mat)
        {
            if (wall != null) { UnityEngine.Object.Destroy(wall); wall = null; }
            else {
                GameObject _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _wall.transform.SetParent(localCube.transform, true);
                _wall.transform.position = player.transform.position + player.transform.forward * forward;
                _wall.transform.position = _wall.transform.position + player.transform.right * right;
                _wall.transform.position = _wall.transform.position + player.transform.up * up;
                _wall.transform.rotation = player.transform.rotation * Quaternion.AngleAxis(rotLeft, Vector3.left); //Rotate around left, 90 will make ceiling
                _wall.transform.rotation = _wall.transform.rotation * Quaternion.AngleAxis(rotUp, Vector3.up); //Rotate around up, 90 makes side wall
                _wall.name = name;
                if(destroyCol) UnityEngine.Object.Destroy(_wall.GetComponent<Collider>());
                _wall.transform.localScale = new Vector3((width), (Height), .05f);
                
                _wall.GetOrAddComponent<MeshRenderer>().material = mat;
                //MelonLogger.Msg($"PICK {wallsPickupable}");
                _wall.GetOrAddComponent<BoxCollider>().size = new Vector3((width), (Height), .05f);
                _wall.GetOrAddComponent<BoxCollider>().isTrigger = true;
                _wall.GetOrAddComponent<MeshRenderer>().enabled = false;
                _wall.GetOrAddComponent<VRC_Pickup>().proximity = 3f;
                _wall.GetOrAddComponent<VRC_Pickup>().pickupable = wallsPickupable;
                _wall.GetOrAddComponent<VRC_Pickup>().orientation = pickupOrient ? VRC_Pickup.PickupOrientation.Any : VRC_Pickup.PickupOrientation.Grip;
                _wall.GetOrAddComponent<VRC_Pickup>().allowManipulationWhenEquipped = true;
                _wall.GetOrAddComponent<Rigidbody>().useGravity = false;
                _wall.GetOrAddComponent<Rigidbody>().isKinematic = true;
                _wall.GetOrAddComponent<MeshRenderer>().enabled = true;
                wall = _wall;
            }
        }

        private void DestroyCube()
        {
            if (wallFront != null) { UnityEngine.Object.Destroy(wallFront); wallFront = null; wallFront_Active = false; } 
            if (wallBack != null) { UnityEngine.Object.Destroy(wallBack); wallBack = null; wallBack_Active = false; }
            if (wallRight != null) { UnityEngine.Object.Destroy(wallRight); wallRight = null; wallRight_Active = false; }
            if (wallLeft != null) { UnityEngine.Object.Destroy(wallLeft); wallLeft = null; wallLeft_Active = false; }
            if (wallTop != null) { UnityEngine.Object.Destroy(wallTop); wallTop = null; wallTop_Active = false; }
            if (wallBottom != null) { UnityEngine.Object.Destroy(wallBottom); wallBottom = null; wallBottom_Active = false; }
            cubeState = 0;
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
        public static LayoutDescription QuickMenu3Column = new LayoutDescription { NumColumns = 3, RowHeight = 420 / 11, NumRows = 11 }; //8 was 380
        public static LayoutDescription QuickMenu3Column4Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 4, NumRows = 4 }; 
        public static LayoutDescription QuickMenu3Column_Longer = new LayoutDescription { NumColumns = 3, RowHeight = 475 / 14, NumRows = 14 };
    }
}

public static class Utils
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            return gameObject.AddComponent<T>();
        }
        return component;
    }
}