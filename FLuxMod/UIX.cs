using System;
using System.Collections.Generic;
using UIExpansionKit.API;

namespace FLuxMod
{
    class UIX
    {
        public static void UIXinit()
        {
            ExpansionKitApi.RegisterSettingAsStringEnum("FLuxMod",
                nameof(Main.amapi_ModsFolder),
                new[]
                {
                    ("ModsFolder", "Mods Folder"),
                    ("Options", "Options"),
                    ("Main", "Main"),
                    ("Expression", "Expression"),
                });

        }
    }
}
