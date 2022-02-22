using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using VRC.Core;
using SIDictionary = System.Collections.Generic.Dictionary<string, bool>;

namespace ImmobilizePlayer
{
    class RiskFunct
    {//I borrowed from https://github.com/Adnezz/VoiceFalloffOverride/blob/f1e6d300b0997e139e0bb616f32f8a9f7752f350/Utilities.cs#L42
        //Borrowed parts from https://github.com/loukylor/VRC-Mods/blob/main/VRChatUtilityKit/Utilities/VRCUtils.cs
        //And also https://github.com/Psychloor/PlayerRotater/blob/master/PlayerRotater/Utilities.cs
        //And then myself https://raw.githubusercontent.com/Nirv-git/SeatMod/main/RiskFunct.cs

        private static bool alreadyCheckingWorld;
        private static SIDictionary checkedWorlds = new SIDictionary();

        internal static System.Collections.IEnumerator CheckWorld()
        {
            if (alreadyCheckingWorld)
            {
                Main.Logger.Error("Attempted to check for world multiple times");
                yield break;
            }

            alreadyCheckingWorld = true;
            Main.CurrentWorldChecked = true;

            // Wait for RoomManager to exist before continuing.
            ApiWorld currentWorld = null;
            while (currentWorld == null)
            {
                currentWorld = RoomManager.field_Internal_Static_ApiWorld_0;
                yield return new WaitForSecondsRealtime(1);
            }
            var worldId = currentWorld.id;
            //Main.Logger.Error($"Checking World with Id {worldId}");

            // Check cache for world, so we keep the number of API calls lower.
            if (checkedWorlds.TryGetValue(worldId, out bool outres))
            {
                Main.WorldTypeGame = outres;
                //Main.Logger.Msg($"Using cached check {Main.WorldTypeGame} for world '{worldId}'");
                alreadyCheckingWorld = false;
                yield break;
            }

            // Check for Game Objects first, as it's the lowest cost check.
            if (GameObject.Find("eVRCRiskFuncEnable") != null || GameObject.Find("UniversalRiskyFuncEnable") != null || GameObject.Find("ModCompatRiskyFuncEnable ") != null)
            {
                Main.WorldTypeGame = false;
                checkedWorlds.Add(worldId, false);
                alreadyCheckingWorld = false;
                //Main.Logger.Msg($"GameObject allowed for world '{worldId}'");
                yield break;
            }

            // Check if whitelisted from EmmVRC - thanks Emilia and the rest of EmmVRC Staff
            var uwr = UnityWebRequest.Get($"https://dl.emmvrc.com/riskyfuncs.php?worldid={worldId}");
            uwr.SendWebRequest();
            while (!uwr.isDone)
                yield return new WaitForEndOfFrame();

            var result = uwr.downloadHandler.text?.Trim().ToLower();
            uwr.Dispose();
            if (!string.IsNullOrWhiteSpace(result))
            {
                switch (result)
                {
                    case "allowed":
                        Main.WorldTypeGame = false;
                        checkedWorlds.Add(worldId, false);
                        alreadyCheckingWorld = false;
                        //Main.Logger.Msg($"EmmVRC allows world '{worldId}'");
                        yield break;
                }
            }

            // Check tags then. should also be in cache as it just got downloaded
            API.Fetch<ApiWorld>(
                worldId,
                new Action<ApiContainer>(
                    container =>
                    {
                        ApiWorld apiWorld;
                        if ((apiWorld = container.Model.TryCast<ApiWorld>()) != null)
                        {
                            bool tagResult = false;
                            foreach (var worldTag in apiWorld.tags)
                            {
                                if (worldTag.IndexOf("game", StringComparison.OrdinalIgnoreCase) != -1 && worldTag.IndexOf("games", StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    tagResult = true;
                                    //Main.Logger.Msg($"Found game tag in world world '{worldId}'");
                                    break;
                                }
                            }
                            Main.WorldTypeGame = tagResult;
                            checkedWorlds.Add(worldId, tagResult);
                            alreadyCheckingWorld = false;
                            //Main.Logger.Msg($"Tag search result: '{tagResult}' for '{worldId}'");
                        }
                        else
                        {
                            Main.Logger.Error("Failed to cast ApiModel to ApiWorld");
                        }
                    }),
                disableCache: false);
        }
    }
}
