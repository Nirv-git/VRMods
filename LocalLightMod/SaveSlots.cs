using MelonLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace LocalLightMod
{
    class SaveSlots
    {
        //Data will look like 1,5.666,5344.55,343.56;
        public static Dictionary<int, System.Tuple<float, float, float>> GetSavedColors()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedColors;
            try
            {
                //Main.Logger.Msg("Value: " + melonPref.Value);
                return new Dictionary<int, System.Tuple<float, float, float>>(melonPref.Value.Split(';').Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]), p => new System.Tuple<float, float, float>(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]))));
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading saved colors - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,0.0,0.0,0.0;2,0.0,0.0,0.0;3,0.0,0.0,0.0;4,0.0,0.0,0.0;5,0.0,0.0,0.0;6,0.0,0.0,0.0"; }
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
            catch (System.Exception ex) { Main.Logger.Error($"Error storing new saved color\n" + ex.ToString()); }
        }

        //pickupOrient;pickupable;lightType;lightRange;lightSpotAngle;lightColor;lightIntensity;lightBounceIntensity;lightShadows;lightShadowStr;name;hideMeshRender;
        //bool(string),bool(string),LightType(string),float,float,Color(float, float, float),Float,Float,LightShadows(string),float,string,bool    
        //String Format 1, 0,0, 1,                     0.0,0.0,    0.0,0.0,0.0,               0.0,0.0,    1,                   ,0.0   ""    
        //              1   2   3                       4  5        6   7   8                  9   10     11                    12     13   14   
        //1,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A;2.. ect

        public static Dictionary<int, (bool, bool, LightType, float, float, Color, float, float, LightShadows, float, string, bool)> GetSavedPrefs()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;//type ? Main.savedPos : Main.savedRot;
            try
            {
                return new Dictionary<int, (bool, bool, LightType, float, float, Color, float, float, LightShadows, float, string, bool)>(melonPref.Value.Split(';')
                    .Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]), p =>
                    (Boolean.Parse(p[1]), Boolean.Parse(p[2]), (p[3] == "Directional" ? LightType.Directional : (p[3] == "Point" ? LightType.Point : LightType.Spot)),
                    float.Parse(p[4]), float.Parse(p[5]), new Color(float.Parse(p[6]), float.Parse(p[7]), float.Parse(p[8])), float.Parse(p[9]), float.Parse(p[10]),
                    (p[11] == "Hard" ? LightShadows.Hard : (p[11] == "None" ? LightShadows.None : LightShadows.Soft)), float.Parse(p[12]), p[13], Boolean.Parse(p[14]))));
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading saved LightPrefs - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;2,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;3,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;4,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False;5,False,True,Point,10.,30.,1,1,1,1.,1.,Hard,1.,N/A,False"; }
            return new Dictionary<int, (bool, bool, LightType, float, float, Color, float, float, LightShadows, float, string, bool)>() { { 1, (false, true, LightType.Point, 10f, 30f, Color.white, 1f, 1f, LightShadows.Hard, 1f, "ERROR", false) } };
            ;
        }

        public static void StorePrefs(int location, (bool, bool, LightType, float, float, Color, float, float, LightShadows, float, string, bool) updated)
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;
            try
            {
                var Dict = GetSavedPrefs();
                Dict[location] = updated;
                melonPref.Value = string.Join(";", Dict.Select(s => String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", s.Key,
                    s.Value.Item1.ToString(), s.Value.Item2.ToString(), s.Value.Item3, s.Value.Item4.ToString("F5").TrimEnd('0'), s.Value.Item5.ToString("F5").TrimEnd('0'),
                    s.Value.Item6.r.ToString(), s.Value.Item6.g.ToString(), s.Value.Item6.b.ToString(),
                    s.Value.Item7.ToString("F5").TrimEnd('0'), s.Value.Item8.ToString("F5").TrimEnd('0'), s.Value.Item9,
                    s.Value.Item10.ToString("F5").TrimEnd('0'), s.Value.Item11, s.Value.Item12.ToString())));
                //Main.Logger.Msg("Value: " + melonPref.Value);
                Main.cat.SaveToFile();
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error storing new saved light values:\n" + ex.ToString()); }
        }


        public static void LoadDefaultSlot()
        {
            if (GetSavedPrefs().TryGetValue(1, out var slot))
            {
                Main.Config.name = slot.Item11;
                Main.Config.pickupOrient = slot.Item1;
                Main.Config.pickupable = slot.Item2;
                Main.Config.lightType = slot.Item3;
                Main.Config.lightRange = slot.Item4;
                Main.Config.lightSpotAngle = slot.Item5;
                Main.Config.lightColor = slot.Item6;
                Main.Config.lightIntensity = slot.Item7;
                Main.Config.lightBounceIntensity = slot.Item8;
                Main.Config.lightShadows = slot.Item9;
                Main.Config.lightShadowStr = slot.Item10;
                Main.Config.hideMeshRender = slot.Item12;
            }
        }

    }
}
