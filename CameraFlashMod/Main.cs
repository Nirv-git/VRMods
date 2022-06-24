using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;

[assembly: MelonInfo(typeof(CameraFlashMod.Main), "CameraFlashMod", "1.1.1", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Green)]

namespace CameraFlashMod
{
    public class Main : MelonMod
    {
        private const string catagory = "CameraFlashMod";
        public static MelonPreferences_Category cat;

        public static MelonPreferences_Entry<LightType> lightType;
        public static MelonPreferences_Entry<float> lightRange;
        public static MelonPreferences_Entry<float> lightSpotAngle;
        public static MelonPreferences_Entry<Color> lightColor;
        public static MelonPreferences_Entry<float> lightIntensity;
        public static MelonPreferences_Entry<string> savedColors;
        public static MelonPreferences_Entry<bool> buttEn;
        public static MelonPreferences_Entry<float> buttX;
        public static MelonPreferences_Entry<float> buttY;
        public static MelonPreferences_Entry<bool> UIXbuttEn;

        public static GameObject cam, viewfinder, uiButton;
        public static Light flash;

        public override void OnApplicationStart()
        {
            cat = MelonPreferences.CreateCategory(catagory, "Camera Flash Mod");
            buttEn = MelonPreferences.CreateEntry(catagory, nameof(buttEn), true, "Flash Button on Camera Viewfinder");
            buttX = MelonPreferences.CreateEntry(catagory, nameof(buttX), -600f, "X position of button");
            buttY = MelonPreferences.CreateEntry(catagory, nameof(buttY), 100f, "Y position of button");

            UIXbuttEn = MelonPreferences.CreateEntry(catagory, nameof(UIXbuttEn), true, "Settings Button on UIX Camera Menu");
            lightType = MelonPreferences.CreateEntry(catagory, nameof(lightType), LightType.Spot, "Light Type", "", true);
            lightRange = MelonPreferences.CreateEntry(catagory, nameof(lightRange), 20f, "Light Range");
            lightSpotAngle = MelonPreferences.CreateEntry(catagory, nameof(lightSpotAngle), 120f, "Spot Angle");
            lightColor = MelonPreferences.CreateEntry(catagory, nameof(lightColor), Color.white, "Color");
            lightIntensity = MelonPreferences.CreateEntry(catagory, nameof(lightIntensity), 1f, "Intensity");

            savedColors = MelonPreferences.CreateEntry(catagory, nameof(savedColors), "1,0.0,0.0,0.0;2,0.0,0.0,0.0;3,0.0,0.0,0.0;4,0.0,0.0,0.0;5,0.0,0.0,0.0;6,0.0,0.0,0.0", "saved colors", "", true);

            //MelonLogger.Msg($"Init");
            UIX.SetupUI();
            LoadAssets.loadAssets();
            MelonCoroutines.Start(OnLoad());
        }

        public override void OnPreferencesSaved()
        {
            switch (lightType.Value)
            {//Check to make sure the Light Type is set to a valid type for Realtime
                case LightType.Directional: break;
                case LightType.Point: break;
                case LightType.Spot: break;
                default: MelonLogger.Msg("Can not use selected LightType, defaulting to Spot"); lightType.Value = LightType.Spot; break;
            }
            if(UIX.UIXButt != null) UIX.UIXButt.SetActive(UIXbuttEn.Value);
            if(uiButton != null) uiButton.SetActive(buttEn.Value);
        }

        public static IEnumerator OnLoad()
        {
            while (GameObject.Find("_Application/TrackingVolume/PlayerObjects/")?.transform.Find("UserCamera/ViewFinder/PhotoControls/Primary /ControlGroup_Main/Scroll View/Viewport/Content/Timer") == null)
                yield return new WaitForSeconds(1f);
            cam = GameObject.Find("_Application/TrackingVolume/PlayerObjects/")?.transform.Find("UserCamera/PhotoCamera").gameObject;
            viewfinder = GameObject.Find("_Application/TrackingVolume/PlayerObjects/")?.transform.Find("UserCamera/ViewFinder").gameObject;
            InitButtons();
            //MelonLogger.Msg("Init Finish");
        }

        public static void InitButtons()
        {
            var newButt = UnityEngine.Object.Instantiate(viewfinder.transform.Find("PhotoControls/Primary /ControlGroup_Main/Scroll View/Viewport/Content/Timer"));
            var butAction = new System.Action(() =>
            {
                ToggleLight();
            });
            newButt.name = "CameraFlash_UI";
            newButt.SetParent(viewfinder.transform.Find("PhotoControls/Primary /ControlGroup_Main"));

            newButt.gameObject.AddComponent<VRC.SDK3.Components.VRCUiShape>();
            newButt.localPosition = new Vector3(buttX.Value, buttY.Value, 0f);
            newButt.localScale = new Vector3(1f, 1f, .001f);
            newButt.localRotation = new Quaternion(0,0,0,1);
            newButt.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            newButt.GetComponent<Button>().onClick.AddListener(butAction);
            newButt.gameObject.AddComponent<BoxCollider>().size = new Vector3(150, 150, 1);
            newButt.gameObject.GetComponent<BoxCollider>().isTrigger = true;
            newButt.GetComponentInChildren<TMPro.TextMeshProUGUI>().m_text = "Toggle Flash";
            newButt.transform.Find("Icon").gameObject.GetComponent<Image>().overrideSprite = LoadAssets.LightOff;
            newButt.gameObject.SetActive(buttEn.Value);
            uiButton = newButt.gameObject;
            buttX.OnValueChanged += OnPositionChange;
            buttY.OnValueChanged += OnPositionChange;
        }

        private static void OnPositionChange(float oldValue, float newValue)
        {
            if (oldValue == newValue) return;
            uiButton.transform.localPosition = new Vector3(buttX.Value, buttY.Value);
        }

        public static void ToggleLight()
        {
            if (cam is null) return;

            if ((!flash?.Equals(null) ?? false)) //If light isn't null, destroy
            {
                UnityEngine.Object.Destroy(flash);
                flash = null;
                uiButton.transform.Find("Icon").gameObject.GetComponent<Image>().overrideSprite = LoadAssets.LightOff;
            }
            else
            {
                var _light = cam.AddComponent<Light>();
                _light.type = lightType.Value;
                _light.range = lightRange.Value; //Spot|Point
                _light.spotAngle = lightSpotAngle.Value; //Spot
                _light.color = lightColor.Value;
                _light.intensity = lightIntensity.Value;
                _light.boundingSphereOverride = new Vector4(0, 0, 0, 4000);
                _light.renderMode = LightRenderMode.ForcePixel;
                _light.useBoundingSphereOverride = true;
                flash = _light;
                uiButton.transform.Find("Icon").gameObject.GetComponent<Image>().overrideSprite = LoadAssets.LightOn;
            }
        }
        public static void UpdateLight()
        {
            if (!flash?.Equals(null) ?? false)
            {
                flash.type = lightType.Value;
                flash.range = lightRange.Value; //Spot|Point
                flash.spotAngle = lightSpotAngle.Value; //Spot
                flash.color = lightColor.Value;
                flash.intensity = lightIntensity.Value;
            }
        }
    }
}



