using System.Collections;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;


[assembly: MelonInfo(typeof(ImmobilizePlayer.Main), "ImmobilizePlayerMod", "0.3.1", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmobilizePlayer
{
    public class Main : MelonMod
    {
        public bool buttonEnabled;
        public bool delayButton;
        public bool firstRun = true;
        public bool imState = false;
        public GameObject buttonQM;

        public override void OnApplicationStart()
        {
            RegisterModPrefs();

            if (delayButton) MelonCoroutines.Start(SetupUI());
            else AddToQM();
        }

        private void AddToQM()
        {
            //MelonLogger.Msg($"Adding QM button");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("Immobilize", (action) =>
            {
                imState = !imState;
                VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.Immobilize(imState);
            }
            , () => imState, (button) => buttonQM = button);

            firstRun = false;
        }

        private IEnumerator SetupUI()//I want the button to be last on the list
        {
            while (QuickMenu.prop_QuickMenu_0 == null) yield return null;
            yield return new WaitForSeconds(13f);
            MelonLogger.Msg($"Adding QM button late and reloading the menu");
            AddToQM();
            MelonPreferences.Save();
        }

        public override void VRChat_OnUiManagerInit()
        {
            MelonCoroutines.Start(ButtonState());
        }

        private IEnumerator ButtonState()
        {//Give UIX a few seconds to make buttons, then set the state of the Toggle 
            yield return new WaitForSeconds(5f);
            OnPreferencesSaved();
        }

        private void RegisterModPrefs()
        {
            MelonPreferences.CreateCategory("ImPlaMod", "Immobilize Player Mod");
            MelonPreferences.CreateEntry<bool>("ImPlaMod", "QuickMenuButton", true, "Put Button on Quick Menu");
            MelonPreferences.CreateEntry<bool>("ImPlaMod", "DelayButton", true, "Delay button position on Menu");

            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            buttonEnabled = MelonPreferences.GetEntryValue<bool>("ImPlaMod", "QuickMenuButton");
            delayButton = MelonPreferences.GetEntryValue<bool>("ImPlaMod", "DelayButton");

            if (!firstRun && buttonEnabled && buttonQM != null)
            {
                buttonQM.active = true;
                //MelonLogger.Msg($"QM Button to active");
            }
            if (!firstRun && !buttonEnabled && buttonQM != null)
            {
                buttonQM.active = false;
                //MelonLogger.Msg($"QM Button to disabled");
            }
            
        }


    }
}
