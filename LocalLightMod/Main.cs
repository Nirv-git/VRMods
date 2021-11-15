using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using VRCSDK2;
using System;

[assembly: MelonInfo(typeof(LocalLightMod.Main), "LocalLightMod", "0.4", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Cyan)]

namespace LocalLightMod
{
    public class Main : MelonMod
    {
        public static class Config
        {
            static public string name = "DefaultName";
            static public float width = .05f;
            static public float legnth = .1f;
            static public float height = .05f;
            static public bool pickupOrient = false;
            static public bool pickupable = true;
            static public LightType lightType = LightType.Point;
            static public float lightRange = 10;
            static public float lightSpotAngle = 30;
            static public Color lightColor = Color.white;
            static public float lightIntensity = 1;
            static public float lightBounceIntensity = 1;//Remove
            static public LightShadows lightShadows = LightShadows.None;
            static public float lightShadowStr = 1;
            static public int lightShadowCustRes = 2048;
            static public bool hideMeshRender = false;
        }

        public static List<GameObject> lightList = new List<GameObject>();
        public static Dictionary<GameObject, Texture2D> textDic = new Dictionary<GameObject, Texture2D>();
        public static GameObject activeLight;

        private const string catagory = "LocalLightMod";
        public static MelonPreferences_Category cat;

        public static MelonPreferences_Entry<bool> loadDefaults;
        public static MelonPreferences_Entry<bool> textureLights;

        public static MelonPreferences_Entry<string> savedColors;
        public static MelonPreferences_Entry<string> savedPrefs;

        public override void OnApplicationStart()
        {
            cat = MelonPreferences.CreateCategory(catagory, "Local Light Mod");

            loadDefaults = MelonPreferences.CreateEntry(catagory, nameof(loadDefaults), false, "Load Slot 1 as Default");
            textureLights = MelonPreferences.CreateEntry(catagory, nameof(textureLights), true, "Texture Lights with Name");

            savedPrefs = MelonPreferences.CreateEntry(catagory, nameof(savedPrefs), "1,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;2,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;3,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;4,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;5,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False", "saved perfs", "", true);
            savedColors = MelonPreferences.CreateEntry(catagory, nameof(savedColors), "1,0.0,0.0,0.0;2,0.0,0.0,0.0;3,0.0,0.0,0.0;4,0.0,0.0,0.0;5,0.0,0.0,0.0;6,0.0,0.0,0.0", "saved colors", "", true);

            UIX.SetupUI();
            if (loadDefaults.Value) SaveSlots.LoadDefaultSlot();
        }


        public static void CreateLight()
        {
            var liName = Config.name;
            liName += Utils.RandomString(2);

            VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            Vector3 pos = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/HeadEffector").transform.position + (player.transform.forward * .25f); // Gets position of Head 
            GameObject _light = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _light.transform.position = pos;
            _light.transform.rotation = player.transform.rotation;
            _light.name = liName;
            _light.transform.localScale = new Vector3((Config.width), (Config.height), Config.legnth);
            _light.GetOrAddComponent<BoxCollider>().size = new Vector3(1, 1, 1);
            _light.GetOrAddComponent<BoxCollider>().isTrigger = true;
            _light.GetOrAddComponent<MeshRenderer>().enabled = false;
            if (textureLights.Value)
            {
                var tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(tex, ImageGen.ImageToPNG(ImageGen.DrawText(liName)));
                textDic.Add(_light, tex);
                _light.GetOrAddComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
            }
            else _light.GetOrAddComponent<MeshRenderer>().material.SetColor("_Color", Color.black);
            _light.GetOrAddComponent<VRC_Pickup>().proximity = 3f;
            _light.GetOrAddComponent<VRC_Pickup>().InteractionText = liName;
            _light.GetOrAddComponent<VRC_Pickup>().pickupable = Config.pickupable;
            _light.GetOrAddComponent<VRC_Pickup>().orientation = Config.pickupOrient ? VRC_Pickup.PickupOrientation.Any : VRC_Pickup.PickupOrientation.Grip; //Pickups snap to hand
            _light.GetOrAddComponent<VRC_Pickup>().allowManipulationWhenEquipped = true;
            _light.GetOrAddComponent<Rigidbody>().useGravity = false;
            _light.GetOrAddComponent<Rigidbody>().isKinematic = true;
            _light.GetOrAddComponent<MeshRenderer>().enabled = !Config.hideMeshRender;

            _light.GetOrAddComponent<Light>().type = Config.lightType; // LightType.Point LightType.Directional LightType.Spot;
            _light.GetOrAddComponent<Light>().range = Config.lightRange; //Spot|Point
            _light.GetOrAddComponent<Light>().spotAngle = Config.lightSpotAngle; //Spot
            _light.GetOrAddComponent<Light>().color = Config.lightColor;
            _light.GetOrAddComponent<Light>().intensity = Config.lightIntensity;
            _light.GetOrAddComponent<Light>().shadows = Config.lightShadows;
            _light.GetOrAddComponent<Light>().shadowStrength = Config.lightShadowStr;
            _light.GetOrAddComponent<Light>().shadowCustomResolution = Config.lightShadowCustRes;
            _light.GetOrAddComponent<Light>().boundingSphereOverride = new Vector4(0, 0, 0, 4000);
            _light.GetOrAddComponent<Light>().useBoundingSphereOverride = true;
            _light.GetOrAddComponent<Light>().flare = null;
            _light.GetOrAddComponent<Light>().renderMode = LightRenderMode.ForcePixel;

            lightList.Add(_light);
            activeLight = _light;
        }

