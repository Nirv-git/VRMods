using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using System.IO;
using ActionMenuApi.Api;

namespace FLuxMod
{
    class CustomActionMenu
    {
        public static Texture2D save, load, reset, gears, floppy, checkmark, Base, Bright, Colorize, Desat, HDR, Hue;
        public static Texture2D s1, s2, s3, s4, s5, s6, curr, x;

        public static void loadAssets()
        {
            save = LoadEmbeddedImages("save.png");
            load = LoadEmbeddedImages("load.png");
            reset = LoadEmbeddedImages("reset.png");
            gears = LoadEmbeddedImages("gears.png");
            floppy = LoadEmbeddedImages("floppy.png");
            checkmark = LoadEmbeddedImages("checkMark.png");
            Base = LoadEmbeddedImages("FLux-Base.png");
            Bright = LoadEmbeddedImages("FLux-Bright.png");
            Colorize = LoadEmbeddedImages("FLux-Colorize.png");
            Desat = LoadEmbeddedImages("FLux-Desat.png");
            HDR = LoadEmbeddedImages("FLux-HDR.png");
            Hue = LoadEmbeddedImages("FLux-Hue.png");
        }

        private static Texture2D LoadEmbeddedImages(string imageName)
        {
            try
            {
                //Load image into Texture
                using var assetStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("FLuxMod.Icons." + imageName);
                using var tempStream = new MemoryStream((int)assetStream.Length);
                assetStream.CopyTo(tempStream);
                var Texture2 = new Texture2D(2, 2);
                ImageConversion.LoadImage(Texture2, tempStream.ToArray());
                Texture2.wrapMode = TextureWrapMode.Clamp;
                Texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                Texture2.name = imageName.Replace(".png", "") + "-Tex";
                return Texture2;
            }
            catch (System.Exception ex) { Main.Logger.Error("Failed to load image from asset bundle: " + imageName + "\n" + ex.ToString()); return null; }
        }

