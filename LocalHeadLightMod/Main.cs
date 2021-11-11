using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using System;
using VRChatUtilityKit.Utilities;
using VRChatUtilityKit.Ui;
using System.Reflection;
using UnhollowerRuntimeLib;
using System.IO;

[assembly: MelonInfo(typeof(LocalHeadLightMod.Main), "LocalHeadLightMod", "0.6.2", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace LocalHeadLightMod
{
    public class Main : MelonMod
    {
        public static class Config
        {
            static public LightType lightType = LightType.Spot;
            static public float lightRange = 10;
            static public float lightSpotAngle = 40;
            static public Color lightColor = Color.white;
            static public float lightIntensity = 1;
        }
        public static Light baseObj;
        private static Transform uiButton;
        private const string catagory = "HeadLight Settings";
        public static MelonPreferences_Entry<float> buttX;
        public static MelonPreferences_Entry<float> buttY;

        public override void OnApplicationStart()
        {
            buttX = MelonPreferences.CreateEntry(catagory, nameof(buttX), 200f, "X position of button");
            buttY = MelonPreferences.CreateEntry(catagory, nameof(buttY), -60f, "Y position of button");

            loadAssets();
            VRCUtils.OnUiManagerInit += Init;
        }

        public static void Init()
        {
            try
            {
                SubMenu headLightSub = new SubMenu("Head Light", "headLightSubMenu", "Head Light");
                SubMenu headLightColorSub = new SubMenu("Head Light Colors", "headLightColorSubMen", "HeadLight Colors");
                SubMenu headLightColorAdjustSub = new SubMenu("Head Light Color Adjust", "headLightColorAdjuSubMen", "HeadLight Color Adjust");
                headLightSub.PageLayoutGroup.m_Spacing = -15;

                ButtonGroup buttgroup = null;

                headLightSub.AddButtonGroup(new ButtonGroup("Head Light", "Head Light", new System.Collections.Generic.List<IButtonGroupElement>()
                {
                    new ToggleButton((state) => ToggleLight(state), LightOn, LightOff, "Head Light Toggle", "HeadlightToggle", "Toggle Headlight On", "Toggle Headlight Off"),
                    new ToggleButton((state) =>
                    {
                        if(state)
                            Config.lightType = LightType.Point;
                        else
                            Config.lightType = LightType.Spot;
                        UpdateLight();
                    }, LightOn, flashLight,  "Spot/Point", "SpotPointToggle", "Switch to Point Light", "Switch to Spot Light"),
                    new SingleButton(() =>
                    {
                        UiManager.OpenSubMenu(UiManager.QMStateController.field_Private_UIPage_0, headLightColorSub.uiPage);
                    }, ColorPicker, "Color", "ColorAdj", "Adjust Colors"),
                }, (group) => buttgroup = group)
                );
                buttgroup.ButtonLayoutGroup.constraintCount = 3;
                buttgroup.RemoveButtonHeader();

                headLightSub.AddButtonGroup(new ButtonGroup("Options", "Options", new System.Collections.Generic.List<IButtonGroupElement>()
                {
                    new SingleButton(() => {Config.lightIntensity += .1f; UpdateLight();}, BrightnessHigher, "Intensity +", "Intensity+Button", "Brighten"),
                    new SingleButton(() => {Config.lightSpotAngle += 5f; UpdateLight();}, AnglePlus, "Angle +", "Angle+Button", "Widen Angle"),
                    new SingleButton(() => {Config.lightRange += 1f; UpdateLight();}, SizePlus, "Range +", "Range+Button", "Increase Range"),

                    new SingleButton(() => {Config.lightIntensity = Utils.Clamp(Config.lightIntensity - .1f, 0, 1000); UpdateLight();}, BrightnessLower, "Intensity -", "Intensity-Button", "Dim"),
                    new SingleButton(() => {Config.lightSpotAngle = Utils.Clamp(Config.lightSpotAngle - 5f, 0, 2000); UpdateLight();}, AngleMinus, "Angle -", "Angle-Button", "Narrow Angle"),
                    new SingleButton(() => {Config.lightRange = Utils.Clamp(Config.lightRange - 1f, 0, 2000); UpdateLight();}, SizeMinus, "Range -", "Range-Button", "Lower Range"),

                    new SingleButton(() => {Config.lightIntensity = 1f; UpdateLight();}, Reset, "Reset", "IntensityResetButton", "Reset Brightness"),
                    new SingleButton(() => {Config.lightSpotAngle = 40f; UpdateLight();}, Reset, "Reset", "AngleResetButton", "Reset Angle"),
                    new SingleButton(() => {Config.lightRange = 10f; UpdateLight();}, Reset, "Reset", "RangeResetButton", "Reset Range"),
                }, (group) => buttgroup = group)
                );
                buttgroup.ButtonLayoutGroup.constraintCount = 3;


                headLightColorSub.AddButtonGroup(new ButtonGroup("Options", "Options", new System.Collections.Generic.List<IButtonGroupElement>()
                {
                    new SingleButton(() => {Config.lightColor = Color.white; UpdateLight();}, Trans, "<color=#FFFFFF>White</color>", "WhiteButton"),
                    new SingleButton(() => {Config.lightColor = Color.red; UpdateLight();}, Trans, "<color=#FF0000>Red</color>", "RedButton"),
                    new SingleButton(() => {Config.lightColor = Color.green; UpdateLight();}, Trans, "<color=#00FF00>Green</color>", "GreenButton"),
                    new SingleButton(() => {Config.lightColor = Color.blue; UpdateLight();}, Trans, "<color=#0000FF>Blue</color>", "BlueButton"),

                    new SingleButton(() => UiManager.OpenSubMenu(UiManager.QMStateController.field_Private_UIPage_0, headLightColorAdjustSub.uiPage), Trans, "<color=#FF0000>C</color><color=#FFFF00>u</color><color=#00FF00>s</color><color=#00FFFF>t</color><color=#0000FF>o</color><color=#FF00FF>m</color> Color", "CustomColorButton", "Adjust the color"),
                    new SingleButton(() => {Config.lightColor = Color.magenta; UpdateLight();}, Trans, "<color=#FF00FF>Magenta</color>", "MagentaButton"),
                    new SingleButton(() => {Config.lightColor = Color.yellow; UpdateLight();}, Trans, "<color=#FFFF00>Yellow</color>", "YellowButton"),
                    new SingleButton(() => {Config.lightColor = Color.cyan; UpdateLight();}, Trans, "<color=#00FFFF>Cyan</color>", "CyanButton"),
                })
                );


                headLightColorAdjustSub.AddButtonGroup(new ButtonGroup("Options", "Options", new System.Collections.Generic.List<IButtonGroupElement>()
                {
                    new SingleButton(() => { Config.lightColor.r = Utils.Clamp(Config.lightColor.r + .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#FF0000>Red +</color>", "Red+Button"),
                    new SingleButton(() => { Config.lightColor.g = Utils.Clamp(Config.lightColor.g + .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#00FF00>Green +</color>", "Green+Button"),
                    new SingleButton(() => { Config.lightColor.b = Utils.Clamp(Config.lightColor.b + .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#0000FF>Blue +</color>", "Blue+Button"),
                    new SingleButton(() => {
                        Config.lightColor.r = Utils.Clamp(Config.lightColor.r + .1f, 0f, 2f);
                        Config.lightColor.g = Utils.Clamp(Config.lightColor.g + .1f, 0f, 2f);
                        Config.lightColor.b = Utils.Clamp(Config.lightColor.b + .1f, 0f, 2f); 
                        UpdateLight(); 
                    }, Trans, "<color=#FF0000>A</color><color=#00FF00>L</color><color=#0000FF>L</color> +", "All+Button"),


                    new SingleButton(() => { Config.lightColor.r = Utils.Clamp(Config.lightColor.r - .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#FF0000>Red -</color>", "Red-Button"),
                    new SingleButton(() => { Config.lightColor.g = Utils.Clamp(Config.lightColor.g - .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#00FF00>Green -</color>", "Green-Button"),
                    new SingleButton(() => { Config.lightColor.b = Utils.Clamp(Config.lightColor.b - .1f, 0f, 2f); UpdateLight(); }, Trans, "<color=#0000FF>Blue -</color>", "Blue-Button"),
                    new SingleButton(() => {
                        Config.lightColor.r = Utils.Clamp(Config.lightColor.r - .1f, 0f, 2f);
                        Config.lightColor.g = Utils.Clamp(Config.lightColor.g - .1f, 0f, 2f);
                        Config.lightColor.b = Utils.Clamp(Config.lightColor.b - .1f, 0f, 2f);
                        UpdateLight();
                    }, Trans, "<color=#FF0000>A</color><color=#00FF00>L</color><color=#0000FF>L</color> -", "All-Button"),

                    new SingleButton(() => { Config.lightColor.r = 1f; UpdateLight(); }, Reset, "Reset", "RedResetButton"),
                    new SingleButton(() => { Config.lightColor.g = 1f; UpdateLight(); }, Reset, "Reset", "GreenResetButton"),
                    new SingleButton(() => { Config.lightColor.b = 1f; UpdateLight(); }, Reset, "Reset", "BlueResetButton"),
                    new SingleButton(() => {
                        Config.lightColor.r = 1f;
                        Config.lightColor.g = 1f;
                        Config.lightColor.b = 1f;
                        UpdateLight();
                    }, Reset, "Reset", "AllResetButton")
                }, (group) => buttgroup = group)
                );

                var origButt = UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Here/QMHeader_H1/RightItemContainer/Button_QM_Expand");
                var newButt = UnityEngine.Object.Instantiate(origButt);
                var butAction = new System.Action(() =>
                {
                    UiManager.OpenSubMenu(UiManager.QMStateController.field_Private_UIPage_0, headLightSub.uiPage);
                });
                newButt.name = "HeadLight_UI";
                newButt.SetParent(UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Here/QMHeader_H1"));
                newButt.localPosition = new Vector3(buttX.Value, buttY.Value, 0f); //200f, -60f
                newButt.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                newButt.GetComponent<Button>().onClick.AddListener(butAction);
                newButt.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = "HeadLight";
                newButt.GetComponentInChildren<Image>().overrideSprite = flashLight;
                uiButton = newButt;
                buttX.OnValueChanged += OnPositionChange;
                buttY.OnValueChanged += OnPositionChange;
            }
            catch (System.Exception ex) { MelonLogger.Error("LocalHeadLightMoid Init: " + ex.ToString()); }

        }

        private static void OnPositionChange(float oldValue, float newValue)
        {
            if (oldValue == newValue) return;
            uiButton.gameObject.transform.localPosition = new Vector3(buttX.Value, buttY.Value);
        }

        public static void ToggleLight(bool state)
        {
            GameObject cam = Camera.main.gameObject;
            if (cam is null) return;

            if ((!baseObj?.Equals(null) ?? false) && !state) //If light isn't null and state is false, destroy
            {
                UnityEngine.Object.Destroy(baseObj);
                baseObj = null;
            }
            else
            {
                var _light = cam.AddComponent<Light>();
                _light.type = Config.lightType;
                _light.range = Config.lightRange; //Spot|Point
                _light.spotAngle = Config.lightSpotAngle; //Spot
                _light.color = Config.lightColor;
                _light.intensity = Config.lightIntensity;
                _light.shadows = LightShadows.None;
                _light.boundingSphereOverride = new Vector4(0, 0, 0, 4);
                _light.renderMode = LightRenderMode.ForcePixel;
                _light.useBoundingSphereOverride = true;
                baseObj = _light;
            }        
        }
        public static void UpdateLight()
        {
            if (!baseObj?.Equals(null) ?? false)
            {
                baseObj.type = Config.lightType;
                baseObj.range = Config.lightRange; //Spot|Point
                baseObj.spotAngle = Config.lightSpotAngle; //Spot
                baseObj.color = Config.lightColor;
                baseObj.intensity = Config.lightIntensity;
            }
        }

        public static AssetBundle assetBundleIcons;
        public static Sprite AngleMinus, AnglePlus, BrightnessHigher, BrightnessLower, ColorPicker, flashLight, LightOff, LightOn, SizeMinus, SizePlus, Trans, Reset;
        private static void loadAssets()
        {
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LocalHeadLightMod.headlight"))
            {
                using (var tempStream = new MemoryStream((int)assetStream.Length))
                {
                    assetStream.CopyTo(tempStream);
                    assetBundleIcons = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    assetBundleIcons.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
            }

            if (assetBundleIcons != null)
            {
                try { AngleMinus = LoadTextureSprite("AngleMinus.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { AnglePlus = LoadTextureSprite("AnglePlus.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { BrightnessHigher = LoadTextureSprite("BrightnessHigher.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { BrightnessLower = LoadTextureSprite("BrightnessLower.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { ColorPicker = LoadTextureSprite("ColorPicker.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { flashLight = LoadTextureSprite("flashLight.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { LightOff = LoadTextureSprite("LightOff.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { LightOn = LoadTextureSprite("LightOn.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { SizeMinus = LoadTextureSprite("SizeMinus.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { SizePlus = LoadTextureSprite("SizePlus.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { Trans = LoadTextureSprite("Trans.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }
                try { Reset = LoadTextureSprite("Reset.png"); } catch { MelonLogger.Error("Failed to load image from asset bundle"); }

            }
            else MelonLogger.Error("Bundle was null");
        }

        private static Sprite LoadTextureSprite(string Texture)
        { // https://github.com/KortyBoi/VRChat-TeleporterVR/blob/59bdfb200497db665621b519a9ff9c3d1c3f2bc8/Utils/ResourceManager.cs#L32
            Texture2D Texture2 = assetBundleIcons.LoadAsset_Internal(Texture, Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            Texture2.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            var rec = new Rect(0.0f, 0.0f, Texture2.width, Texture2.height);
            var piv = new Vector2(.5f, 5f);
            var border = Vector4.zero;
            var s = Sprite.CreateSprite_Injected(Texture2, ref rec, ref piv, 100.0f, 0, SpriteMeshType.Tight, ref border, false);
            s.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return s;
        }

    }
}



