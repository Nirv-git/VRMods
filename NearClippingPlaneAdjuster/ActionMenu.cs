using UnityEngine;
using System.IO;
using ActionMenuApi.Api;
using System.Collections.Generic;

namespace NearClipPlaneAdj
{
    public class CustomActionMenu
    {
        public static Texture2D n05, n01, n001, n0001, plane;

        public static void loadAssets()
        {
            n05 = LoadEmbeddedImages("n05.png");
            n01 = LoadEmbeddedImages("n01.png");
            n001 = LoadEmbeddedImages("n001.png");
            n0001 = LoadEmbeddedImages("n0001.png");
            plane = LoadEmbeddedImages("plane.png");
        }

        private static Texture2D LoadEmbeddedImages(string imageName)
        {
            try
            {
                //Load image into Texture
                using var assetStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NearClippingPlaneAdjuster.Icons." + imageName);
                using var tempStream = new MemoryStream((int)assetStream.Length);
                assetStream.CopyTo(tempStream);
                var Texture2 = new Texture2D(2, 2);
                ImageConversion.LoadImage(Texture2, tempStream.ToArray());
                Texture2.wrapMode = TextureWrapMode.Clamp;
                Texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                return Texture2;
            }
            catch (System.Exception ex) { NearClipPlaneAdjMod.Logger.Error("Failed to load image from asset bundle: " + imageName + "\n" + ex.ToString()); return null; }
        }


        public static void InitUi()
        {
            loadAssets();

            if (NearClipPlaneAdjMod.amapi_ModsFolder.Value)
                AMUtils.AddToModsFolder("<color=#ff00ff>NearClip Plane Adj</color>", () => AMsubMenu(), plane);
            else
                VRCActionMenuPage.AddSubMenu(ActionMenuPage.Options, "<color=#ff00ff>NearClip Plane Adj</color>", () => AMsubMenu(), plane);
        }

        private static void AMsubMenu()
        {
            var clipList = new float[] {
                .05f,
                .01f,
                .001f,
                .0001f
            };

            foreach (var clip in clipList)
            { 
                CustomSubMenu.AddButton(clip.ToString(), () =>
                {
                    NearClipPlaneAdjMod main = new NearClipPlaneAdjMod(); main.ChangeNearClipPlane(clip, true);
                }, StoredIcon(clip));
            }
        }

        
        static ref Texture2D StoredIcon(float key)
        {
            switch (key)
            {
                case .05f: return ref n05;
                case .01f: return ref n01;
                case .001f: return ref n001;
                case .0001f: return ref n0001;
                default: NearClipPlaneAdjMod.Logger.Msg("Something Broke - StoredIcon Switch"); return ref plane;
            }
        }
    }
}
