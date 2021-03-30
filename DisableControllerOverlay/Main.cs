using UIExpansionKit.API;
using MelonLoader;
using UnityEngine;


[assembly: MelonModInfo(typeof(DisableControllerOverlay.DisableControllerOverlayMod), "DisableControllerOverlay", "0.9.1", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]

namespace DisableControllerOverlay
{

    public class DisableControllerOverlayMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            var toolTipsMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Controller Tooltips", () => toolTipsMenu.Show());

            toolTipsMenu.AddLabel("Enable/Disable the Controller Tooltips that sometimes get stuck on!");
            toolTipsMenu.AddSimpleButton("Disable Tooltips", (() => ToggleToolTips(false)));
            toolTipsMenu.AddSimpleButton("Enable Tooltips", (() => ToggleToolTips(true)));
            toolTipsMenu.AddSimpleButton("Close", () => toolTipsMenu.Hide());

        }


        private void ToggleToolTips(bool value)
        {

            GameObject ConLeft = GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (left)");
            if (ConLeft != null)
            {
                for (int i = 0; i < ConLeft.transform.childCount; i++)
                {
                    GameObject child = ConLeft.transform.GetChild(i).gameObject;
                    if (child.name.StartsWith("ControllerUI") && child.name.EndsWith("(Clone)"))
                    {
                        MelonModLogger.Log(child.name + "Set to: " + value);
                        child.SetActive(value);
                    }
                }
            }
            GameObject ConRight = GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Controller (right)");
            if (ConRight != null)
            {
                for (int i = 0; i < ConRight.transform.childCount; i++)
                {
                    GameObject child = ConRight.transform.GetChild(i).gameObject;
                    if (child.name.StartsWith("ControllerUI") && child.name.EndsWith("(Clone)"))
                    {
                        MelonModLogger.Log(child.name + "Set to: " + value);
                        child.SetActive(value);
                    }
                }
            }
          
        }







    }
}

