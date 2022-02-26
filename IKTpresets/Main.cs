using System.Collections;
using MelonLoader;
using UnityEngine;
using System;
using System.Linq;
using UIExpansionKit.API;
using UnityEngine.UI;

[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonInfo(typeof(IKTpresets.Main), "IKTpresets", "0.1.6", "Nirvash", "https://github.com/Nirv-git/VRMods")]

namespace IKTpresets
{
    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;

        internal const string IkTweaksCategory = "IkTweaks";
        private static MelonPreferences_Category category;
        public static MelonPreferences_Entry<bool> FixShoulders;
        public static MelonPreferences_Entry<bool> CalibrateHalfFreeze;
        public static MelonPreferences_Entry<bool> CalibrateFollowHead;
        public static MelonPreferences_Entry<bool> CalibrateUseUniversal;
        public static MelonPreferences_Entry<bool> CalibrateStorePerAvatar;
        public static MelonPreferences_Entry<bool> UseKneeTrackers;
        public static MelonPreferences_Entry<bool> UseElbowTrackers;
        public static MelonPreferences_Entry<bool> UseChestTracker;
        public static MelonPreferences_Entry<string> IgnoreAnimationsMode;
        public static MelonPreferences_Entry<bool> PlantFeet;
        public static MelonPreferences_Entry<bool> FullBodyVrIk;
        public static MelonPreferences_Entry<bool> DisableFbt;
        public static MelonPreferences_Entry<float> MaxSpineAngleFwd;
        public static MelonPreferences_Entry<float> MaxSpineAngleBack;
        public static MelonPreferences_Entry<int> SpineRelaxIterations;
        public static MelonPreferences_Entry<float> MaxNeckAngleFwd;
        public static MelonPreferences_Entry<float> MaxNeckAngleBack;
        public static MelonPreferences_Entry<float> NeckPriority;
        public static MelonPreferences_Entry<bool> AddHumanoidPass;
        public static MelonPreferences_Entry<bool> MapToes;
        public static MelonPreferences_Entry<bool> StraightenNeck;
        public static MelonPreferences_Entry<float> StraightSpineAngle;
        public static MelonPreferences_Entry<float> StraightSpinePower;
        public static MelonPreferences_Entry<bool> PinHipRotation;
        public static MelonPreferences_Entry<bool> DoHipShifting;
        public static MelonPreferences_Entry<bool> PreStraightenSpine;
        public static MelonPreferences_Entry<string> MeasureMode;
        public static MelonPreferences_Entry<bool> APoseCalibration;
        public static MelonPreferences_Entry<bool> Unrestrict3PointHeadRotation;
        public static MelonPreferences_Entry<float> WingspanMeasurementAdjustFactor;
        public static MelonPreferences_Entry<bool> OneHandedCalibration;
        public static MelonPreferences_Entry<bool> NoWallFreeze;
        public static MelonPreferences_Entry<Vector3> HandAngleOffset;
        public static MelonPreferences_Entry<Vector3> HandPositionOffset;
        public static MelonPreferences_Entry<float> ElbowGoalOffset;
        public static MelonPreferences_Entry<float> KneeGoalOffset;
        public static MelonPreferences_Entry<float> ChestGoalOffset;

        public enum IgnoreAnimationsModeEnum
        {
            None = 0,
            Head = 1,
            Hands = 2,
            HandAndHead = Head | Hands,
            Others = 4,
            All = HandAndHead | Others
        }

        public enum MeasureAvatarModeEnum
        {
            Default,
            Height,
            ImprovedWingspan
        }

        private static Transform butt, butt2, butt3, buttKeyboard;
        private static string tempString = "";
        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<bool> saveWithEveryChange;
        public static MelonPreferences_Entry<string> ignoreAnimModeDefault;
        public static MelonPreferences_Entry<string> savedPrefs;
        public static MelonPreferences_Entry<string> savedPrefNames;


        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("IKTpresets", ConsoleColor.DarkRed);

