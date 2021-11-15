using MelonLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace CameraFlashMod
{
    class SaveSlots
    {
        //Data will look like 1,5.666,5344.55,343.56;
        public static Dictionary<int, System.Tuple<float, float, float>> GetSavedColors()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedColors;
            try
            {
                //MelonLoader.MelonLogger.Msg("Value: " + melonPref.Value);
                return new Dictionary<int, System.Tuple<float, float, float>>(melonPref.Value.Split(';').Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]), p => new System.Tuple<float, float, float>(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]))));
            }
            catch (System.Exception ex) { MelonLoader.MelonLogger.Error($"Error loading saved colors - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,0.0,0.0,0.0;2,0.0,0.0,0.0;3,0.0,0.0,0.0;4,0.0,0.0,0.0;5,0.0,0.0,0.0;6,0.0,0.0,0.0"; }
            return new Dictionary<int, System.Tuple<float, float, float>>() { { 1, new System.Tuple<float, float, float>(999.999f, 999.999f, 999.999f) } };
        }

        public static void Store(int location, System.Tuple<float, float, float> updated)
        {
            MelonPreferences_Entry<string> melonPref = Main.savedColors;
            try
            {
                var Dict = GetSavedColors();
                Dict[location] = updated;
                melonPref.Value = string.Join(";", Dict.Select(s => String.Format("{0},{1},{2},{3}", s.Key, s.Value.Item1.ToString("F5").TrimEnd('0'), s.Value.Item2.ToString("F5").TrimEnd('0'), s.Value.Item3.ToString("F5").TrimEnd('0'))));
                Main.cat.SaveToFile();
            }
            catch (System.Exception ex) { MelonLoader.MelonLogger.Error($"Error storing new saved color\n" + ex.ToString()); }
        }

    }
}
