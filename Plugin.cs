using BepInEx;
using HarmonyLib;
using LargeNumbers;
using UnityEngine;

namespace SpaceTravelIdleUnlocker
{
    [BepInPlugin("STIU", "Space Travel Idle Unlocker", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        
        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
            Harmony.CreateAndPatchAll(typeof(Plugin));
        }
        [HarmonyPatch(typeof(BigBangManager), "CalculateDarkMatter")]
        [HarmonyPrefix]
        static bool ModifiedCalculateDarkMatter(BigBangManager __instance)
        {
            __instance.calculatedBigBangDarkMatter = __instance.CalculateBigBangDarkMatter();
            __instance.calculatedTotalDarkMatter = __instance.calculatedBigBangDarkMatter;
            __instance.calculatedDarkMatterIncrement = ((__instance.calculatedTotalDarkMatter > ScientificNotation.zero) ? 
                __instance.calculatedTotalDarkMatter : ScientificNotation.zero);
            var panel = GameObject.FindObjectOfType<BigBangPanel>(true);
            if (panel != null)
            {
                AccessTools.Field(typeof(BigBangPanel), "BIG_BANG_FORMULA").SetValue(panel, "(((R + I) * (E + T + D)) ^ 0.9) * 0.35 = <color=#bf2222><b>{0}</b></color>");
            }
            
            return false; // Returning false in prefix patches skips running the original code
        }
    }
}
