using MelonLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace IKTpresets
{
    class SaveSlots
    {

        //FixShoulders(bool), PinHipRotation(bool), DoHipShifting(bool), PreStraightenSpine(bool), StraightenNeck(bool), SpineRelaxIterations(int), MaxSpineAngleFwd(float),MaxSpineAngleBack(float), MaxNeckAngleFwd(float), MaxNeckAngleBack(float), NeckPriority(float), StraightSpineAngle(float), StraightSpinePower(float), MeasureMode(string), WingspanMeasurementAdjustFactor(float), PlantFeet(bool), HandAngleOffset(Vector3), HandPositionOffset(Vector3) ElbowGoalOffset(float); KneeGoalOffset(float); ChestGoalOffset(float);
        //      bool               bool                  bool                     bool                     bool                  int                        float                  float                float                          float               float                  float                       float                    string                    float                            bool            float, float, float            float, float, float                   float,            float,             float
        //        1                  2                     3                        4                        5                     6                          7                      8                    9                              10                  11                     12                           13                     14                          15                             16               17   18  19                    20      21   22                       23             24                       25

        public static Dictionary<int, System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float,string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>> 
            GetSaved()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;
            try
            {
                //MelonLoader.MelonLogger.Msg("Value: " + melonPref.Value);
                return new Dictionary<int, System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float,string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>>(
                    melonPref.Value.Split(';').Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]),
                    p => new System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float, string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>(
                        new System.Tuple<bool, bool, bool, bool, bool>(bool.Parse(p[1]), bool.Parse(p[2]), bool.Parse(p[3]), bool.Parse(p[4]), bool.Parse(p[5])),
                        new System.Tuple<int, float, float, float, float, float, float>(int.Parse(p[6]), float.Parse(p[7]), float.Parse(p[8]), float.Parse(p[9]), float.Parse(p[10]),
                        float.Parse(p[11]), float.Parse(p[12])),
                        new System.Tuple<float, string, float, bool, Vector3, Vector3>(float.Parse(p[13]), p[14], float.Parse(p[15]), bool.Parse(p[16]), new Vector3(float.Parse(p[17]), float.Parse(p[18]), float.Parse(p[19])),
                        new Vector3(float.Parse(p[20]), float.Parse(p[21]), float.Parse(p[22]))),
                        new System.Tuple<float, float, float>(float.Parse(p[23]), float.Parse(p[24]), float.Parse(p[25]))
                        )));
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading prefs - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,True,True,True,False,True,10,30.,30.,30.,35.,3.6,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;2,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;3,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;4,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;5,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;6,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;7,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;8,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;9,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;10,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;11,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;12,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;13,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;14,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;15,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;16,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5"; }
            return new Dictionary<int, System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float, string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>>()
            {{ 1, new System.Tuple<System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float,string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>(new System.Tuple<bool, bool, bool, bool, bool>(false, false, false, false, false), new System.Tuple<int, float, float, float, float, float, float>(0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), new System.Tuple<float, string, float, bool, Vector3, Vector3>(0.0f,"", 0.0f, false, new Vector3(0, 0, 0), new Vector3(0, 0, 0)), new System.Tuple<float, float, float>(0f,0f,0f)) } };

        }

        public static void Store(int location)
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;
            try
            {
                var updated = new System.Tuple < System.Tuple<bool, bool, bool, bool, bool>, System.Tuple<int, float, float, float, float, float, float>, System.Tuple<float,string, float, bool, Vector3, Vector3>, System.Tuple<float, float, float>>(
                    new System.Tuple<bool, bool, bool, bool, bool>(Main.FixShoulders.Value, Main.PinHipRotation.Value, Main.DoHipShifting.Value, Main.PreStraightenSpine.Value, Main.StraightenNeck.Value),
                    new System.Tuple<int, float, float, float, float, float, float>(Main.SpineRelaxIterations.Value, Main.MaxSpineAngleFwd.Value, Main.MaxSpineAngleBack.Value, Main.MaxNeckAngleFwd.Value,
                    Main.MaxNeckAngleBack.Value, Main.NeckPriority.Value, Main.StraightSpineAngle.Value), new System.Tuple<float, string, float, bool, Vector3, Vector3>(Main.StraightSpinePower.Value,
                    Main.MeasureMode.Value, Main.WingspanMeasurementAdjustFactor.Value, Main.PlantFeet.Value, Main.HandAngleOffset.Value, Main.HandPositionOffset.Value), 
                    new System.Tuple<float, float, float>(Main.ElbowGoalOffset.Value, Main.KneeGoalOffset.Value, Main.ChestGoalOffset.Value));
                var Dict = GetSaved();
                Dict[location] = updated;
                melonPref.Value = string.Join(";", Dict.Select(s => String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}", s.Key,
                    s.Value.Item1.Item1, s.Value.Item1.Item2, s.Value.Item1.Item3, s.Value.Item1.Item4, s.Value.Item1.Item5,
                    s.Value.Item2.Item1.ToString(), s.Value.Item2.Item2.ToString("F5").TrimEnd('0'), s.Value.Item2.Item3.ToString("F5").TrimEnd('0'), s.Value.Item2.Item4.ToString("F5").TrimEnd('0'),
                    s.Value.Item2.Item5.ToString("F5").TrimEnd('0'), s.Value.Item2.Item6.ToString("F5").TrimEnd('0'), s.Value.Item2.Item7.ToString("F5").TrimEnd('0'),
                    s.Value.Item3.Item1.ToString("F5").TrimEnd('0'), s.Value.Item3.Item2, s.Value.Item3.Item3.ToString("F5").TrimEnd('0'), s.Value.Item3.Item4,
                    s.Value.Item3.Item5.x.ToString("F5").TrimEnd('0'), s.Value.Item3.Item5.y.ToString("F5").TrimEnd('0'), s.Value.Item3.Item5.z.ToString("F5").TrimEnd('0'),
                    s.Value.Item3.Item6.x.ToString("F5").TrimEnd('0'), s.Value.Item3.Item6.y.ToString("F5").TrimEnd('0'), s.Value.Item3.Item6.z.ToString("F5").TrimEnd('0'),
                     s.Value.Item4.Item1.ToString("F5").TrimEnd('0'), s.Value.Item4.Item2.ToString("F5").TrimEnd('0'), s.Value.Item4.Item3.ToString("F5").TrimEnd('0')
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
                Main.FixShoulders.Value = Dict[location].Item1.Item1;
                Main.PinHipRotation.Value = Dict[location].Item1.Item2;
                Main.DoHipShifting.Value = Dict[location].Item1.Item3;
                Main.PreStraightenSpine.Value = Dict[location].Item1.Item4;
                Main.StraightenNeck.Value = Dict[location].Item1.Item5;
                Main.SpineRelaxIterations.Value = Dict[location].Item2.Item1;
                Main.MaxSpineAngleFwd.Value = Dict[location].Item2.Item2;
                Main.MaxSpineAngleBack.Value = Dict[location].Item2.Item3;
                Main.MaxNeckAngleFwd.Value = Dict[location].Item2.Item4;
                Main.MaxNeckAngleBack.Value = Dict[location].Item2.Item5;
                Main.NeckPriority.Value = Dict[location].Item2.Item6;
                Main.StraightSpineAngle.Value = Dict[location].Item2.Item7;
                Main.StraightSpinePower.Value = Dict[location].Item3.Item1;
                Main.MeasureMode.Value = Dict[location].Item3.Item2;
                Main.WingspanMeasurementAdjustFactor.Value = Dict[location].Item3.Item3;
                Main.PlantFeet.Value = Dict[location].Item3.Item4;
                Main.HandAngleOffset.Value = Dict[location].Item3.Item5;
                Main.HandPositionOffset.Value = Dict[location].Item3.Item6;
                Main.ElbowGoalOffset.Value = Dict[location].Item4.Item1;
                Main.KneeGoalOffset.Value = Dict[location].Item4.Item2;
                Main.ChestGoalOffset.Value = Dict[location].Item4.Item3;
                MelonPreferences.Save();
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading prefs from slot {location}\n" + ex.ToString()); }
        }

        //Slot names
        //"1,Slot 1;2,Slot 2;3,Slot 3;4,Slot 4;5,Slot 5;6,Slot 6"
        public static Dictionary<int, string> GetSavedSlotNames()
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefNames;
            try
            {
                //Main.Logger.Msg("Value: " + melonPref.Value);
                return new Dictionary<int, string>(melonPref.Value.Split(';').Select(s => s.Split(',')).ToDictionary(p => int.Parse(p[0]), p => p[1]));
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error loading slot names - Resetting to Defaults:\n" + ex.ToString()); melonPref.Value = "1,N/A;2,N/A;3,N/A;4,N/A;5,N/A;6,N/A;7,N/A;8,N/A;9,N/A;10,N/A;11,N/A;12,N/A;13,N/A;14,N/A;15,N/A;16,N/A"; }
            return new Dictionary<int, string>() { { 1, "Error" } };

        }

        public static void StoreSlotNames(int location, string updated)
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefNames;
            try
            {
                var Dict = GetSavedSlotNames();
                Dict[location] = updated;
                melonPref.Value = string.Join(";", Dict.Select(s => String.Format("{0},{1}", s.Key, s.Value)));
                Main.cat.SaveToFile();
            }
            catch (System.Exception ex) { Main.Logger.Error($"Error storing new saved slot names - \n" + ex.ToString()); }
        }

        public static void MigrateData() 
        {
            MelonPreferences_Entry<string> melonPref = Main.savedPrefs;

            switch (melonPref.Value.Split(';')[0].Split(',').Length)
            {
                case 23 : Main.Logger.Msg($"MigrateData - 23 Elements Found - Migrating Data"); Main.Logger.Msg($"Current saved value before migration: {melonPref.Value}");
                    melonPref.Value = melonPref.Value.Replace(";", ",0.1,0.1,0.5;") + ",0.1,0.1,0.5";
                    break; //23 Example data - "1,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;2,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;3,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;4,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;5,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;6,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;7,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;8,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;9,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;10,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;11,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;12,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;13,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;14,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;15,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0;16,true,true,true,false,true,10,30,30,30,15,2,15,2,ImprovedWingspan,1.1,false,0,-105,0,0.015,-0.005,0"
                
                case 26: Main.Logger.Msg($"MigrateData - 26 Elements Found - Current version"); 
                    break;
                
                default: Main.Logger.Msg($"MigrateData Default case, data is corrupt - Resetting to default"); Main.Logger.Msg($"Current saved value before reset: {melonPref.Value}");
                    melonPref.Value = "1,True,True,True,False,True,10,30.,30.,30.,35.,3.6,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;2,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;3,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;4,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;5,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;6,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;7,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;8,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;9,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;10,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;11,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;12,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;13,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;14,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;15,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5;16,True,True,True,False,True,10,30.,30.,30.,15.,2.,15.,2.,ImprovedWingspan,1.1,False,0.,-105.,0.,0.015,-0.005,0.,0.1,0.1,0.5";
                    break;
            }
        }
    }
}
