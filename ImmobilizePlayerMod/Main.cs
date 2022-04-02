using System.Collections;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.XR;
using System;

[assembly: MelonInfo(typeof(ImmobilizePlayer.Main), "ImmobilizePlayerMod", "0.4.4", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmobilizePlayer
{
    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Entry<bool> buttonEnabled;
        public static MelonPreferences_Entry<bool> autoButtonEnabled;
        public static MelonPreferences_Entry<bool> allowAutoForNonFBT;
        public static MelonPreferences_Entry<bool> allowAutoForFBTnoIKT;
        public static MelonPreferences_Entry<bool> movementToggle;
        public static MelonPreferences_Entry<float> deadZone;
        public static MelonPreferences_Entry<float> delay;
        public static MelonPreferences_Entry<bool> settleBefore;
        public static MelonPreferences_Entry<float> settleTime;

        public static MelonPreferences_Entry<bool> debug;
        public static MelonPreferences_Entry<bool> debugHUD;

        public bool imState;
        public static bool ranOnce = false;
        public bool pauseAuto = false;
        public static bool tempDisable = false;
        private bool waitingToSettle = false;
        private float nextTime;
        public object coroutine;
        public GameObject buttonQM, buttonQMAuto, buttonQMAutoPause;
        public static VRCUiManager _uiManagerInstance;
        public static Image rightIcon;
        public static bool WorldTypeGame = false;
        public static bool CurrentWorldChecked = false;
        public static VRCInput vertical;
        public static VRCInput horizontal;
        public bool qmButtOveride = false;


        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("ImmobilizePlayerMod");

            MelonPreferences.CreateCategory("ImPlaMod", "Immobilize Player Mod");
            buttonEnabled = MelonPreferences.CreateEntry<bool>("ImPlaMod", "QuickMenuButton", true, "Put Button on Quick Menu");
            movementToggle = MelonPreferences.CreateEntry<bool>("ImPlaMod", "MovementToggle", false, "Auto toggle if not moving | Only will trigger if using VR");
            autoButtonEnabled = MelonPreferences.CreateEntry<bool>("ImPlaMod", "autoButtonEnabled", true, "Put Auto Toggle Button on Quick Menu");
            allowAutoForNonFBT = MelonPreferences.CreateEntry<bool>("ImPlaMod", "allowAutoForNonFBT", false, "-Auto- Allow for half body VR players");
            allowAutoForFBTnoIKT = MelonPreferences.CreateEntry<bool>("ImPlaMod", "allowAutoForFBTnoIKT", false, "-Auto- Allow for FBT players without IKTweaks (Not recommened, will cause Chest bone issues)");
            deadZone = MelonPreferences.CreateEntry<float>("ImPlaMod", "DeadZone", 0.03f, "-Auto- Deadzone for Movement detection");
            delay = MelonPreferences.CreateEntry<float>("ImPlaMod", "delay", .001f, "-Auto- Delay between checks");
            settleBefore = MelonPreferences.CreateEntry<bool>("ImPlaMod", "settleBefore", true, "-Auto- Settle for X seconds before Immobilizing");
            settleTime = MelonPreferences.CreateEntry<float>("ImPlaMod", "settleTime", 3f, "-Auto- Time to wait for settling");

            debug = MelonPreferences.CreateEntry<bool>("ImPlaMod", "debug", false, "Debug messages in Console");
            debugHUD = MelonPreferences.CreateEntry<bool>("ImPlaMod", "debugHUD", false, "Debug HUD");

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
                qmButtOveride = action;
                Utils.SetImmobilize(action);
            }, () => false, (button) => { buttonQM = button; button.gameObject.SetActive(buttonEnabled.Value); });
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("Immobilize Auto Toggle", (action) =>
            {
                movementToggle.Value = action;
            }, () => movementToggle.Value, (button) => { buttonQMAuto = button; button.gameObject.SetActive(autoButtonEnabled.Value || movementToggle.Value); });
        }

        public override void OnPreferencesSaved()
        {
            if (movementToggle.Value) autoButtonEnabled.Value = true; //Maybe

            if (buttonQM != null)
                buttonQM.SetActive(buttonEnabled.Value);
            if (buttonQMAuto != null)
                buttonQMAuto.SetActive(autoButtonEnabled.Value || movementToggle.Value);
            if (!rightIcon?.Equals(null)??false)
                rightIcon.gameObject.SetActive(movementToggle.Value && debugHUD.Value);       
        }

        private void OnValueChange(bool oldValue, bool newValue)
        {
            try
            {
                if (oldValue == newValue) return;
                if (newValue)
                {
                    if (CurrentWorldChecked == false) MelonCoroutines.Start(RiskFunct.CheckWorld());
                    coroutine = MelonCoroutines.Start(AutoSet());
                }
                else
                {
                    if (!coroutine?.Equals(null) ?? false) MelonCoroutines.Stop(coroutine);
                    Utils.SetImmobilize(false);
                }
            }
            catch (System.Exception ex) { Logger.Error("Error in starting/stopping coroutine:\n" + ex.ToString()); }
        }

        public IEnumerator WaitForQM()
        {
            while (GameObject.Find("/UserInterface/Canvas_QuickMenu(Clone)/Container/Window/MicButton") == null) //Why wait for the MicButton, because I use this in other mods so I only need to fix one thing if it breaks in the future! Also you can't open the camera without going through the QM
                yield return new WaitForSeconds(1f);
            _uiManagerInstance = (VRCUiManager)typeof(VRCUiManager).GetMethods().First(x => x.ReturnType == typeof(VRCUiManager)).Invoke(null, new object[0]);
            vertical = VRCInputManager.field_Private_Static_Dictionary_2_String_VRCInput_0["Vertical"];
            horizontal = VRCInputManager.field_Private_Static_Dictionary_2_String_VRCInput_0["Horizontal"];
            CreateIndicators();
            if (movementToggle.Value) coroutine = MelonCoroutines.Start(AutoSet());

            Logger.Msg(ConsoleColor.Green, $"Listing JoystickNames");
            foreach (var name in Input.GetJoystickNames())
                Logger.Msg(ConsoleColor.Green, $"{name}");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (buildIndex)
            {
                case -1:
                    qmButtOveride = false;
                    WorldTypeGame = false;
                    CurrentWorldChecked = false;
                    if (movementToggle.Value) MelonCoroutines.Start(RiskFunct.CheckWorld());
                    break;
                default:
                    break;
            }
        }

        static public void VRCTrackingManager_PrepareForCalibration() => OnCalibrationBegin();
        static void OnCalibrationBegin()
        { //https://github.com/SDraw/ml_mods/blob/af8eb07bd810067b968f0d21bb1dacf0be89d8b3/ml_clv/Main.cs#L118
            if (!ranOnce) return;
            try
            {
                if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "On cal");
                if (movementToggle.Value)
                {
                    if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Pausing Auto");
                    tempDisable = true;
                    movementToggle.Value = false;
                }
            }
            catch (System.Exception ex) { Logger.Error("Error in OnCalibrationBegin:\n" + ex.ToString()); }
        }

        static public void VRCTrackingManager_RestoreTrackingAfterCalibration() => OnCalibrationEnd();
        static void OnCalibrationEnd()
        {
            try
            {
                if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Off cal");
                if (tempDisable)
                {
                    if (debug.Value) Logger.Msg(ConsoleColor.Cyan, "Resume Auto");
                    tempDisable = false;
                    movementToggle.Value = true;
                }
            }
            catch (System.Exception ex) { Logger.Error("Error in OnCalibrationEnd:\n" + ex.ToString()); }
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

        public IEnumerator AutoSet()
        {
            ranOnce = true;
            if(!XRDevice.isPresent) yield break;
            while (movementToggle.Value)
            {
                if (debug.Value) { yield return new WaitForSeconds(1); 
                    Logger.Msg($"GetAxis - Vertical {Input.GetAxis("Vertical")}, Horizontal {Input.GetAxis("Horizontal")}"); 
                    Logger.Msg(ConsoleColor.Cyan, $"GetAxis - Vertical {Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickVertical")}, Horizontal {Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal")} _Oculus_Primary" ); 
                    Logger.Msg(ConsoleColor.Blue, $"GetAxisRaw - Vertical {Input.GetAxisRaw("Vertical")}, Horizontal {Input.GetAxisRaw("Horizontal")}");
                    Logger.Msg(ConsoleColor.DarkBlue, $"VRCInput - Vertical {vertical.field_Public_Single_0}, Horizontal {horizontal.field_Public_Single_0}");
                    Logger.Msg(ConsoleColor.DarkGray, $"GetAxis- - Vertical {Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical")}, Horizontal {Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickHorizontal")} _Oculus_Secondary"); 
                }

                if (Mathf.Abs(vertical.field_Public_Single_0) < deadZone.Value && Mathf.Abs(horizontal.field_Public_Single_0) < deadZone.Value)
                {
                    if (imState == false) {
                        if (settleBefore.Value)
                        {
                            //if (debug.Value) Logger.Msg(ConsoleColor.Yellow, "settleBefore.Value = true");
                            if (!waitingToSettle)
                            {
                                //if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "!waitingToSettle");
                                nextTime = Time.time + settleTime.Value;
                                waitingToSettle = true;
                            }
                            if (nextTime > Time.time)
                            {
                                //if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "nextTime < Time.time");
                                yield return new WaitForSeconds(delay.Value);
                                continue;
                            }
                            else
                            {
                                //if (debug.Value) Logger.Msg(ConsoleColor.DarkYellow, "NOT nextTime < Time.time");
                                waitingToSettle = false;
                            }
                            //if (debug.Value) Logger.Msg(ConsoleColor.Yellow, "End of settleBefore.Value");
                        }
                        if (allowAutoForNonFBT.Value || Utils.LocalPlayerFBT()) //Don't do this on users without FBT by default, as it can look funny
                        {  
                            if (!Utils.LocalPlayerFBT() || ( Utils.LocalPlayerFBT() && (Utils.IKTweaksEnabled() || allowAutoForFBTnoIKT.Value)))
                            {  //If FBT also require IKTweaks to be enabled, else your chest bone will get screwed up
                                if (!WorldTypeGame)
                                {
                                    Utils.SetImmobilize(true);
                                    if (debugHUD.Value) rightIcon.color = new Color(1f, 0f, 0f, 0.25f);
                                }
                                else
                                if (debug.Value) Logger.Msg(ConsoleColor.Green, "Not Immobilizing due to GAME world");
                            }
                            else
                                if (debug.Value) Logger.Msg(ConsoleColor.Green, "Not Immobilizing due FBT in use and IKTweaks not enabled (overide isn't enabled)\n" +
                                $"LocalPlayerFBT {Utils.LocalPlayerFBT()} || IKTweaksEnabled {Utils.IKTweaksEnabled()} || allowAutoForFBTnoIKT {allowAutoForFBTnoIKT.Value}");
                        }
                        else
                            if (debug.Value) Logger.Msg(ConsoleColor.Green, "Not Immobilizing due to no FBT (override isn't enabled)\n" +
                                $"LocalPlayerFBT {Utils.LocalPlayerFBT()} || allowAutoForNonFBT {allowAutoForNonFBT.Value} || ");
                        imState = true;
                        if (debug.Value) Logger.Msg(ConsoleColor.Green, "No Movement");
                    }
                }
                else
                {
                    waitingToSettle = false;
                    if (!Utils.MenuOpen() && imState == true)
                    {
                        if (debug.Value) Logger.Msg(ConsoleColor.Green, "Movement && Menu not open");
                        if (!qmButtOveride)
                        {
                            Utils.SetImmobilize(false);
                            imState = false;
                            if (debugHUD.Value) rightIcon.color = new Color(0f, 1f, 0f, 0.25f);
                        }
                        else
                            if (debug.Value) Logger.Msg(ConsoleColor.Green, "QM Immobilize button set, not auto unImmobilizing");
                    }
                    //else
                        //if (debug.Value) Logger.Msg("Movement && Menu OPEN || imState = - " + Utils.MenuOpen());
                }
                yield return new WaitForSeconds(delay.Value);
            }
        }
    }
}
