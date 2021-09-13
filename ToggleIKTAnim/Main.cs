using MelonLoader;
using UIExpansionKit.API;

[assembly: MelonInfo(typeof(ToggleIKTAnim.Main), "ToggleIKTAnim", "0.0.0.1", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ToggleIKTAnim
{
    public class Main : MelonMod
    {
        private static bool animDisabled;

        public override void OnApplicationStart()
        {

            switch (MelonPreferences.GetEntryValue<string>("IkTweaks", "IgnoreAnimationsMode"))
            {
                case "All": Main.animDisabled = true; break;
                case "None": Main.animDisabled = false; break;
                default: Main.animDisabled = false; break;
            }

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("IKT Animations Disabled", (action) =>
            {
                if (Main.animDisabled)//Currently disabled, undisable
                {
                    MelonPreferences.SetEntryValue<string>("IkTweaks", "IgnoreAnimationsMode", "None");
                    Main.animDisabled = !Main.animDisabled;
                }
                else //Currently enabled, disable
                {
                    MelonPreferences.SetEntryValue<string>("IkTweaks", "IgnoreAnimationsMode", "All");
                    Main.animDisabled = !Main.animDisabled;
                }
                MelonPreferences.Save();
            }, () => Main.animDisabled);
        }
    }
}