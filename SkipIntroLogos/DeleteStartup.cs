using System;
using System.Linq;
using System.Threading;
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
        private static int _startupRoutineLogged;

        public override void Load()
        {
            _log = Log;
            _harmony = new Harmony(PluginGuid);
            var controller = typeof(StartupExperienceController);
            RegisterPrefix(controller, nameof(StartupExperienceController.PlayStartupExperience), nameof(SkipStartupExperience),
                "StartupExperienceController.PlayStartupExperience()");
            RegisterPrefix(controller, nameof(StartupExperienceController.PlayStartupExperience_SkipSplash), nameof(LogSkipSplash),
                "StartupExperienceController.PlayStartupExperience_SkipSplash()");

            // The generated IL2CPP interop assembly sanitizes the native nested type name.
            var startupRoutine = AccessTools.Inner(controller, "_StartupExperienceRoutine_d__20");
            RegisterPrefix(startupRoutine, "MoveNext", nameof(LogStartupRoutine),
                "StartupExperienceController.<StartupExperienceRoutine>d__20.MoveNext()");

            Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
        }

        private void RegisterPrefix(Type targetType, string targetName, string prefixName, string targetLabel)
        {
            try
            {
                if (targetType == null)
                {
                    Log.LogError($"Harmony Prefix registration failed for {targetLabel}. Target type not found.");
                    return;
                }

                var target = AccessTools.DeclaredMethod(targetType, targetName, Type.EmptyTypes);
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

        private static void LogStartupRoutine()
        {
            if (Interlocked.Exchange(ref _startupRoutineLogged, 1) == 0)
            {
                _log.LogInfo("StartupExperienceController.<StartupExperienceRoutine>d__20.MoveNext() Prefix fired (diagnostic 3).");
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
