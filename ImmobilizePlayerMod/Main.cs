using System.Collections;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.XR;
using System;


[assembly: MelonInfo(typeof(ImmobilizePlayer.Main), "ImmobilizePlayerMod", "0.4", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmobilizePlayer
{
    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Entry<bool> buttonEnabled;
        public static MelonPreferences_Entry<bool> autoButtonEnabled;
        public static MelonPreferences_Entry<bool> movementToggle;
        public static MelonPreferences_Entry<float> deadZone;
        public static MelonPreferences_Entry<float> delay;
        public static MelonPreferences_Entry<bool> settleBefore;
        public static MelonPreferences_Entry<float> settleTime;

        public static MelonPreferences_Entry<bool> debug;
        public static MelonPreferences_Entry<bool> debugHUD;

        public bool imState;
        public bool pauseAuto = false;
        public static bool tempDisable = false;
        public GameObject buttonQM, buttonQMAuto, buttonQMAutoPause;
        private static VRCUiManager _uiManagerInstance;
        public object coroutine;
        public static Image rightIcon;
        private bool waitingToSettle = false;
        private float nextTime;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("ImmobilizePlayerMod");

            MelonPreferences.CreateCategory("ImPlaMod", "Immobilize Player Mod");
            buttonEnabled = MelonPreferences.CreateEntry<bool>("ImPlaMod", "QuickMenuButton", true, "Put Button on Quick Menu");
            movementToggle = MelonPreferences.CreateEntry<bool>("ImPlaMod", "MovementToggle", false, "Auto toggle if not moving | Only will trigger if using VR and FBT");
            autoButtonEnabled = MelonPreferences.CreateEntry<bool>("ImPlaMod", "autoButtonEnabled", false, "Put Auto Toggle Button on Quick Menu");
            deadZone = MelonPreferences.CreateEntry<float>("ImPlaMod", "DeadZone", 0.03f, "-Auto- Deadzone for Movement detection");
            delay = MelonPreferences.CreateEntry<float>("ImPlaMod", "delay", .001f, "-Auto- Delay between checks");
            settleBefore = MelonPreferences.CreateEntry<bool>("ImPlaMod", "settleBefore", true, "-Auto- Settle for X seconds before Immobilizing");
            settleTime = MelonPreferences.CreateEntry<float>("ImPlaMod", "settleTime", 3f, "-Auto- Time to wait for settling");

            debug = MelonPreferences.CreateEntry<bool>("ImPlaMod", "debug", false, "debug");
            debugHUD = MelonPreferences.CreateEntry<bool>("ImPlaMod", "debugHUD", false, "debugHUD");

            movementToggle.OnValueChanged += OnValueChange;

            AddToQM();
            MelonCoroutines.Start(WaitForQM());

            MethodsResolver.ResolveMethods();
            // Patches - https://github.com/SDraw/ml_mods/blob/af8eb07bd810067b968f0d21bb1dacf0be89d8b3/ml_clv/Main.cs#L28
            if (MethodsResolver.PrepareForCalibration != null)
                HarmonyInstance.Patch(MethodsResolver.PrepareForCalibration, null, new HarmonyLib.HarmonyMethod(typeof(Main), nameof(VRCTrackingManager_PrepareForCalibration)));
            if (MethodsResolver.RestoreTrackingAfterCalibration != null)
                HarmonyInstance.Patch(MethodsResolver.RestoreTrackingAfterCalibration, null, new HarmonyLib.HarmonyMethod(typeof(Main), nameof(VRCTrackingManager_RestoreTrackingAfterCalibration)));
            if (MethodsResolver.IKTweaks_Calibrate != null)
                HarmonyInstance.Patch(MethodsResolver.IKTweaks_Calibrate, new HarmonyLib.HarmonyMethod(typeof(Main), nameof(VRCTrackingManager_PrepareForCalibration)), null);
            if (MethodsResolver.IKTweaks_ApplyStoredCalibration != null)
                HarmonyInstance.Patch(MethodsResolver.IKTweaks_ApplyStoredCalibration, new HarmonyLib.HarmonyMethod(typeof(Main), nameof(VRCTrackingManager_RestoreTrackingAfterCalibration)), null);
        }
        private void AddToQM()
        {
            Logger.Msg($"Adding QM button");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("Immobilize", (action) =>
            {
                imState = action;
                SetImmobilize(action);
            }, () => false, (button) => { buttonQM = button; button.gameObject.SetActive(buttonEnabled.Value); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("Immobilize Auto Toggle", (action) =>
            {
                movementToggle.Value = action;
            }, () => movementToggle.Value, (button) => { buttonQMAuto = button; button.gameObject.SetActive(autoButtonEnabled.Value || movementToggle.Value); });
        }

        public override void OnPreferencesSaved()
        {
            if (buttonQM != null)
                buttonQM.SetActive(buttonEnabled.Value);
            if (buttonQMAuto != null)
                buttonQMAuto.SetActive(autoButtonEnabled.Value || movementToggle.Value);
            if (!rightIcon?.Equals(null)??false)
                rightIcon.gameObject.SetActive(movementToggle.Value && debugHUD.Value);
        }

        private void OnValueChange(bool oldValue, bool newValue)
        {
            if (oldValue == newValue) return;
            if (newValue)
                coroutine = MelonCoroutines.Start(AutoSet());
            else
            {
                MelonCoroutines.Stop(coroutine);
                SetImmobilize(false);
            }
        }

        public IEnumerator WaitForQM()
        {
            while (GameObject.Find("/UserInterface/Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null) //Why wait for the MicButton, because I use this in other mods so I only need to fix one thing if it breaks in the future! Also you can't open the camera without going through the QM
                yield return new WaitForSeconds(1f);
            _uiManagerInstance = (VRCUiManager)typeof(VRCUiManager).GetMethods().First(x => x.ReturnType == typeof(VRCUiManager)).Invoke(null, new object[0]);
            CreateIndicators();
            if (movementToggle.Value) coroutine = MelonCoroutines.Start(AutoSet());
        }

        private void SetImmobilize(bool value)
        {
            VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.Immobilize(value);
        }

        private void CreateIndicators()
        {
            Transform hud = GameObject.Find("UnscaledUI/HudContent").transform;
            GameObject iconTemplate = hud.transform.Find("Hud/AFK/Icon").gameObject;

            rightIcon = UnityEngine.Object.Instantiate(iconTemplate, iconTemplate.transform, true).GetComponent<Image>();
            rightIcon.gameObject.name = "Icon-Immobilize";
            rightIcon.gameObject.transform.SetParent(rightIcon.gameObject.transform.parent.parent.parent.parent);
            rightIcon.gameObject.transform.localPosition = new Vector3(440.379f, 463.72f, 0f);//Vector3(483.379f, 369.72f, 0f);
            rightIcon.gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
            rightIcon.gameObject.SetActive(debugHUD.Value);
            rightIcon.color = new Color(1f, 0f, 1f, 0.25f);
        }

        private bool MenuOpen()
        { //Thanks to https://github.com/RequiDev/ReMod.Core/blob/cdd7e84c65e2c23933576e92f7134aec2f52c0bf/VRChat/VRCUiManagerEx.cs#L5
            if (_uiManagerInstance == null)
                return true;
            return _uiManagerInstance.field_Private_Boolean_0;
        }

        public static bool LocalPlayerFBT()
        { 
            var IKController = VRCPlayer.field_Internal_Static_VRCPlayer_0.field_Private_VRC_AnimationController_0.field_Private_IkController_0;
            var IKControllerEnum = IKController.prop_IkType_0; 
            var hasfbt = IKControllerEnum.HasFlag(IkController.IkType.SixPoint); 
            return hasfbt;
        }

        static public void VRCTrackingManager_PrepareForCalibration() => OnCalibrationBegin();
        static void OnCalibrationBegin()
        { //https://github.com/SDraw/ml_mods/blob/af8eb07bd810067b968f0d21bb1dacf0be89d8b3/ml_clv/Main.cs#L118
            if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "On cal");
            if (movementToggle.Value)
            {
                if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Pausing Auto");
                tempDisable = true;
                movementToggle.Value = false;
            }
        }

        static public void VRCTrackingManager_RestoreTrackingAfterCalibration() => OnCalibrationEnd();
        static void OnCalibrationEnd()
        {
            if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Off cal");
            if (tempDisable)
            {
                if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Resume Auto");
                tempDisable = false;
                movementToggle.Value = true;
            }
        }

        public IEnumerator AutoSet()
        {
            //if(!XRDevice.isPresent) yield break;
            while (movementToggle.Value)
            {
                if (debug.Value) Logger.Msg($"GetAxis - Vertical {Input.GetAxis("Vertical")}, Horizontal {Input.GetAxis("Horizontal")}");
                //if (debug.Value) Logger.Msg($"GetAxisRaw - Vertical {Input.GetAxisRaw("Vertical")}, Horizontal {Input.GetAxisRaw("Horizontal")}");

                if (Mathf.Abs(Input.GetAxis("Vertical")) < deadZone.Value && Mathf.Abs(Input.GetAxis("Horizontal")) < deadZone.Value)
                {
                    if (imState == false) {
                        if (settleBefore.Value)
                        {
                            if (debug.Value) Logger.Msg(ConsoleColor.Yellow, "settleBefore.Value = true");
                            if (!waitingToSettle)
                            {
                                if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "!waitingToSettle");
                                nextTime = Time.time + settleTime.Value;
                                waitingToSettle = true;
                            }
                            if (nextTime > Time.time)
                            {
                                if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "nextTime < Time.time");
                                yield return new WaitForSeconds(delay.Value);
                                continue;
                            }
                            else
                            {
                                if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "NOT nextTime < Time.time");
                                waitingToSettle = false;
                            }
                            if (debug.Value) Logger.Msg(ConsoleColor.Yellow, "End of settleBefore.Value");
                        }
                        if (LocalPlayerFBT()) //Don't do this on users without FBT, as it will look funny
                            SetImmobilize(true);
                        else
                            if (debug.Value) Logger.Msg("Not Immobilizing due to no FBT");
                        imState = true;
                        if (debug.Value) Logger.Msg("No Movement");
                        if (debugHUD.Value) rightIcon.color = new Color(1f, 0f, 0f, 0.25f);
                    }
                }
                else
                {
                    waitingToSettle = false;
                    if (!MenuOpen() && imState == true)
                    {
                        SetImmobilize(false);
                        imState = false;
                        if (debug.Value) Logger.Msg("Movement && Menu not open - " + MenuOpen());
                        if (debugHUD.Value) rightIcon.color = new Color(0f, 1f, 0f, 0.25f);
                    }
                    else
                        if (debug.Value) Logger.Msg("Movement && Menu OPEN || imState = - " + MenuOpen());
                }
                yield return new WaitForSeconds(delay.Value);
            }
        }

    }
}
