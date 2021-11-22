using MelonLoader;
using UnityEngine;
using System.Collections;
using System;

[assembly: MelonModInfo(typeof(QMVolumePercent.QMVolumePercentMod), "QMVolumePercentMod", "0.2", "Nirvash")]
[assembly: MelonModGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Yellow)]

namespace QMVolumePercent
{
    public class QMVolumePercentMod : MelonMod
    {
        private static GameObject AudioPage;
        private static Color textColor = Color.white;
        private static string[] sliders = new string[] { "VolumeSlider_World", "VolumeSlider_Voices", "VolumeSlider_Avatars" };

        private const string catagory = "QMVolumePercent";
        public static MelonPreferences_Category cat;
        public static MelonPreferences_Entry<int> textR;
        public static MelonPreferences_Entry<int> textG;
        public static MelonPreferences_Entry<int> textB;

        public override void OnApplicationStart()
        {
            cat = MelonPreferences.CreateCategory(catagory, "QMVolumePercent Settings");
            textR = MelonPreferences.CreateEntry(catagory, nameof(textR), 100, "Text Red (0-100)");
            textG = MelonPreferences.CreateEntry(catagory, nameof(textG), 100, "Text Green (0-100)");
            textB = MelonPreferences.CreateEntry(catagory, nameof(textB), 100, "Text Blue (0-100)");
            MelonCoroutines.Start(OnLoad());
            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            var oldColor = textColor;
            textColor = new Color(Utils.Clamp(textR.Value, 0, 100) / 100f, Utils.Clamp(textG.Value, 0, 100) / 100f, Utils.Clamp(textB.Value, 0, 100) / 100f);
            if ((!AudioPage?.Equals(null) ?? false) && oldColor != textColor)
                SetColor();
        }

        public static IEnumerator OnLoad()
        {
            MelonLogger.Msg($"Init");
            while (GameObject.Find("/UserInterface")?.transform.Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_AudioSettings/Content/Audio/VolumeSlider_World/") == null)
                yield return new WaitForSeconds(1f);
            AudioPage = GameObject.Find("/UserInterface").transform.Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_AudioSettings/").gameObject;

            foreach (var slider in sliders)
            {
                var textObj = AudioPage.transform.Find("Content/Audio/" + slider + "/Text_QM_H4 (1)").gameObject;
                var textObjParent = textObj.transform.parent;
                textObj.transform.SetParent(null);
                textObj.transform.SetParent(textObjParent);
                UnityEngine.Object.Destroy(textObj.GetComponent<VRC.UI.Core.Styles.StyleElement>());
                textObj.transform.localPosition = new Vector3(100, 14, 0); //Something to do with anchoredPos causes this to offset to 84 14 0 when opening the menu... hmmm
                textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-348, -18); //Make sure all are consistant, Avatars has a different one
                textObj.SetActive(true);
            }
            SetColor();
        }

        private static void SetColor()
        {
            foreach (var slider in sliders)
            {
                var textObj = AudioPage.transform.Find("Content/Audio/" + slider + "/Text_QM_H4 (1)").gameObject;
                textObj.GetComponent<TMPro.TextMeshProUGUI>().color = textColor;
            }
        }
    }

    public static class Utils
    {
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
