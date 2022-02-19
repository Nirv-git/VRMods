using MelonLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace FLuxMod
{
    class SaveSlots
    { 
                    //Main.flux_HDRClamp.Value = .778f;
                    //Main.flux_Hue.Value = .102f;
                    //Main.flux_Colorize.Value = .25f;
                    //Main.flux_Brightness.Value = .623f;
                    //Main.flux_Desat.Value = .255f;

        public static Dictionary<int, System.Tuple<float, float, float, float, float>>GetSaved()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;
            try
            {
                //MelonLoader.MelonLogger.Msg("Value: " + melonPref.Value);
                return new Dictionary<int, System.Tuple<float, float, float, float, float>>(
                    melonPref.Value.Split(';').Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]),
                    p => new System.Tuple<float, float, float, float, float>(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]), float.Parse(p[4]), float.Parse(p[5]))
                        ));
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading prefs - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,0.222,0.102,0.75,0.623,0.255;2,0,0.102,0,1,0;3,0.5,0.102,0,1,0;4,0.5,0.102,0,0.75,0.15;5,0.5,0.102,0,0.10,0.25;6,0.222,0.102,0.75,0.623,0.255"; }
            return new Dictionary<int, System.Tuple<float, float, float, float, float>>()
            {{ 1, new System.Tuple<float, float, float, float, float>(0f,0f,0f,0f,0f) } };

        }

        public static void Store(int location)
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;
            try
            {
                var updated = new System.Tuple<float, float, float, float, float>(Main.flux_HDRClamp.Value, Main.flux_Hue.Value, Main.flux_Colorize.Value, Main.flux_Brightness.Value,
                    Main.flux_Desat.Value);
                var Dict = GetSaved();
                Dict[location] = updated;
                melonPref.Value = string.Join(";", Dict.Select(s => String.Format("{0},{1},{2},{3},{4},{5}", s.Key,
                    s.Value.Item1.ToString("F5").TrimEnd('0'), s.Value.Item2.ToString("F5").TrimEnd('0'), s.Value.Item3.ToString("F5").TrimEnd('0'),
                    s.Value.Item4.ToString("F5").TrimEnd('0'), s.Value.Item5.ToString("F5").TrimEnd('0')     
                )));
                Main.cat.SaveToFile();
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error storing new saved pref\n" + ex.ToString()); }
        }

        public static void LoadSlot(int location)
        {
            try
            {
                var Dict = GetSaved();
                Main.pauseOnValueChange = true;
                Main.flux_HDRClamp.Value = Dict[location].Item1;
                Main.flux_Hue.Value = Dict[location].Item2;
                Main.flux_Colorize.Value = Dict[location].Item3;
                Main.flux_Brightness.Value = Dict[location].Item4;
                Main.flux_Desat.Value = Dict[location].Item5;
                Main.pauseOnValueChange = false;
                Main.OnValueChange(0f, 0f);
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading prefs from slot {location}\n" + ex.ToString()); }
        }

    }
}
