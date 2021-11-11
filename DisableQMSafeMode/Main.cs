using MelonLoader;
using UnityEngine;
using System.Collections;
using System;

[assembly: MelonModInfo(typeof(DisableQMSafeMode.DisableQMSafeModeMod), "DisableQMSafeMode", "0.1", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace DisableQMSafeMode
{
    public class DisableQMSafeModeMod : MelonMod
    {
        private static GameObject butt;

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(OnLoad());
        }

        public static IEnumerator OnLoad()
        {
            MelonLogger.Msg($"Init");
            var path = "UserInterface/Canvas_QuickMenu(Clone)/Container/Window/Toggle_SafeMode";
            //while (GameObject.Find(path) == null)
            while (GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/Toggle_SafeMode") == null)
                yield return new WaitForSeconds(1f);
            butt = GameObject.Find("/UserInterface").transform.Find("Canvas_QuickMenu(Clone)/Container/Window/Toggle_SafeMode").gameObject;
            Toggle();
        }

        private static void Toggle()
        {
            MelonLogger.Msg($"Disabling Panic Button on QM");
            butt.SetActive(false);
        }
    }
}

