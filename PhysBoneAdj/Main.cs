using MelonLoader;
using UnityEngine;
using System;
using System.Collections;
using UIExpansionKit.API;
using VRC.SDK3.Dynamics.PhysBone.Components;




[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonInfo(typeof(PhysBoneAdj.Main), "PhysBoneAdjMod", "0.1", "Nirvash", "https://github.com/Nirv-git/VRMods")]

namespace PhysBoneAdj
{
    public class Main : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static MelonPreferences_Category cat;

        public static MelonPreferences_Entry<bool> rememberLast;
        public static MelonPreferences_Entry<bool> updateEach;

        public static VRCPhysBone lastBone;
        public static float multi = .1f;
        public static UIExpansionKit.API.Controls.IMenuButton multiButt;
        public static AnimationCurve tempCurve = new AnimationCurve();
        public static float keytime = 1f;
        public static float value = 1f;
        public static int selkey = 0;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance("PhysBoneAdjMod", ConsoleColor.Gray);

            cat = MelonPreferences.CreateCategory("PhysBoneAdjMod", "PhysBoneAdjMod");
            rememberLast = MelonPreferences.CreateEntry("PhysBoneAdjMod", nameof(rememberLast), true, "Remember Last Bone");
            updateEach = MelonPreferences.CreateEntry("PhysBoneAdjMod", nameof(updateEach), true, "Update bone after each change");
            AddToQM();
        }

        private void AddToQM()
        {
            Logger.Msg($"Adding QM button");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UiElementsQuickMenu).AddSimpleButton("PhysBoneAdjust", (action) =>
            {
                if(rememberLast.Value && (!lastBone?.Equals(null) ?? false))
                    SingleBoneMenu(lastBone);
                else
                    LocalBonesMenu();
            }); 
        }

        private void LocalBonesMenu()
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);
            Menu.AddLabel("PhysBone Adjust");
            Menu.AddSimpleButton("-Close-", () => Menu.Hide());

            var localBones = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.root.GetComponentsInChildren<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
            foreach (var bone in localBones)
            {
                if (!bone?.rootTransform?.Equals(null) ?? false)
                {
                    Menu.AddSimpleButton($"{bone.rootTransform.name}", (() =>
                    {
                    SingleBoneMenu(bone);
                    lastBone = bone;
                    }));
                }
            }
            Menu.Show();
        }

        private void SingleBoneMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu4Column5Row);

            Menu.AddLabel($"Bone-{bone.rootTransform.name}");
            Menu.AddToggleButton("isAnimated", (action) =>
            {
                bone.isAnimated = action;
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneMenu(bone);
            }, () => bone.isAnimated);
            Menu.AddSimpleButton("-Close-", () => Menu.Hide());
            Menu.AddSimpleButton("<-- Back to Bone List", () => LocalBonesMenu());
            ///////
            Menu.AddSimpleButton("Forces\nMenu", () => SingleBoneForcesMenu(bone));
            Menu.AddSimpleButton("Limits\nMenu", () => SingleBoneLimitsMenu(bone));
            Menu.AddSimpleButton("Grab&Move\nMenu", () => SingleBoneGrabMoveMenu(bone));
            Menu.AddToggleButton("Allow Collison", (action) =>
            {
                bone.allowCollision = action;
                SingleBoneMenu(bone);
            }, () => bone.allowCollision);
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.radius = Clamp(bone.radius - multi, 0f, 10000f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneMenu(bone);
            });
            Menu.AddSimpleButton($"Radius\n{bone.radius:F5}\n-Curve-", () =>
            {
                CurveMenu(bone, "radius", "rad");
            });
            Menu.AddSimpleButton("+", () =>
            {
                bone.radius += multi;
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneMenu(bone);
            });
            multiButt = Menu.AddSimpleButton($"Multiplier\n{multi}", () => ChangeMulti());
            ///////
            Menu.AddLabel($"Endpoint\nX:{bone.endpointPosition.x}\nY:{bone.endpointPosition.y}\nZ:{bone.endpointPosition.z}");
            Menu.AddLabel($"MultiChild Type:\n{bone.multiChildType}");
            Menu.AddSpacer();
            Menu.AddSpacer();
            ///////
            Menu.AddToggleButton("Apply Settings on every change", (action) =>
            {
                updateEach.Value = action;
            }, () => updateEach.Value);
            Menu.AddSpacer();
            Menu.AddSimpleButton("Apply Settings", (action) =>
            {
                MelonCoroutines.Start(ResetBone(bone));
            });
            Menu.AddToggleButton("Bone\nEnabled", (action) =>
            {
                bone.enabled = action;
                SingleBoneMenu(bone);
            }, () => bone.enabled);
            
            Menu.Show();
        }



        private void SingleBoneForcesMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu4ColumnSlim);
            Menu.AddLabel($"Bone-{bone.rootTransform.name}");
            Menu.AddSpacer();
            multiButt = Menu.AddSimpleButton($"Multiplier\n{multi}", () => ChangeMulti());
            Menu.AddSimpleButton("Back", () => SingleBoneMenu(bone));
            ///////
            Menu.AddSimpleButton("-", () => 
            {
                bone.pull = Clamp(bone.pull - multi, 0f, 1f);
                if(updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"Pull\n{bone.pull:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.pull = Clamp(bone.pull + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "pull", "force");
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.spring = Clamp(bone.spring - multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"Momentum/Spring\n{bone.spring:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.spring = Clamp(bone.spring + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "spring", "force");
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.stiffness = Clamp(bone.stiffness - multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"Stiffness\n{bone.stiffness:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.stiffness = Clamp(bone.stiffness + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "stiffness", "force");
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.immobile = Clamp(bone.immobile - multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"Immobile\n{bone.immobile:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.immobile = Clamp(bone.immobile + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "immobile", "force");
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.gravity = Clamp(bone.gravity - multi, -1f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"Gravity\n{bone.gravity:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.gravity = Clamp(bone.gravity + multi, -1f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "gravity", "force");
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.gravityFalloff = Clamp(bone.gravityFalloff - multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddLabel($"GravityFalloff\n{bone.gravityFalloff:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.gravityFalloff = Clamp(bone.gravityFalloff + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneForcesMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "gravityFalloff", "force");
            });
            ///////
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSimpleButton("Help", () =>
            {
                ForceHelpMenu(bone);
            });

            Menu.Show();
        }

        private void ForceHelpMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
            Menu.AddSimpleButton("Back", () => SingleBoneForcesMenu(bone));
            Menu.AddLabel("*Pull - Amount of force used to return bones to their rest position.");
            Menu.AddLabel("*Spring/Momentum - Amount bones will wobble when trying to reach their rest position.");
            Menu.AddLabel("*Stiffness - The amount bones will try to stay at their resting position. Only available in Advanced Integration Type.");
            Menu.AddLabel("*Immobile - Reduces any motion as calculated from the root transform's parent.");
            Menu.AddLabel("*Gravity - Amount of gravity applied to bones. Positive value pulls bones down, negative pulls upwards.");
            Menu.AddLabel("*Gravity Falloff - Only available if Gravity is non-zero. It controls how much Gravity is removed while in the rest position. A value of 1.0 means that Gravity will not affect the bone while in rest position at all. This allows you to have the effects of gravity when the bone is rotated off the initial position without affecting the bone's rest state.");
            Menu.AddSimpleButton($"**Open ReadMe on VRC's Site**", () =>
            {
                Application.OpenURL("https://docs.vrchat.com/docs/physbones#forces");
            });
            Menu.Show();
        }

        private void SingleBoneLimitsMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnSlim);
            Menu.AddLabel($"Bone-{bone.rootTransform.name}");
            Menu.AddLabel($"Rotaion\nX:{bone.limitRotation.x}\nY:{bone.limitRotation.y}\nZ:{bone.limitRotation.z}");
            Menu.AddSimpleButton("Back", () => SingleBoneMenu(bone));
            ///////
            Menu.AddSpacer();
            Menu.AddSimpleButton($"Type:{bone.limitType}", () =>
            {
                switch (bone.limitType)
                {
                    case VRCPhysBone.LimitType.None: bone.limitType = VRCPhysBone.LimitType.Angle; break;
                    case VRCPhysBone.LimitType.Angle: bone.limitType = VRCPhysBone.LimitType.Hinge; break;
                    case VRCPhysBone.LimitType.Hinge: bone.limitType = VRCPhysBone.LimitType.Polar; break;
                    case VRCPhysBone.LimitType.Polar: bone.limitType = VRCPhysBone.LimitType.None; break;
                    default: bone.limitType = VRCPhysBone.LimitType.None; break;
                }
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            Menu.AddSpacer();
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.maxAngleX = Clamp(bone.maxAngleX - 1, 0f, 180f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            Menu.AddLabel($"Max Angle {bone.maxAngleX:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.maxAngleX = Clamp(bone.maxAngleX + 1, 0f, 180f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            ///////
            Menu.AddSimpleButton("--", () =>
            {
                bone.maxAngleX = Clamp(bone.maxAngleX - 5, 0f, 180f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "maxAngleX", "limit");
            });
            Menu.AddSimpleButton("++", () =>
            {
                bone.maxAngleX = Clamp(bone.maxAngleX + 5, 0f, 180f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            ///////
            Menu.AddLabel("vvvv");
            Menu.AddLabel("Polar Only");
            Menu.AddLabel("vvvv");
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.maxAngleZ = Clamp(bone.maxAngleZ - 1, 0f, 90f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            Menu.AddLabel($"Max Angle Z/Yaw {bone.maxAngleZ:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.maxAngleZ = Clamp(bone.maxAngleZ + 1, 0f, 90f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            ///////
            Menu.AddSimpleButton("--", () =>
            {
                bone.maxAngleZ = Clamp(bone.maxAngleZ - 5, 0f, 90f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "maxAngleZ", "limit");
            });
            Menu.AddSimpleButton("++", () =>
            {
                bone.maxAngleZ = Clamp(bone.maxAngleZ + 5, 0f, 90f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneLimitsMenu(bone);
            });
            ///////
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSimpleButton("Help", () =>
            {
                LimitsHelpMenu(bone);
            });


            Menu.Show();
        }

        private void LimitsHelpMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
            Menu.AddSimpleButton("Back", () => SingleBoneLimitsMenu(bone));
            Menu.AddLabel("*Setting Limits allows you to limit the amount that a PhysBone chain can move.");
            Menu.AddLabel("*Angle means the bone chain will be limited to some Max Angle, centered on an axis as defined by Rotation");
            Menu.AddLabel("*Hinge means that the bone chain will be limited to some Max Angle along the plane defined by the Rotation");
            Menu.AddLabel("*Polar is a bit more complicated. If you take a Hinge and sweep it across Yaw by some amount, you get a segment of a sphere in Polar coordinates. You can configure Max Pitch and Max Yaw to adjust the size of the segment, and use Rotation to define where that segment is located on the sphere. The visualization for Polar is especially helpful.");
            Menu.AddSimpleButton($"**Open ReadMe on VRC's Site**", () =>
            {
                Application.OpenURL("https://docs.vrchat.com/docs/physbones#limits");
            });
            Menu.Show();
        }
        private void SingleBoneGrabMoveMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnSlim);
            Menu.AddLabel($"Bone-{bone.rootTransform.name}");
            multiButt = Menu.AddSimpleButton($"Multiplier\n{multi}", () => ChangeMulti());
            Menu.AddSimpleButton("Back", () => SingleBoneMenu(bone));
            ///////
            Menu.AddToggleButton("Allow Grabbing", (action) =>
            {
                bone.allowGrabbing = action;
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            }, () => bone.allowGrabbing);
            Menu.AddToggleButton("Allow Posing", (action) =>
            {
                bone.allowPosing = action;
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            }, () => bone.allowPosing);
            Menu.AddSpacer();
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.grabMovement = Clamp(bone.grabMovement - multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            });
            Menu.AddLabel($"Grab movement\n{bone.grabMovement:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.grabMovement = Clamp(bone.grabMovement + multi, 0f, 1f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                bone.maxStretch = Clamp(bone.maxStretch - multi, 0f, 5f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            });
            Menu.AddLabel($"Max strech\n{bone.maxStretch:F5}");
            Menu.AddSimpleButton("+", () =>
            {
                bone.maxStretch = Clamp(bone.maxStretch + multi, 0f, 5f);
                if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                SingleBoneGrabMoveMenu(bone);
            });
            ///////
            Menu.AddSpacer();
            Menu.AddSimpleButton("Curve", () =>
            {
                CurveMenu(bone, "maxStretch", "move");
            });
            Menu.AddSpacer();
            ///////
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSpacer();
            ///////
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSpacer();
            ///////
            Menu.AddSpacer();
            Menu.AddSpacer();
            Menu.AddSimpleButton("Help", () =>
            {
                GrabMoveHelpMenu(bone);
            });

            Menu.Show();
        }

        private void GrabMoveHelpMenu(VRCPhysBone bone)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
            Menu.AddSimpleButton("Back", () => SingleBoneGrabMoveMenu(bone));
            Menu.AddLabel("*Allow Grabbing - Allows players to grab the bones.");
            Menu.AddLabel("*Allow Posing - Allows players to pose the bones after grabbing.");
            Menu.AddLabel("*Grab Movement - Controls how grabbed bones move. A value of zero results in bones using pull & spring to reach the grabbed position. A value of one results in bones immediately moving to the grabbed position.");
            Menu.AddLabel("*Max Stretch - Maximum amount the bones can stretch when being grabbed. Actual length based on each bone's original rest length.");
            Menu.AddSimpleButton($"**Open ReadMe on VRC's Site**", () =>
            {
                Application.OpenURL("https://docs.vrchat.com/docs/physbones#grab--pose");
            });
            Menu.Show();
        }


        private void CurveMenu(VRCPhysBone bone, string type, string menu)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnSlimmer);
            Menu.AddLabel(type + " - " + bone.rootTransform.name);
            Menu.AddSimpleButton("Create/Edit Curve(Beta)", () =>
            {
                NewCurveMenu(bone, type, menu);
            }); 
            Menu.AddSimpleButton("-Back-", () =>
            {
                switch (menu)
                {
                    case "force": SingleBoneForcesMenu(bone); break;
                    case "limit": SingleBoneLimitsMenu(bone); break;
                    case "move": SingleBoneGrabMoveMenu(bone); break;
                    case "rad": SingleBoneMenu(bone); break;
                }
            });
            ///////
            AnimationCurve existingCurve = null;
            switch (type)
            {
                case "pull": existingCurve = bone.pullCurve; break;
                case "spring": existingCurve = bone.springCurve; break;
                case "stiffness": existingCurve = bone.stiffnessCurve; break;
                case "immobile": existingCurve = bone.immobileCurve; break;
                case "gravity": existingCurve = bone.gravityCurve; break;
                case "gravityFalloff": existingCurve = bone.gravityFalloffCurve; break;
                case "maxAngleX": existingCurve = bone.maxAngleXCurve; break;
                case "maxAngleZ": existingCurve = bone.maxAngleZCurve; break;
                case "maxStretch": existingCurve = bone.maxStretchCurve; break;
                case "radius": existingCurve = bone.radiusCurve; break;
            }
            
            if (existingCurve != null)
            {
                foreach (var key in existingCurve.keys)
                {
                    Menu.AddLabel($"T:{key.time:F2}");
                    Menu.AddLabel($"V:{key.value:F3}");
                    Menu.AddSpacer();
                }
            }
            else Menu.AddLabel($"Curve is Null");

            Menu.Show();
        }


        private void NewCurveMenu(VRCPhysBone bone, string type, string menu)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu3ColumnSlimmer);
            Menu.AddLabel(type + " - " + bone.rootTransform.name);
            Menu.AddSimpleButton("Apply Curve to Bone", () =>
            {
                var ConfirmMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
                ConfirmMenu.AddLabel("You are sure you want to apply the curve to this bone?");
                ConfirmMenu.AddSimpleButton("Yes", () => {
                    switch (type)
                    {
                        case "pull": bone.pullCurve = tempCurve; break;
                        case "spring": bone.springCurve = tempCurve; break;
                        case "stiffness": bone.stiffnessCurve = tempCurve; break;
                        case "immobile": bone.immobileCurve = tempCurve; break;
                        case "gravity": bone.gravityCurve = tempCurve; break;
                        case "gravityFalloff": bone.gravityFalloffCurve = tempCurve; break;
                        case "maxAngleX": bone.maxAngleXCurve = tempCurve; break;
                        case "maxAngleZ": bone.maxAngleZCurve = tempCurve; break;
                        case "maxStretch": bone.maxStretchCurve = tempCurve; break;
                        case "radius": bone.radiusCurve = tempCurve; break;
                    }
                    if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));
                    NewCurveMenu(bone, type, menu);
                });
                ConfirmMenu.AddSimpleButton("No", () => NewCurveMenu(bone, type, menu));
                ConfirmMenu.Show();
            });
            Menu.AddSimpleButton("-Back-", () =>
            {
                CurveMenu(bone, type, menu);
            });
            ///////
            Menu.AddSimpleButton("New Curve", () =>
            {
                tempCurve = new AnimationCurve();
                NewCurveMenu(bone, type, menu);
            });
            
            Menu.AddSimpleButton("Copy Curve from Existing", () =>
            {
                tempCurve = new AnimationCurve();

                switch (type)
                {
                    case "pull": tempCurve = bone.pullCurve; break;
                    case "spring": tempCurve = bone.springCurve; break;
                    case "stiffness": tempCurve = bone.stiffnessCurve; break;
                    case "immobile": tempCurve = bone.immobileCurve; break;
                    case "gravity": tempCurve = bone.gravityCurve; break;
                    case "gravityFalloff": tempCurve = bone.gravityFalloffCurve; break;
                    case "maxAngleX": tempCurve = bone.maxAngleXCurve; break;
                    case "maxAngleZ": tempCurve = bone.maxAngleZCurve; break;
                    case "maxStretch": tempCurve = bone.maxStretchCurve; break;
                    case "radius": tempCurve = bone.radiusCurve; break;
                }

                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton("Remove Curve from Bone", () =>
            {
                var ConfirmMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
                ConfirmMenu.AddLabel("You are sure you want to remove the curve from this bone?");
                ConfirmMenu.AddSimpleButton("Yes", () => {
                    switch (type)
                    {
                        case "pull": bone.pullCurve = new AnimationCurve(); break;
                        case "spring": bone.springCurve = new AnimationCurve(); break;
                        case "stiffness": bone.stiffnessCurve = new AnimationCurve(); break;
                        case "immobile": bone.immobileCurve = new AnimationCurve(); break;
                        case "gravity": bone.gravityCurve = new AnimationCurve(); break;
                        case "gravityFalloff": bone.gravityFalloffCurve = new AnimationCurve(); break;
                        case "maxAngleX": bone.maxAngleXCurve = new AnimationCurve(); break;
                        case "maxAngleZ": bone.maxAngleZCurve = new AnimationCurve(); break;
                        case "maxStretch": bone.maxStretchCurve = new AnimationCurve(); break;
                        case "radius": bone.radiusCurve = new AnimationCurve(); break;
                    }
                    if (updateEach.Value) MelonCoroutines.Start(ResetBone(bone));

                    NewCurveMenu(bone, type, menu);
                });
                ConfirmMenu.AddSimpleButton("No", () => NewCurveMenu(bone, type, menu));
                ConfirmMenu.Show();
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                keytime = Clamp(keytime - .05f, 0f, 1f);
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton($"Key Time: {keytime:F2}\nSet to:0/1", () =>
            {
                if (keytime != 0)
                    keytime = 0;
                else
                    keytime = 1;
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton("+", () =>
            {
                keytime = Clamp(keytime + .05f, 0f, 1f);
                NewCurveMenu(bone, type, menu);
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                value = Clamp(value - .05f, 0f, 1f);
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton($"Value: {value:F3}\nSet to:0/1", () =>
            {
                if (value != 0)
                    value = 0;
                else
                    value = 1;
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton("+", () =>
            {
                value = Clamp(value + .05f, 0f, 1f);
                NewCurveMenu(bone, type, menu);
            });
            ///////
            Menu.AddSimpleButton("-", () =>
            {
                selkey = Clamp(selkey - 1, 0, tempCurve.GetKeys().Length-1);
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton($"Select Key #\n{selkey}", () =>
            {
                keytime = tempCurve.GetKey(selkey).time;
                value = tempCurve.GetKey(selkey).value;
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton("+", () =>
            {
                selkey = Clamp(selkey + 1, 0, tempCurve.GetKeys().Length-1);
                NewCurveMenu(bone, type, menu);
            });
            ///////
            Menu.AddSimpleButton("Add new Key", () =>
            {
                tempCurve.AddKey(keytime, value);
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton($"Write to Key #\n{selkey}", () =>
            {
                var anim = new AnimationCurve();
                int i = 0;
                foreach (var key in tempCurve.keys)
                {
                    if (i == selkey)
                    {
                        var keyframe = new Keyframe();
                        keyframe.inTangent = tempCurve.GetKey(selkey).inTangent;
                        keyframe.inWeight = tempCurve.GetKey(selkey).inWeight;
                        keyframe.outTangent = tempCurve.GetKey(selkey).outTangent;
                        keyframe.outWeight = tempCurve.GetKey(selkey).outWeight;
                        keyframe.time = keytime;
                        keyframe.value = value;
                        keyframe.weightedMode = tempCurve.GetKey(selkey).weightedMode;
                        anim.AddKey(keyframe);
                    }
                    else
                        anim.AddKey(tempCurve.GetKey(i));
                    i++;
                }
                tempCurve = anim;
                NewCurveMenu(bone, type, menu);
            });
            Menu.AddSimpleButton($"Delete Key #\n{selkey}", () =>
            {
                var anim = new AnimationCurve();
                int i = 0;
                foreach (var key in tempCurve.keys)
                {
                    if (!(i == selkey))
                    {
                        anim.AddKey(tempCurve.GetKey(i));
                    }      
                    i++;
                }
                tempCurve = anim;
                NewCurveMenu(bone, type, menu);
            });
            ///////
            if (tempCurve != null)
            {
                int i = 0;
                foreach (var key in tempCurve.keys)
                {
                    Menu.AddLabel($"Key:{i}");
                    Menu.AddLabel($"T:{key.time:F2}");
                    Menu.AddLabel($"V:{key.value:F3}");
                    i++;
                }
                Menu.AddSpacer();
                Menu.AddSpacer();
                Menu.AddSimpleButton("Help", () =>
                {
                    CurveHelp(bone, type, menu);
                });
            }
            else Menu.AddLabel($"Curve is Null");

            Menu.Show();
        }


        private void CurveHelp(VRCPhysBone bone, string type, string menu)
        {
            var Menu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu1Column);
            Menu.AddSimpleButton("Back", () => NewCurveMenu(bone, type, menu));
            Menu.AddLabel("*This is kinda an experimental option for editing curves. It has a terrible UI, I know, suggestions welcome!");
            Menu.AddLabel("*The logic this tool uses is simpple and likely causes oddness when editing existing curves due to the way tanget stuff works, idk.");

            Menu.AddLabel("*Apply Curve to Bone - Sets the currently shown curve to the bone");
            Menu.AddLabel("*New Curve - Creates a blank curve for you to edit");
            Menu.AddLabel("*Copy Curve from Existing - Copies curce from the bone");
            Menu.AddLabel("*Remove Curve from Bone - Sets a blank curve on the bone, essentially none at all");
            Menu.AddLabel("*Key Time - Editable Time , used for Adding or Writting to a Key");
            Menu.AddLabel("*Value - Editable Value, used for Adding or Writting to a Key");
            Menu.AddLabel("*Select Key # - Copies values from key # to the above two feilds");
            Menu.AddLabel("*Add new Key - Adds a new key with the values above");
            Menu.AddLabel("*Write to Key # - Overwrites the selected key # with the values above");
                Menu.AddLabel("*Delete Key # - Removes the selected key from the curve"); 
            Menu.AddLabel("*Keyframe list - Lists all keys in the curve. Key# indexed at 0 | Time 0-1 | Value 0-1");
            Menu.Show();
        }


        private void ChangeMulti()
        {
            switch (multi)
            {
                case .0001f: multi = .001f; break;
                case .001f: multi = .01f; break;
                case .01f: multi = .1f; break;
                case .1f: multi = 1f; break;
                case 1f: multi = .0001f; break;
                default: multi = .1f; break;
            }
            multiButt.Text = $"Multiplier\n{multi}";
        }

        public float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static IEnumerator ResetBone(VRCPhysBone bone)
        {   //Don't see another clean way to do this
            bone.enabled = false;
            yield return new WaitForSeconds(.0001f);
            bone.enabled = true;
        }

    }
}

namespace UIExpansionKit.API
{

    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu1Column = new LayoutDescription { NumColumns = 1, RowHeight = 375 / 10, NumRows = 10 };
        public static LayoutDescription QuickMenu3ColumnSlim = new LayoutDescription { NumColumns = 3, RowHeight = 375 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu3ColumnSlimmer = new LayoutDescription { NumColumns = 3, RowHeight = 375 / 11, NumRows = 11 };

        public static LayoutDescription QuickMenu4Column5Row = new LayoutDescription { NumColumns = 4, RowHeight = 375 / 5, NumRows = 5 };
        public static LayoutDescription QuickMenu4ColumnSlim = new LayoutDescription { NumColumns = 4, RowHeight = 375 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu1Column11Row = new LayoutDescription { NumColumns = 1, RowHeight = 375 / 11, NumRows = 11 };
    }
}
