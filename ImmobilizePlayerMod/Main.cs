using System.Collections;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;


[assembly: MelonInfo(typeof(ImmobilizePlayer.Main), "ImmobilizePlayerMod", "0.3.7", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmobilizePlayer
{
    public class Main : MelonMod
    {
        public bool buttonEnabled;
        //public bool delayButton;
        public bool imState = false;
        public GameObject buttonQM;

        public override void OnApplicationStart()
        {
            RegisterModPrefs();
            //if (delayButton) //Dirty hack broke with ML 0.4.0
            //{
            //    //MelonCoroutines.Start(SetupUI());
            //    ExpansionKitApi.RegisterWaitConditionBeforeDecorating(SetupUI()); //This doesnt work for positioning the order of the buttons past a point
            //}
            //else AddToQM();
            AddToQM();
        }

        private void AddToQM()
        {
            MelonLogger.Msg($"Adding QM button");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddToggleButton("Immobilize", (action) =>
            {
                imState = !imState;
                VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.Immobilize(imState);
            }
            , () => imState, (button) => {buttonQM = button; button.gameObject.SetActive(buttonEnabled); });
        }

        //private IEnumerator SetupUI()//I want the button to be last on the list
        //{
        //    while (QuickMenu.prop_QuickMenu_0 == null) yield return null;
        //    yield return new WaitForSeconds(15f);
        //    MelonLogger.Msg($"Adding QM button late");
        //    AddToQM();
        //    //MelonPreferences.Save();
        //}

        private void RegisterModPrefs()
        {
            MelonPreferences.CreateCategory("ImPlaMod", "Immobilize Player Mod");
            MelonPreferences.CreateEntry<bool>("ImPlaMod", "QuickMenuButton", true, "Put Button on Quick Menu");
            //MelonPreferences.CreateEntry<bool>("ImPlaMod", "DelayButton", true, "Delay button position on Menu");

            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            buttonEnabled = MelonPreferences.GetEntryValue<bool>("ImPlaMod", "QuickMenuButton");
            //delayButton = MelonPreferences.GetEntryValue<bool>("ImPlaMod", "DelayButton");

            if (buttonQM != null)
            {
                buttonQM.SetActive(buttonEnabled);
                //MelonLogger.Msg($"QM Button to active");
            }  
        }
    }
}
