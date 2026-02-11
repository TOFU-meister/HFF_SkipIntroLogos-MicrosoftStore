using BepInEx;
using HarmonyLib;

namespace SkipIntroLogos
{
	[BepInPlugin("org.bepinex.plugins.humanfallflat.skipintrologos", "Skip Intro Logos", "2.0.0.0")]
	[BepInProcess("Human.exe")]
	class DeleteStartup : BaseUnityPlugin
	{
		private void Awake()
		{
			Harmony.CreateAndPatchAll(typeof(DeleteStartup)); // aplies the [HarmonyPrefix] method written down below once this plugin loads
		}

		[HarmonyPatch(typeof(StartupExperienceController), "PlayStartupExperience")]
		[HarmonyPrefix]
		private static bool SkipStartupExperience()
		{
			// patch PlayStartupExperience() to skip the "startup experience" (intro logos and music) instead of running the original code
			StartupExperienceController.instance.SkipStartupExperience(null); // this method expects a multiplayerServer as an argument, but accepts "null" as well, allowing us to easily skip the intro logos!
			// below 2 lines are for aesthetic -- looks better than the screen suddenly flashing from black to the main menu
			MenuCameraEffects.instance.BlackOut();
			StartupExperienceController.instance.FadeOutDim();

			return false; // returning false skips the original code
		}
	}
}