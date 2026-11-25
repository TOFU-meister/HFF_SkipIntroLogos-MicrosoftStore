using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
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
        private static ManualLogSource _log;

        public override void Load()
        {
            _log = Log;
            _harmony = new Harmony(PluginGuid);
            RegisterPrefix(nameof(StartupExperienceController.PlayStartupExperience), nameof(SkipStartupExperience));
            RegisterPrefix(nameof(StartupExperienceController.PlayStartupExperience_SkipSplash), nameof(LogSkipSplash));

            Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private void RegisterPrefix(string targetName, string prefixName)
        {
            string targetLabel = $"StartupExperienceController.{targetName}()";

            try
            {
                var target = AccessTools.DeclaredMethod(typeof(StartupExperienceController), targetName, Type.EmptyTypes);
                if (target == null)
                {
                    Log.LogError($"Harmony Prefix registration failed for {targetLabel}. Target method not found.");
                    return;
                }

                var prefix = AccessTools.DeclaredMethod(typeof(DeleteStartup), prefixName);
                _harmony.Patch(target, prefix: new HarmonyMethod(prefix));

                if (Harmony.GetPatchInfo(target)?.Prefixes.Any(patch =>
                    patch.owner == PluginGuid && patch.PatchMethod == prefix) == true)
                {
                    Log.LogInfo($"Harmony Prefix registered for {targetLabel}.");
                }
                else
                {
                    Log.LogError($"Harmony Prefix registration could not be verified for {targetLabel}.");
                }
            }
            catch (Exception exception)
            {
                Log.LogError($"Harmony Prefix registration failed for {targetLabel}. {exception}");
            }
        }

        private static void LogSkipSplash()
        {
            _log.LogInfo("StartupExperienceController.PlayStartupExperience_SkipSplash() Prefix fired.");
        }

        private static bool SkipStartupExperience(StartupExperienceController __instance)
        {
            _log.LogInfo("StartupExperienceController.PlayStartupExperience() Prefix fired.");

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
