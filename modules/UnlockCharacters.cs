using HarmonyLib;
using Henley.Scene;
using Henley.UI;

namespace grimbahack_hxh.modules;

[HarmonyPatch(typeof(CharaSelectStartup), nameof(CharaSelectStartup.StartupScene))]
public class UnlockCharacters
{
    public static void Prefix()
    {
        CharaSelectSettings.UnlockIcon(7);
        CharaSelectSettings.UnlockIcon(11);
    }
}
