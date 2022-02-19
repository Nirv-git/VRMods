using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FLuxMod
{
    public static class Utils
    {
        public static VRCPlayer GetVRCPlayer()
        {
            return VRCPlayer.field_Internal_Static_VRCPlayer_0;
        }

        public static string NumberFormat(float value)
        {
            return value.ToString("F2").TrimEnd('0');
        }

        public static float Rescale(float value)
        {
            float converted = value / .97f;
            return converted >= 1f ? 1f : converted;
        }

        public static float Invert(float value)
        {
            return 1f - value;
        }
    }
}
