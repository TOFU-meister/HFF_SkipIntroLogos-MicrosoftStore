using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SkipIntroLogos
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class DeleteStartup : BasePlugin
    {
        public const string PluginGuid = "org.bepinex.plugins.humanfallflat.skipintrologos.microsoftstore";
        public const string PluginName = "Skip Intro Logos - Microsoft Store";
        public const string PluginVersion = "0.1.0-pre";

        private Harmony _harmony;

        public override void Load()
        {
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(DeleteStartup));

            Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        [HarmonyPatch(typeof(StartupExperienceController), nameof(StartupExperienceController.PlayStartupExperience))]
        [HarmonyPrefix]
        private static bool SkipStartupExperience(StartupExperienceController __instance)
        {
            if (__instance == null)
            {
                return true;
            }

            __instance.SkipStartupExperience(null);

            if (MenuCameraEffects.instance != null)
            {
                MenuCameraEffects.instance.BlackOut();
            }

            __instance.FadeOutDim();

            return false;
        }
    }
}
