using UIExpansionKit.API;
using UIExpansionKit.Components;
using MelonLoader;
using UnityEngine;
using VRC.SDKBase;


[assembly: MelonModInfo(typeof(PlayerSpeedAdjSlower.PlayerSpeedAdjSlowerMod), "PlayerSpeedAdjSlower", "1.33", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]

//_originalWalkSpeed = locoCtrl.field_Public_Single_2;
//_originalRunSpeed = locoCtrl.field_Public_Single_0;
//_originalStrafeSpeed = locoCtrl.field_Public_Single_1;

namespace PlayerSpeedAdjSlower
{

    public class PlayerSpeedAdjSlowerMod : MelonMod
    {
        float PlayerSpeedValue = 2f;
        public override void OnApplicationStart()
        {
            
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Adj Speed", ShowAdjSpeedMenu);
            //ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.AvatarMenu, "Speed Default", (() => ChangePlayerSpeed(2f)));
            //ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.AvatarMenu, "Speed 1/2", (() => ChangePlayerSpeed(1f)));
            //ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.AvatarMenu, "Speed 1/10", (() => ChangePlayerSpeed(.2f)));
            // ExpansionKitApi.RegisterSimpleMenuButton(ExpandedMenu.SettingsMenu, "Speed 1/100", (() => ChangePlayerSpeed(.02f))); //Breaks game, can't move at all
            MelonModLogger.Log("Slow speed Adjust Init");
        }

        private void ShowAdjSpeedMenu()
        {

            var adjSpeedMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);

            adjSpeedMenu.AddLabel("Adjust your movement speed");
            adjSpeedMenu.AddSimpleButton("Speed 2", (() => ChangePlayerSpeed(4f)));
            adjSpeedMenu.AddSimpleButton("Speed 1.5", (() => ChangePlayerSpeed(3f)));
            adjSpeedMenu.AddSimpleButton("Speed Default", (() => ChangePlayerSpeed(2f)));
            adjSpeedMenu.AddSimpleButton("Speed 1/2", (() => ChangePlayerSpeed(1f)));
            adjSpeedMenu.AddSimpleButton("Speed 1/4", (() => ChangePlayerSpeed(.5f)));
            adjSpeedMenu.AddSimpleButton("Speed 1/10", (() => ChangePlayerSpeed(.2f)));
            adjSpeedMenu.AddSimpleButton("Close", () => adjSpeedMenu.Hide());

            adjSpeedMenu.Show();
        }

        internal static VRCPlayerApi LocalPlayerApi
        {
            get
            {
                if (_vpalocal == null)
                {
                    _vpalocal = VRCPlayerApi.GetPlayerByGameObject(VRCPlayer.field_Internal_Static_VRCPlayer_0.gameObject);
                    //MelonModLogger.Log("vpalocal if: " + _vpalocal);
                }
                //MelonModLogger.Log("vpalocal return: " + _vpalocal);
                return _vpalocal;
            }
        }
        private static VRCPlayerApi _vpalocal;
        public override void OnLevelWasLoaded(int level)
        {
            _vpalocal = null;
            MelonModLogger.Log("vpalocal set to null - World Change");
        }

        private void ChangePlayerSpeed(float value)
        {
            MelonModLogger.Log("Orig walk speed: " + LocalPlayerApi.GetWalkSpeed());
            PlayerSpeedValue = value;
            LocalPlayerApi.SetWalkSpeed(value);
            LocalPlayerApi.SetRunSpeed(value * 2);
            MelonCoroutines.Start(runPlaySpeedChangeLoop());
            MelonModLogger.Log("Speed walk adjusted: " + value);
        }

        System.Collections.IEnumerator runPlaySpeedChangeLoop()
        {
            MelonModLogger.Log("Set walk speed to: " + PlayerSpeedValue + " every 30 sec for 5 minutes");
            int count = 10;
            while (count > 0)
            {
                yield return new WaitForSecondsRealtime(30);
                LocalPlayerApi.SetWalkSpeed(PlayerSpeedValue);
                LocalPlayerApi.SetRunSpeed(PlayerSpeedValue * 2);
                count--;
            }


        }
    }
}
