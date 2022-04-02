using UnityEngine;
using VRC.SDKBase;
using VRC;
using System;
using MelonLoader;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;


namespace LocalLightMod
{
    public static class Utils
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                return gameObject.AddComponent<T>();
            }
            return component;
        }
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static string NumFormat(float value)
        {
            return value.ToString("F3").TrimEnd('0');
        }

        public static string RandomString(int length)
        {
            var chars = "123456789";
            var stringChars = new char[length];
            var random = new System.Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            string temp = new String(stringChars);
            //Main.Logger.Msg("RandomString" + temp);
            return temp;
        }

    }
}
