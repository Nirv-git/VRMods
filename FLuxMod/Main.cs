using MelonLoader;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnhollowerRuntimeLib;
using System.IO;

[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonInfo(typeof(FLuxMod.Main), "FLuxMod", "0.3", "Nirvash", "https://github.com/Nirv-git/VRMods")]
//Uses https://rollthered.booth.pm/items/3092302
namespace FLuxMod
{
    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<bool> amapi_ModsFolder;

        public static MelonPreferences_Entry<float> flux_HDRClamp;
        public static MelonPreferences_Entry<float> flux_Hue;
        public static MelonPreferences_Entry<float> flux_Colorize;
        public static MelonPreferences_Entry<float> flux_Brightness;
        public static MelonPreferences_Entry<float> flux_Desat;
        public static MelonPreferences_Entry<float> flux_scale;

        public static MelonPreferences_Entry<string> savedPrefs;
        public static MelonPreferences_Entry<string> slot1Name;
        public static MelonPreferences_Entry<string> slot2Name;
        public static MelonPreferences_Entry<string> slot3Name;
        public static MelonPreferences_Entry<string> slot4Name;
        public static MelonPreferences_Entry<string> slot5Name;
        public static MelonPreferences_Entry<string> slot6Name;

        public static GameObject fluxObj;
        public static bool isEnabled = false;
        public static bool pauseOnValueChange = false;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("FLuxMod", ConsoleColor.DarkRed);

            cat = MelonPreferences.CreateCategory("FLuxMod", "FLuxMod");
            amapi_ModsFolder = MelonPreferences.CreateEntry("FLuxMod", nameof(amapi_ModsFolder), false, "Place Action Menu in 'Mods' Sub Menu instead of 'Config' menu (Restart Required)");
            flux_HDRClamp = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_HDRClamp), .222f, "HDRClamp (0-1)"); //.778
            flux_Hue = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_Hue), .102f, "Hue (0-1)");

            flux_Colorize = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_Colorize), .75f, "Colorize (0-1)"); //.25
            flux_Brightness = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_Brightness), .623f, "Brightness (0-1)");
            flux_Desat = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_Desat), .255f, "Desaturation (0-1)");

            flux_scale = MelonPreferences.CreateEntry("FLuxMod", nameof(flux_scale), 1f, "Scale of sphere around vision");

            slot1Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot1Name), "Default", "Slot 1 Name");
            slot2Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot2Name), "No Bloom", "Slot 2 Name");
            slot3Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot3Name), "HDR Only", "Slot 3 Name");
            slot4Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot4Name), "Dim", "Slot 4 Name");
            slot5Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot5Name), "Sleep", "Slot 5 Name");
            slot6Name = MelonPreferences.CreateEntry("FLuxMod", nameof(slot6Name), "SlotName", "Slot 6 Name");
            savedPrefs = MelonPreferences.CreateEntry("FLuxMod", nameof(savedPrefs), "1,0.222,0.102,0.75,0.623,0.255;2,0,0.102,0,1,0;3,0.5,0.102,0,1,0;4,0.5,0.102,0,0.75,0.15;5,0.5,0.102,0,0.10,0.25;6,0.222,0.102,0.75,0.623,0.255", "savedPrefs", "", true);

            loadAssets();

            flux_HDRClamp.OnValueChanged += OnValueChange;
            flux_Hue.OnValueChanged += OnValueChange;
            flux_Colorize.OnValueChanged += OnValueChange;
            flux_Brightness.OnValueChanged += OnValueChange;
            flux_Desat.OnValueChanged += OnValueChange;
            flux_scale.OnValueChanged += OnValueChange;

            CustomActionMenu.InitUi();
        }

        public static void ToggleObject()
        {
            if (!fluxObj?.Equals(null) ?? false)
            {
                try { UnityEngine.Object.Destroy(fluxObj); } catch (System.Exception ex) { Logger.Error(ex.ToString()); }
                fluxObj = null;
                isEnabled = false;
            }
            else
            {
                VRCPlayer player = Utils.GetVRCPlayer();
                if (player?.field_Internal_Animator_0?.isHuman ?? false)
                {
                    GameObject obj = GameObject.Instantiate(fluxPrefab);
                    GameObject cam = Camera.main.gameObject;
                    obj.transform.SetParent(cam.transform);
                    obj.transform.localPosition = new Vector3(0f, 0f, 0f);
                    obj.layer = 5;

                    fluxObj = obj;
                    OnValueChange(0f, 0f);
                    isEnabled = true;
                }
            }
        }
        
        public static void OnValueChange(float oldValue, float newValue)
        { 
            if (!pauseOnValueChange && (!fluxObj?.Equals(null) ?? false))
            {
                //TODO flip images for Colorize and HDR Clamp
                var mat = fluxObj.GetComponent<Renderer>().material;
                mat.SetFloat("_HDRClamp", Utils.Rescale(Utils.Invert(flux_HDRClamp.Value)));
                mat.SetFloat("_Hue", Utils.Rescale(flux_Hue.Value));
                mat.SetFloat("_Colorize", Utils.Rescale(Utils.Invert(flux_Colorize.Value)));
                mat.SetFloat("_Brightness", Utils.Rescale(flux_Brightness.Value));
                mat.SetFloat("_Desaturate", Utils.Rescale(flux_Desat.Value));
                fluxObj.transform.localScale = new Vector3(flux_scale.Value, flux_scale.Value, flux_scale.Value);
                mat.SetFloat("_Length", flux_scale.Value); //0.02
                mat.SetFloat("_Offset", flux_scale.Value); //.3
            }
        }

        public static AssetBundle assetBundle;
        public static GameObject fluxPrefab;
        private void loadAssets()
        {//https://github.com/ddakebono/BTKSASelfPortrait/blob/master/BTKSASelfPortrait.cs
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FLuxMod.flux"))
            {
                using (var tempStream = new MemoryStream((int)assetStream.Length))
                {
                    assetStream.CopyTo(tempStream);
                    assetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
            }

            if (assetBundle != null)
            {
                fluxPrefab = assetBundle.LoadAsset_Internal("FluxSphere", Il2CppType.Of<GameObject>()).Cast<GameObject>();
                fluxPrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }
            else Logger.Error("Bundle was null");
        }
    }
}