        private static void InitTextures()
        {
            s1 = new Texture2D(2, 2);
            s1.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            s2 = new Texture2D(2, 2);
            s2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            s3 = new Texture2D(2, 2);
            s3.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            s4 = new Texture2D(2, 2);
            s4.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            s5 = new Texture2D(2, 2);
            s5.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            s6 = new Texture2D(2, 2);
            s6.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            curr = new Texture2D(2, 2);
            curr.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        public static void InitUi()
        {
            loadAssets();
            InitTextures();

            if (Main.amapi_ModsFolder.Value)
                AMUtils.AddToModsFolder("<color=#ff00ff>FLux</color>", () => AMsubMenu(), Base);
            else
                VRCActionMenuPage.AddSubMenu(ActionMenuPage.Options, "<color=#ff00ff>FLux</color>", () => AMsubMenu(), Base);
        }

        private static void AMsubMenu()
        {
            CustomSubMenu.AddToggle("FLux Enabled", !Main.fluxObj?.Equals(null) ?? false, (action) =>
            {
                Main.ToggleObject();
            }, null);
            CustomSubMenu.AddRadialPuppet("HDRClamp", f => Main.flux_HDRClamp.Value = f, Main.flux_HDRClamp.Value, HDR);
            CustomSubMenu.AddRadialPuppet("Hue", f => Main.flux_Hue.Value = f, Main.flux_Hue.Value, Hue);
            CustomSubMenu.AddRadialPuppet("Colorize", f => Main.flux_Colorize.Value = f, Main.flux_Colorize.Value, Colorize);
            CustomSubMenu.AddRadialPuppet("Brightness", f => Main.flux_Brightness.Value = f, Main.flux_Brightness.Value, Bright);
            CustomSubMenu.AddRadialPuppet("Desat", f => Main.flux_Desat.Value = f, Main.flux_Desat.Value, Desat);

            CustomSubMenu.AddSubMenu("Extras", () =>
            {
                CustomSubMenu.AddSubMenu($"\nReset", () =>
                {
                    CustomSubMenu.AddButton($"\n<size=30>Confirm Reset</size>", () =>
                    {
                        Main.pauseOnValueChange = true;
                        Main.flux_HDRClamp.Value = .222f;
                        Main.flux_Hue.Value = .102f;
                        Main.flux_Colorize.Value = .75f;
                        Main.flux_Brightness.Value = .623f;
                        Main.flux_Desat.Value = .255f;
                        Main.pauseOnValueChange = false;
                        Main.OnValueChange(0f, 0f);
                    }, checkmark);
                }, reset);

                StoredMenu();

                CustomSubMenu.AddButton($"*Rotary puppets max out at 97%\n*Slot names can be changed in Mod Settings", () => { }, null);
                CustomSubMenu.AddButton($"Uses:\nVRChat Flux Bloom Removal Shader\nrollthered.\nbooth.pm", () => { }, null);
            }, gears);
        }

        private static void StoredMenu()
        {
            CustomSubMenu.AddSubMenu("\nPresets", () =>
            {
                var savedInfo = SaveSlots.GetSaved();
                GenTextures(savedInfo);
                var list = new MelonPreferences_Entry<string>[] {
                Main.slot1Name,//To make index 1 referenced
                Main.slot1Name,
                Main.slot2Name,
                Main.slot3Name,
                Main.slot4Name,
                Main.slot5Name,
                Main.slot6Name,
                };

                foreach (KeyValuePair<int, System.Tuple<float, float, float, float, float>> slot in savedInfo)
                {

                    CustomSubMenu.AddSubMenu($"\n{slot.Key}-{list[slot.Key].Value}", () => 
                    {
                        CustomSubMenu.AddSubMenu("<size=40>Save</size>", () =>
                        {
                            CustomSubMenu.AddButton($"\n<size=30>Confirm Save</size>", () =>
                            {
                                SaveSlots.Store(slot.Key);
                                GenTextures(SaveSlots.GetSaved());
                                AMUtils.RefreshActionMenu();
                            }, checkmark);
                        }, save); ;
                        CustomSubMenu.AddButton($"\n\n<size=25>{slot.Key} {list[slot.Key].Value}</size>", () =>
                        {
                        }, StoredIcon(slot.Key));
                        CustomSubMenu.AddButton($"\n<size=40>Load</size>", () =>
                        {
                            SaveSlots.LoadSlot(slot.Key);
                        }, load);
                    }, StoredIcon(slot.Key));
                }
                CustomSubMenu.AddButton("\n" + "Current" + "", () =>
                {
                }, curr);
            }, floppy);
        }

        static ref Texture2D StoredIcon(int key)
        {
            switch (key)
            {
                case 1: return ref s1;
                case 2: return ref s2;
                case 3: return ref s3;
                case 4: return ref s4;
                case 5: return ref s5;
                case 6: return ref s6;
                default: Main.Logger.Msg("Something Broke - StoredIcon Switch"); return ref x;
            }
        }
        private static void GenTextures(Dictionary<int, System.Tuple<float, float, float, float, float>> savedInfo)
        {
            foreach (KeyValuePair<int, System.Tuple<float, float, float, float, float>> slot in savedInfo)
            {
                string label = $"HDR:{Utils.NumberFormat(slot.Value.Item1)}\nHue:{Utils.NumberFormat(slot.Value.Item2)}" +
                    $"\nColor:{Utils.NumberFormat(slot.Value.Item3)}\nBright:{Utils.NumberFormat(slot.Value.Item4)}" +
                    $"\nDesat:{Utils.NumberFormat(slot.Value.Item5)}";
                ImageConversion.LoadImage(StoredIcon(slot.Key), ImageGen.ImageToPNG(ImageGen.DrawText(label)));
            }
            ImageConversion.LoadImage(curr, ImageGen.ImageToPNG(ImageGen.DrawText($"HDR:{Utils.NumberFormat(Main.flux_HDRClamp.Value)}\nHue:{Utils.NumberFormat(Main.flux_Hue.Value)}" +
                    $"\nColor:{Utils.NumberFormat(Main.flux_Colorize.Value)}\nBright:{Utils.NumberFormat(Main.flux_Brightness.Value)}" +
                    $"\nDesat:{Utils.NumberFormat(Main.flux_Desat.Value)}")));
        }
    }

}
