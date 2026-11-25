using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using StartupRoutine = StartupExperienceController._StartupExperienceRoutine_d__20;

namespace SkipIntroLogos
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class DeleteStartup : BasePlugin
    {
        public const string PluginGuid = "org.bepinex.plugins.humanfallflat.skipintrologos.microsoftstore";
        public const string PluginName = "Skip Intro Logos - Microsoft Store";
        public const string PluginVersion = "0.1.0-pre";

        // Resume labels in the verified native MoveNext jump table.
        private const int WaitingForFirstCameraTransition = 1;
        private const int WaitingForCurveLogo = 2;
        private const int AfterHumanLogoDelay = 7;
        private const double PressAnythingDronesTime = 13.2;

        private static ManualLogSource _log;
        private static int _skipLogged;
        private static int _failureLogged;
        private static StartupExperienceUI _skippedStartupUi;

        public override void Load()
        {
            _log = Log;
            try
            {
                if (!MatchesAnalyzedFiles())
                {
                    Log.LogWarning("Intro skip disabled: startup binaries differ from the analyzed Store build.");
                    return;
                }

                var target = AccessTools.DeclaredMethod(typeof(StartupRoutine), nameof(StartupRoutine.MoveNext), Type.EmptyTypes);
                var menuFocus = AccessTools.DeclaredMethod(typeof(MainMenu), nameof(MainMenu.OnGotFocus), Type.EmptyTypes);
                var harmony = new Harmony(PluginGuid);
                harmony.Patch(menuFocus,
                    postfix: new HarmonyMethod(typeof(DeleteStartup), nameof(RestoreMainMenuLogo)));
                harmony.Patch(target,
                    prefix: new HarmonyMethod(typeof(DeleteStartup), nameof(SkipLogoStates)),
                    finalizer: new HarmonyMethod(typeof(DeleteStartup), nameof(RestoreLogoTiming)));
                Log.LogInfo("Intro skip and main menu logo handoff registered (test 4-1).");
            }
            catch (Exception exception)
            {
                Log.LogError($"Intro skip setup failed (test 4-1). {exception}");
            }
        }

        private static bool MatchesAnalyzedFiles()
        {
            return MatchesHash(Path.Combine(Paths.GameRootPath, "GameAssembly.dll"),
                       "2061BD0A0C55C84F44AD53E4436440D6FA29BA260AEC3A2268FF93D656503D43")
                && MatchesHash(Path.Combine(Paths.GameRootPath, "HumanFallFlat_Data", "il2cpp_data", "Metadata", "global-metadata.dat"),
                       "99D63FEE40BD05FC7B6C2A84F0EA4F17F991D31021DEC32249E95E522D17E81B")
                && MatchesHash(typeof(StartupExperienceController).Assembly.Location,
                       "702E5880A92585201078BEB90F7834BB830942ADE9750F26F8371BCFBD7D3D9B");
        }

        private static bool MatchesHash(string path, string expected)
        {
            using var file = File.OpenRead(path);
            using var hash = SHA256.Create();
            return Convert.ToHexString(hash.ComputeHash(file)) == expected;
        }

        private static void SkipLogoStates(StartupRoutine __instance,
            out (StartupExperienceController Controller, float OffsetTime) __state)
        {
            __state = default;
            try
            {
                int state = __instance.__1__state;
                if (state != WaitingForFirstCameraTransition && state != WaitingForCurveLogo)
                {
                    return;
                }

                var controller = __instance.__4__this;
                var camera = controller?.cameraEffects;
                var drones = controller?.drones;
                var startupUi = __instance._startupUI_5__2;
                if (camera == null || drones == null || startupUi == null)
                {
                    LogSkipFailure("Startup camera, drones or UI missing.");
                    return;
                }

                float dronesTime = drones.dronesTime;
                if (!float.IsFinite(dronesTime))
                {
                    LogSkipFailure("IntroDrones returned a non-finite playback time.");
                    return;
                }

                if (state == WaitingForFirstCameraTransition)
                {
                    // Preserve the native one-second camera lead-in and its subsequent transition.
                    if (dronesTime < 1f)
                    {
                        return;
                    }

                    camera.transitionDuration = 5f;
                    camera.Transition(1f, Vector2.zero, 0f, 0.7f, 0f, 0f);
                }

                // State 7 starts the user manager before testing the remaining logo hold.
                // Move only that deadline into the past and restore the field after this call.
                __state = (controller, controller.offsetTime);
                controller.offsetTime = (float)(dronesTime - PressAnythingDronesTime - 1d);
                __instance.__1__state = AfterHumanLogoDelay;
                _skippedStartupUi = startupUi;

                if (Interlocked.Exchange(ref _skipLogged, 1) == 0)
                {
                    _log.LogInfo("Intro logo states bypassed; resuming native startup at state 7 (test 4-1).");
                }
            }
            catch (Exception exception)
            {
                LogSkipFailure(exception.ToString());
            }
        }

        private static void RestoreMainMenuLogo(MainMenu __instance)
        {
            var startupUi = _skippedStartupUi;
            if (startupUi == null)
            {
                return;
            }

            _skippedStartupUi = null;
            try
            {
                if (startupUi != StartupExperienceUI.instance || !startupUi.gameObject.activeInHierarchy)
                {
                    return;
                }

                // OnGotFocus and Update hide MainMenu.logo while the startup UI is active.
                // Run the game's usual logo handoff when this menu first receives focus.
                __instance.RemoveStartExperienceLogo();
                var logo = __instance.logo;
                if (logo != null && logo.activeSelf && !startupUi.gameObject.activeInHierarchy)
                {
                    _log.LogInfo("Main menu title logo enabled by native startup UI handoff (test 4-1).");
                }
                else
                {
                    _log.LogWarning("Main menu logo handoff did not reach the expected UI state (test 4-1).");
                }
            }
            catch (Exception exception)
            {
                _log.LogWarning($"Main menu logo handoff failed (test 4-1). {exception}");
            }
        }

        private static void RestoreLogoTiming((StartupExperienceController Controller, float OffsetTime) __state)
        {
            if (__state.Controller != null)
            {
                __state.Controller.offsetTime = __state.OffsetTime;
            }
        }

        private static void LogSkipFailure(string reason)
        {
            if (Interlocked.Exchange(ref _failureLogged, 1) == 0)
            {
                _log.LogWarning($"Intro skip could not be applied (test 4-1). {reason}");
            }
        }
    }
}
