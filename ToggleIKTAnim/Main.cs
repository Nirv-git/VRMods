using MelonLoader;
using UIExpansionKit.API;
using System;
using System.ComponentModel;
using IKTweaks;

[assembly: MelonInfo(typeof(ToggleIKTAnim.Main), "ToggleIKTAnim", "0.0.0.2", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonPriority(Priority = 2)]

namespace ToggleIKTAnim
{
    public class Main : MelonMod
    {
        private static bool animDisabled;

        
        public override void OnApplicationStart()
        {

            switch (MelonPreferences.GetEntryValue<IKTweaks.IgnoreAnimationsMode>("IkTweaks", "IgnoreAnimationsMode"))
            {
                case IKTweaks.IgnoreAnimationsMode.All: Main.animDisabled = true; break;
                case IKTweaks.IgnoreAnimationsMode.None: Main.animDisabled = false; break;
                default: Main.animDisabled = false; break;
            }

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("IKT Animations Disabled", (action) =>
            {
                if (Main.animDisabled)//Currently disabled, undisable
                {
                    MelonPreferences.SetEntryValue<IKTweaks.IgnoreAnimationsMode>("IkTweaks", "IgnoreAnimationsMode", IKTweaks.IgnoreAnimationsMode.None);
                    Main.animDisabled = !Main.animDisabled;
                }
                else //Currently enabled, disable
                {
                    MelonPreferences.SetEntryValue<IKTweaks.IgnoreAnimationsMode>("IkTweaks", "IgnoreAnimationsMode", IKTweaks.IgnoreAnimationsMode.All);
                    Main.animDisabled = !Main.animDisabled;
                }
                MelonPreferences.Save();
            }, () => Main.animDisabled);
        }
    }
}