            cat = MelonPreferences.CreateCategory("IKTpresets", "IKTpresets");
            saveWithEveryChange = MelonPreferences.CreateEntry("IKTpresets", nameof(saveWithEveryChange), true, "MelonPreferences.Save with every edit in EditMenu");
            ignoreAnimModeDefault = MelonPreferences.CreateEntry("IKTpresets", nameof(ignoreAnimModeDefault), nameof(IgnoreAnimationsModeEnum.None), "Animations mode to toggle between 'Ignore all (always slide around)' and. (Default is 'Play all animations')");
            ExpansionKitApi.RegisterSettingAsStringEnum("IKTpresets",
                nameof(ignoreAnimModeDefault),
                new[]
                {
                    (nameof(IgnoreAnimationsModeEnum.None), "Play all animations"),
                    (nameof(IgnoreAnimationsModeEnum.Head), "Ignore head animations"),
                    (nameof(IgnoreAnimationsModeEnum.Hands), "Ignore hands animations"),
                    (nameof(IgnoreAnimationsModeEnum.HandAndHead), "Ignore head and hands"),
                    //(nameof(IgnoreAnimationsModeEnum.All), "Ignore all (always slide around)")
                });
            savedPrefs = MelonPreferences.CreateEntry("IKTpresets", nameof(savedPrefs), "1,True,True,True,False,True,10,30.,30.,30.,35.,3.6,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;2,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;3,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;4,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;5,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;6,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;7,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;8,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;9,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;10,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;11,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;12,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;13,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;14,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;15,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;16,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5", "savedPrefs", "", true);
            savedPrefNames = MelonPreferences.CreateEntry("IKTpresets", nameof(savedPrefNames), "1,N/A;2,N/A;3,N/A;4,N/A;5,N/A;6,N/A;7,N/A;8,N/A;9,N/A;10,N/A;11,N/A;12,N/A;13,N/A;14,N/A;15,N/A;16,N/A", "savedSlotNames", "", true);

            SaveSlots.MigrateData();

