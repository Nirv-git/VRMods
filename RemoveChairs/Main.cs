using System;
using System.Linq;
using System.Collections;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using VRC.SDKBase;
using Boo.Lang;

[assembly: MelonInfo(typeof(RemoveChairs.Main), "RemoveChairs", "1.34", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]


namespace RemoveChairs
{
    public class Main : MelonMod
    {
        public override void OnApplicationStart()
        {

            var toggleChairsMenu = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.WorldMenu).AddSimpleButton("Toggle Chairs", () => toggleChairsMenu.Show());
            toggleChairsMenu.AddSimpleButton("Disable Active Chairs", (() => {
                int countChange = 0;
                var objects = Resources.FindObjectsOfTypeAll<VRCStation>();
                foreach (var item in objects)
                {
                    if (item.gameObject.active) //Only disable active chairs
                    {
                        countChange++;
                        objectsDisabled.Add(item);
                        item.gameObject.SetActive(false); // item.gameObject finds the parent gameObject of the VRCStation 
                    }
                }
                MelonLogger.Log("Disabled " + countChange + " chair objects");
            }));
            toggleChairsMenu.AddSimpleButton("Re-enable Chairs", (() => {
                int countChange = 0;
                foreach (var item in objectsDisabled)
                {
                    if (item is null) continue;
                    countChange++;
                    item.gameObject.SetActive(true);
                }
                MelonLogger.Log("Enabled " + countChange + " chair objects");
                objectsDisabled.Clear();
            }));
            toggleChairsMenu.AddSimpleButton("Close", () => toggleChairsMenu.Hide());
        }

  

        List<VRCStation> objectsDisabled = new List<VRCStation>();

        public override void OnLevelWasLoaded(int level)
        {
            
            switch (level)//Without switch this would run 3 times at world load
            {
                case 0: //App
                case 1: //ui
                    break;
                default:
                    objectsDisabled.Clear(); //Clear the list if we change worlds 
                    break;
            }
        }


    }
}