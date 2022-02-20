using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmobilizePlayer
{
    public static class Utils
    {
        public static bool LocalPlayerFBT()
        {
            var IKControllerEnum = VRCPlayer.field_Internal_Static_VRCPlayer_0?.field_Private_VRC_AnimationController_0?.field_Private_IkController_0?.prop_IkType_0;
            var hasfbt = IKControllerEnum?.HasFlag(IkController.IkType.SixPoint) ?? false;
            return hasfbt;
        }

        public static bool MenuOpen()
        { //Thanks to https://github.com/RequiDev/ReMod.Core/blob/cdd7e84c65e2c23933576e92f7134aec2f52c0bf/VRChat/VRCUiManagerEx.cs#L5
            if (Main._uiManagerInstance == null)
                return true;
            return Main._uiManagerInstance.field_Private_Boolean_0;
        }

        public static void SetImmobilize(bool value)
        {
            VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.Immobilize(value);
        }

    }
}