            if (MelonHandler.Mods.Any(m => m.Info.Name == "IKTweaks"))
            {
                ExpansionKitApi.RegisterWaitConditionBeforeDecorating(SetupUI());
            }
        }

        public IEnumerator SetupUI()
        {
            yield return new WaitForSeconds(1f);
            InitPrefs();
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("IKT Presets", () => PresetsMain());
        }

        public void InitPrefs()
        {
            category = MelonPreferences.CreateCategory(IkTweaksCategory, "IK Tweaks");

            FixShoulders = category.GetEntry<bool>("PitchYawShoulders");
            IgnoreAnimationsMode = category.GetEntry<string>(nameof(IgnoreAnimationsMode));//  nameof(IKTweaks.IgnoreAnimationsMode.HandAndHead), "Animations mode in FBT");
            PlantFeet = category.GetEntry<bool>(nameof(PlantFeet));//  false, "Feet stick to ground");

            FullBodyVrIk = category.GetEntry<bool>(nameof(FullBodyVrIk));//  true, "Enable IKTweaks (use custom VRIK)");
            AddHumanoidPass = category.GetEntry<bool>(nameof(AddHumanoidPass));//  true, "Enforce local NetIK (see what others see)");
            MapToes = category.GetEntry<bool>(nameof(MapToes));//  false, "Map toes (use if your feet trackers move with your toes)");

            UseKneeTrackers = category.GetEntry<bool>(nameof(UseKneeTrackers));// false, "Use knee trackers");
            UseElbowTrackers = category.GetEntry<bool>(nameof(UseElbowTrackers));//  false, "Use elbow trackers");
            UseChestTracker = category.GetEntry<bool>(nameof(UseChestTracker));//  false, "Use chest tracker");

            CalibrateFollowHead = category.GetEntry<bool>(nameof(CalibrateFollowHead));//  true, "Avatar follows head when calibrating (recommended)");
            CalibrateHalfFreeze = category.GetEntry<bool>(nameof(CalibrateHalfFreeze));//  true, "Freeze avatar on one trigger hold in follow head mode");
            CalibrateUseUniversal = category.GetEntry<bool>(nameof(CalibrateUseUniversal));//  true, "Use universal calibration (requires follow head mode)");

            CalibrateStorePerAvatar = category.GetEntry<bool>(nameof(CalibrateStorePerAvatar));//  true, "Store calibration per avatar (when not using universal calibration)");
            DisableFbt = category.GetEntry<bool>(nameof(DisableFbt));//  false, "Disable FBT even if trackers are present");
            PinHipRotation = category.GetEntry<bool>(nameof(PinHipRotation));//  true, "Enforce hip rotation match");

            DoHipShifting = category.GetEntry<bool>(nameof(DoHipShifting));//  true, "Shift hip pivot (support inverted hip)");
            PreStraightenSpine = category.GetEntry<bool>(nameof(PreStraightenSpine));//  false, "Pre-straighten spine (improve IK stability)");
            StraightenNeck = category.GetEntry<bool>(nameof(StraightenNeck));//  true, "Straighten neck");

            SpineRelaxIterations = category.GetEntry<int>(nameof(SpineRelaxIterations));//  10, "Spine Relax Iterations (max 25)");
            MaxSpineAngleFwd = category.GetEntry<float>(nameof(MaxSpineAngleFwd));//  30f, "Max spine angle forward (degrees)");
            MaxSpineAngleBack = category.GetEntry<float>(nameof(MaxSpineAngleBack));//  30f, "Max spine angle back (degrees)");

            MaxNeckAngleFwd = category.GetEntry<float>(nameof(MaxNeckAngleFwd));//  30f, "Max neck angle forward (degrees)");
            MaxNeckAngleBack = category.GetEntry<float>(nameof(MaxNeckAngleBack));//  15f, "Max neck angle back (degrees)");
            NeckPriority = category.GetEntry<float>(nameof(NeckPriority));//  2f, "Neck bend priority (1=even with spine, 2=twice as much as spine)");

            StraightSpineAngle = category.GetEntry<float>(nameof(StraightSpineAngle));//  15f, "Straight spine angle (degrees)");
            StraightSpinePower = category.GetEntry<float>(nameof(StraightSpinePower));//  2f, "Straight spine power");
            MeasureMode = category.GetEntry<string>(nameof(MeasureMode));//  nameof(MeasureAvatarMode.ImprovedWingspan), "Avatar scaling mode");

            APoseCalibration = category.GetEntry<bool>(nameof(APoseCalibration));//  false, "A-pose calibration");
            Unrestrict3PointHeadRotation = category.GetEntry<bool>(nameof(Unrestrict3PointHeadRotation));//  true, "Allow more head rotation in 3/4-point tracking");
            WingspanMeasurementAdjustFactor = category.GetEntry<float>(nameof(WingspanMeasurementAdjustFactor));//  1.1f, "Improved wingspan adjustment factor");

            OneHandedCalibration = category.GetEntry<bool>(nameof(OneHandedCalibration));//  false, "One-handed calibration");
            NoWallFreeze = category.GetEntry<bool>(nameof(NoWallFreeze));//  true, "Don't freeze head/hands inside walls");

            HandAngleOffset = category.GetEntry<Vector3>(nameof(HandAngleOffset));//  DefaultHandAngle, "Hand angle offset", null, true);
            HandPositionOffset = category.GetEntry<Vector3>(nameof(HandPositionOffset));//  DefaultHandOffset, "Hand position offset", null, true);

            ElbowGoalOffset = category.GetEntry<float>(nameof(ElbowGoalOffset));//, 0.1f, "Elbows bend goal offset (0-1)");
            KneeGoalOffset = category.GetEntry<float>(nameof(KneeGoalOffset));//, 0.1f, "Knees bend goal offset (0-1)");
            ChestGoalOffset = category.GetEntry<float>(nameof(ChestGoalOffset));//, 0.5f, "Chest bend goal offset (0-1)");
        }

        public void PresetsMain()
        {

            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu4ColumnsSlim);
            menu.AddSimpleButton("Edit current values", (() =>
            {
                menu.Hide(); EditMenu();
            }));

            menu.AddSimpleButton("Save/Load Slots", (() =>
            {
                menu.Hide(); SaveLoadMenu();
            }));
            menu.AddToggleButton("IKT Animations Disabled", (action) =>
            {
                IgnoreAnimationsMode.Value = GetIgnoreAnimValue() ? ignoreAnimModeDefault.Value : "All";
                MelonPreferences.Save();
            }, () => GetIgnoreAnimValue() );

            bool GetIgnoreAnimValue()
            {
                switch (IgnoreAnimationsMode.Value)
                {
                    case "All": return true; 
                    case "None": return false; 
                    default: return false;
                }
            }
            menu.AddSpacer();

            var slotNames = SaveSlots.GetSavedSlotNames();
            foreach (System.Collections.Generic.KeyValuePair<int, System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float, string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>>
                slot in SaveSlots.GetSaved())
            {
                //Logger.Msg($"{slot.Key}");
                menu.AddSimpleButton($"Slot: {slot.Key}\n{slotNames[slot.Key]}", (() =>
                {
                    if (slotNames[slot.Key] == "N/A")
                    {
                        var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                        menu2.AddLabel($"This slot has no name set, are you sure you want to load from it?");
                        menu2.AddSimpleButton($"Yes", () =>
                        {
                            SaveSlots.LoadSlot(slot.Key);
                            PresetsMain();
                        });
                        menu2.AddSimpleButton($"No", () =>
                        {
                            PresetsMain();
                        });
                        menu2.Show();
                    }
                    else
                        SaveSlots.LoadSlot(slot.Key);
                }));
            }

            menu.Show();
        }


        public void EditMenu()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu4ColumnsSlim);

            menu.AddToggleButton("Pitch Yaw Shoulders", (action) =>
            {
                FixShoulders.Value = !FixShoulders.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => FixShoulders.Value);

            menu.AddToggleButton("Enforce hip rotation match", (action) =>
            {
                PinHipRotation.Value = !PinHipRotation.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => PinHipRotation.Value);

            menu.AddToggleButton("Shift hip pivot (support-inverted-hip)", (action) =>
            {
                DoHipShifting.Value = !DoHipShifting.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => DoHipShifting.Value);

            menu.AddToggleButton("Pre-straighten spine (improve IK stability)", (action) =>
            {
                PreStraightenSpine.Value = !PreStraightenSpine.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => PreStraightenSpine.Value);

            menu.AddToggleButton("Straighten Neck", (action) =>
            {
                StraightenNeck.Value = !StraightenNeck.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => StraightenNeck.Value);

            menu.AddSimpleButton($"Spine Relax Iterations:\n{SpineRelaxIterations.Value}", () =>
            {
                string label()
                {
                    return $"Spine Relax Iterations(max 25): {SpineRelaxIterations.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    SpineRelaxIterations.Value += 2;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    SpineRelaxIterations.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"10", () =>
                {
                    SpineRelaxIterations.Value = 10;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    SpineRelaxIterations.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    SpineRelaxIterations.Value -= 2;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Max Spine Angle Fwd:\n{MaxSpineAngleFwd.Value}", () =>
            {
                string label()
                {
                    return $"Max Spine Angle Fwd: {MaxSpineAngleFwd.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    MaxSpineAngleFwd.Value += 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    MaxSpineAngleFwd.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"30", () =>
                {
                    MaxSpineAngleFwd.Value = 30;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    MaxSpineAngleFwd.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    MaxSpineAngleFwd.Value -= 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Max Spine Angle Back:\n{MaxSpineAngleBack.Value}", () =>
            {
                string label()
                {
                    return $"Max Spine Angle Back: {MaxSpineAngleBack.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    MaxSpineAngleBack.Value += 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    MaxSpineAngleBack.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"30", () =>
                {
                    MaxSpineAngleBack.Value = 30;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    MaxSpineAngleBack.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    MaxSpineAngleBack.Value -= 2;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Max Neck Angle Fwd:\n{MaxNeckAngleFwd.Value}", () =>
            {
                string label()
                {
                    return $"Max Neck Angle Fwd: {MaxNeckAngleFwd.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    MaxNeckAngleFwd.Value += 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    MaxNeckAngleFwd.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"30", () =>
                {
                    MaxNeckAngleFwd.Value = 30;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    MaxNeckAngleFwd.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    MaxNeckAngleFwd.Value -= 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Max Neck Angle Back:\n{MaxNeckAngleBack.Value}", () =>
            {
                string label()
                {
                    return $"Max Neck Angle Back: {MaxNeckAngleBack.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    MaxNeckAngleBack.Value += 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    MaxNeckAngleBack.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"15", () =>
                {
                    MaxNeckAngleBack.Value = 15;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    MaxNeckAngleBack.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    MaxNeckAngleBack.Value -= 2;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Neck Priority:\n{NeckPriority.Value}", () =>
            {
                string label()
                {
                    return $"Neck Priority: {NeckPriority.Value}\n(1=even with spine, 2=twice as much as spine)";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    NeckPriority.Value += .2f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    NeckPriority.Value += .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"2f", () =>
                {
                    NeckPriority.Value = 2f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    NeckPriority.Value -= .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    NeckPriority.Value -= .2f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Straight spine angle:\n{StraightSpineAngle.Value}", () =>
            {
                string label()
                {
                    return $"Straight spine angle: {StraightSpineAngle.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    StraightSpineAngle.Value += 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    StraightSpineAngle.Value += 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"15", () =>
                {
                    StraightSpineAngle.Value = 15;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    StraightSpineAngle.Value -= 1;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    StraightSpineAngle.Value -= 5;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Straight spine power:\n{StraightSpinePower.Value}", () =>
            {
                string label()
                {
                    return $"Straight spine power: {StraightSpinePower.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    StraightSpinePower.Value += .2f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    StraightSpinePower.Value += .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"2", () =>
                {
                    StraightSpinePower.Value = 2;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    StraightSpinePower.Value -= .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    StraightSpinePower.Value -= .2f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"Avatar scaling mode:\n{MeasureMode.Value}", () =>
            {
                string label()
                {
                    return $"Avatar scaling mode: {MeasureMode.Value}\n--Must recalibrate--";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"Default", () =>
                {
                    MeasureMode.Value = "Default";
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"Height", () =>
                {
                    MeasureMode.Value = "Height";
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"ImprovedWingspan", () =>
                {
                    MeasureMode.Value = "ImprovedWingspan";
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddSimpleButton($"ImprovedWingspan adjustment factor:\n{WingspanMeasurementAdjustFactor.Value}", () =>
            {
                string label()
                {
                    return $"Improved wingspan adjustment factor: {WingspanMeasurementAdjustFactor.Value}\n--Must recalibrate--";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"++", () =>
                {
                    WingspanMeasurementAdjustFactor.Value += .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    WingspanMeasurementAdjustFactor.Value += .01f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"1.1", () =>
                {
                    WingspanMeasurementAdjustFactor.Value = 1.1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"-", () =>
                {
                    WingspanMeasurementAdjustFactor.Value -= .01f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"--", () =>
                {
                    WingspanMeasurementAdjustFactor.Value -= .1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.Show();
            });

            menu.AddToggleButton("Feet stick to ground", (action) =>
            {
                PlantFeet.Value = !PlantFeet.Value;
                if (saveWithEveryChange.Value) MelonPreferences.Save();
            }, () => PlantFeet.Value);

            menu.AddSimpleButton($"<-Back", () =>
            {
                PresetsMain();
            });

            menu.AddSimpleButton($"Elbow/Knee/Chest GoalOffset\n{ElbowGoalOffset.Value} {KneeGoalOffset.Value} {ChestGoalOffset.Value}", () =>
            {
                string label()
                {
                    return $"Elbow GoalOffset\n{ElbowGoalOffset.Value}";
                }
                string label2()
                {
                    return $"Knee GoalOffset\n{KneeGoalOffset.Value}";
                }
                string label3()
                {
                    return $"Chest GoalOffset\n{ChestGoalOffset.Value}";
                }
                var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
                menu2.AddLabel(label(), (button) => butt = button.transform);
                menu2.AddSimpleButton($"-", () =>
                {
                    ElbowGoalOffset.Value -= .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"0.1", () =>
                {
                    ElbowGoalOffset.Value = 0.1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    ElbowGoalOffset.Value += .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt.GetComponentInChildren<Text>().text = label();
                });
                //
                menu2.AddLabel(label2(), (button) => butt2 = button.transform);
                menu2.AddSimpleButton($"-", () =>
                {
                    KneeGoalOffset.Value -= .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt2.GetComponentInChildren<Text>().text = label2();
                });
                menu2.AddSimpleButton($"0.1", () =>
                {
                    KneeGoalOffset.Value = 0.1f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt2.GetComponentInChildren<Text>().text = label2();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    KneeGoalOffset.Value += .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt2.GetComponentInChildren<Text>().text = label2();
                });
                //
                menu2.AddLabel(label3(), (button) => butt3 = button.transform);
                menu2.AddSimpleButton($"-", () =>
                {
                    ChestGoalOffset.Value -= .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt3.GetComponentInChildren<Text>().text = label3();
                });
                menu2.AddSimpleButton($"0.5", () =>
                {
                    ChestGoalOffset.Value = 0.5f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt3.GetComponentInChildren<Text>().text = label3();
                });
                menu2.AddSimpleButton($"+", () =>
                {
                    ChestGoalOffset.Value += .05f;
                    if (saveWithEveryChange.Value) MelonPreferences.Save();
                    butt3.GetComponentInChildren<Text>().text = label3();
                });
                //
                menu2.AddSimpleButton($"<-Back", () =>
                {
                    EditMenu();
                });
                menu2.AddLabel("Sensible range of values is between 0 and 1.");

                menu2.Show();
            });

            menu.AddSpacer();
            menu.AddSimpleButton($"Help Docs", () =>
            {
                HelpDocs();
            });

            menu.Show();
        }


        public void HelpDocs()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenuWide);

            menu.AddLabel("* Pitch-Yaw Shoulders - changes how shoulder angles are computed in 3/4-point tracking. Enabling it usually provides better-looking results");
            menu.AddLabel("* Enforce hip rotation match - if enabled, avatar's hip rotation will exactly match tracker's rotation. Otherwise, IK may rotate the hip to bend the spine more.");
            menu.AddLabel("* Shift hip pivot (support inverted hip) - if enabled, the hip will be rotated around the midpoint of two leg bones (where the hip bone should be normally). This greatly improves IK on avatars with the inverted hip rig hack.");
            menu.AddLabel("* Pre-straighten spine (improve IK stability) - if enabled, you avatar's spine will be forcefully straightened before solving it. This reduces flippiness/jitter on avatars that have spine bent backwards by default.");
            menu.AddLabel("* Straighten neck - this does something cursed to the neck. No further description can be provided.");
            menu.AddLabel("* Spine Relax Iterations (max 25) - how much work will be done on bending the spine. Below 5 is not recommended, 10 will provide about 1mm precision for hip positioning, 25 is the maximum sensible value.");
            menu.AddLabel("* Maximum bend angles - how much spine/neck can be bent forward/back. If your spine bends too much to your taste or looks cursed on your specific avatar, reduce these angles (minimum recommended value is 1 though)");
            menu.AddLabel("* Neck bend priority - neck will bend this much faster than the spine. This is intended to handle the fact that people move their neck way more than their spine, so IK should start off by bending it, not spine.");
            menu.AddLabel("* Straight spine angle - withing this angle from perfectly straight, the spine will be considered almost straight and maximum bend angles will be reduced.");
            menu.AddLabel("* Straight spine power - controls the curve with which the spine transitions from straight to bend within the straight angle. Recommended values are between 1 and 2.");
            menu.AddLabel("* Avatar scaling mode - controls how your avatar scale is computed. 'Height' scales the avatar so that your real floor alights with the virtual one (at the cost of you likely getting t-rex arms), 'Improved wingspan' attempts to measure avatar arm span more accurately than VRC default.");
            menu.AddLabel("* Improved wingspan adjustment factor - your wingspan is adjusted by this factor in 'Improved wingspan' scaling mode. If you consistently get avatar arms too long/short, consider tweaking this a tiny bit (to like 1.05 or 1.15)");
            menu.AddLabel("* Feet stick to ground - uncheck if you want your feet (and the rest of your avatar) to be unable to leave the ground, like in ol' good times");
            menu.AddLabel("* Hand angles/offsets (found in VRChat Settings menu -> left blue panel -> More IKTweaks -> Adjust hand angles/offsets) - you can configure how avatar hands are positioned relative to controllers. Defaults were tuned for Index controllers, but should be applicable to most other controllers too.");
            menu.AddLabel("* Elbow/knee/chest bend goal offset - controls how far bend goal targets will be away from the actual joint. Lower values should produce better precision with bent joint, higher values - better stability with straight joint. Sensible range of values is between 0 and 1.");

            menu.AddSimpleButton($"Open Full ReadMe on Github", () =>
            {
                Application.OpenURL("https://github.com/knah/VRCMods#iktweaks");
            });
            menu.AddSimpleButton($"<-Back", () =>
            {
                EditMenu();
            });
            menu.Show();
        }




        private void SaveLoadMenu()
        {
            var storedMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnsSlim);
            var slotNames = SaveSlots.GetSavedSlotNames();

            storedMenu.AddSimpleButton("<-Back", (() =>
            {
                PresetsMain();
            }));

            storedMenu.AddSimpleButton($"Edit Slot Names", () =>
            {
                EditSlotNames();
            });
            storedMenu.AddSpacer();
            //
            storedMenu.AddSpacer();
            storedMenu.AddSpacer();
            storedMenu.AddSpacer();

            foreach (System.Collections.Generic.KeyValuePair<int, System.Tuple < System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float, string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>>
                slot in SaveSlots.GetSaved())
            {
                string label =  $"Slot: {slot.Key}\n{slotNames[slot.Key]}";
                storedMenu.AddLabel(label);
                storedMenu.AddSimpleButton($"Load", () =>
                {
                    if (slotNames[slot.Key] == "N/A")
                    {
                        var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                        menu2.AddLabel($"This slot has no name set, are you sure you want to load from it?");
                        menu2.AddSimpleButton($"Yes", () =>
                        {
                            SaveSlots.LoadSlot(slot.Key);
                            SaveLoadMenu();
                        });
                        menu2.AddSimpleButton($"No", () =>
                        {
                            SaveLoadMenu();
                        });
                        menu2.Show();
                    }
                    else
                        SaveSlots.LoadSlot(slot.Key);
                });
                storedMenu.AddSimpleButton($"Save", () =>
                {
                    var menu2 = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1ColumnWideSlim);
                    menu2.AddLabel($"Are you sure you want to save to slot:\n{slot.Key} | {slotNames[slot.Key]}");
                    menu2.AddSimpleButton($"Yes", () =>
                    {
                        SaveSlots.Store(slot.Key);
                        SaveLoadMenu();
                    });
                    menu2.AddSimpleButton($"No", () =>
                    {
                        SaveLoadMenu();
                    });
                    menu2.Show();
                });
            }
            storedMenu.Show();
        }

        private void EditSlotNames()
        {
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnsSlim);

            menu.AddSimpleButton("<-Back", (() =>
            {
                SaveLoadMenu();
            }));
            menu.AddLabel($"Current String:\n{tempString}");
            menu.AddSimpleButton($"Edit String", () =>
            {
                SetName(false);
            });
            menu.AddSpacer();
            menu.AddSpacer();
            menu.AddSpacer();

            var slotNames = SaveSlots.GetSavedSlotNames();
            foreach (System.Collections.Generic.KeyValuePair<int, string> slot in slotNames)
            {
                menu.AddLabel($"Slot: {slot.Key}\n{slot.Value}");
                menu.AddSimpleButton($"Load String", () =>
                {
                    tempString = slot.Value;
                    EditSlotNames();
                });
                menu.AddSimpleButton($"Set String", () =>
                {
                    SaveSlots.StoreSlotNames(slot.Key, tempString);
                    EditSlotNames();
                });

            }
            menu.Show();
        }

        public  void SetName(bool cas)
        {
            var chars = "abcdefghijklmnopqrstuvwxyz-_ ".ToCharArray();
            var menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3Column10Row);

            menu.AddSimpleButton("<-Back", () => { EditSlotNames(); });
            menu.AddLabel(tempString, (button) => buttKeyboard = button.transform); ;
            menu.AddSimpleButton("BackSpace", () => {
                if (tempString.Length > 0) tempString = tempString.Remove(tempString.Length - 1, 1);
                buttKeyboard.GetComponentInChildren<Text>().text = tempString;
            });

            foreach (char c in chars)
            {
                var s = cas ? c.ToString().ToUpper() : c.ToString();
                menu.AddSimpleButton(s, () => {
                    tempString += s;
                    buttKeyboard.GetComponentInChildren<Text>().text = tempString;
                });
            }

            menu.AddSimpleButton("Toggle Case", () => { SetName(!cas); });
            menu.Show();
        }
    }
}

namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenuWide = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu3ColumnsSlim = new LayoutDescription { NumColumns = 6, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu5Columns4Row = new LayoutDescription { NumColumns = 5, RowHeight = 380 / 4, NumRows = 4 };
        public static LayoutDescription QuickMenu1ColumnWideSlim = new LayoutDescription { NumColumns = 1, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu3Column10Row = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 10, NumRows = 10 };
        public static LayoutDescription QuickMenu4ColumnsSlim = new LayoutDescription { NumColumns = 4, RowHeight = 380 /5, NumRows = 5 };
    }
}