        public static void UpdateLight(GameObject selLight)
        {
            if (!selLight?.Equals(null) ?? false)
            {
                var _light = selLight;
                _light.GetOrAddComponent<MeshRenderer>().enabled = !Config.hideMeshRender;
                _light.GetOrAddComponent<VRC_Pickup>().orientation = Config.pickupOrient ? VRC_Pickup.PickupOrientation.Any : VRC_Pickup.PickupOrientation.Grip;
                _light.GetOrAddComponent<VRC_Pickup>().pickupable = Config.pickupable;
                _light.GetOrAddComponent<Light>().type = Config.lightType; // LightType.Point LightType.Directional LightType.Spot;
                _light.GetOrAddComponent<Light>().range = Config.lightRange; //Spot|Point
                _light.GetOrAddComponent<Light>().spotAngle = Config.lightSpotAngle; //Spot
                _light.GetOrAddComponent<Light>().color = Config.lightColor;
                _light.GetOrAddComponent<Light>().intensity = Config.lightIntensity;
                _light.GetOrAddComponent<Light>().shadows = Config.lightShadows;
                _light.GetOrAddComponent<Light>().shadowStrength = Config.lightShadowStr;
                _light.GetOrAddComponent<Light>().shadowCustomResolution = Config.lightShadowCustRes;
            }
        }

        public static void LoadLightSettings(GameObject selLight)
        {
            if (!selLight?.Equals(null) ?? false)
            {
                var _light = selLight;
                Config.hideMeshRender = !_light.GetOrAddComponent<MeshRenderer>().enabled;
                Config.pickupOrient = (_light.GetOrAddComponent<VRC_Pickup>().orientation == VRC_Pickup.PickupOrientation.Any);
                Config.pickupable = _light.GetOrAddComponent<VRC_Pickup>().pickupable;
                Config.lightType = _light.GetOrAddComponent<Light>().type;
                Config.lightRange = _light.GetOrAddComponent<Light>().range;
                Config.lightSpotAngle = _light.GetOrAddComponent<Light>().spotAngle;
                Config.lightColor = _light.GetOrAddComponent<Light>().color;
                Config.lightIntensity = _light.GetOrAddComponent<Light>().intensity;
                Config.lightShadows = _light.GetOrAddComponent<Light>().shadows;
                Config.lightShadowStr = _light.GetOrAddComponent<Light>().shadowStrength;
                Config.lightShadowCustRes = _light.GetOrAddComponent<Light>().shadowCustomResolution;
            }
        }

        public static string LightDetailsString(GameObject selLight)
        {
            if (!selLight?.Equals(null) ?? false)
            {
                var _light = selLight;
                return $"{_light.name}\n{_light.GetOrAddComponent<Light>().type}\nR:{_light.GetOrAddComponent<Light>().color.r} G:{_light.GetOrAddComponent<Light>().color.g}" +
                    $" B:{_light.GetOrAddComponent<Light>().color.b}\nInten:{Utils.NumFormat(_light.GetOrAddComponent<Light>().intensity)}";
            }
            return "Null";
        }

        public static void CleanupVisObjects()
        {
            foreach (var obj in lightList)
            {
                CleanupOneObject(obj, false);
            }
            lightList.Clear();
        }

        public static void CleanupOneObject(GameObject obj, bool clearAsGo = true)
        {
            if (!obj?.Equals(null) ?? false)
            {
                //MelonLogger.Msg("Removing Object");
                if(clearAsGo) lightList.Remove(obj);
                UnityEngine.Object.Destroy(obj);
                if (textDic.TryGetValue(obj, out Texture2D tex))
                {
                    //MelonLogger.Msg("Removing Texture");
                    textDic.Remove(obj);
                    if (!tex?.Equals(null) ?? false)
                        UnityEngine.Object.Destroy(tex);
                }
            }
        }
    }
}



