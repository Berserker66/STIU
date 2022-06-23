using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LargeNumbers;
using UnityEngine;

namespace SpaceTravelIdleUnlocker
{
    [BepInPlugin("STIU", "Space Travel Idle Unlocker", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public ConfigEntry<double> DarkMatterExponent;
        public ConfigEntry<double> DarkMatterFactor;
        public ConfigEntry<bool> GrantBetaCards;

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            GrantBetaCards = Config.Bind("General", "GrantBetaCards", false,
                "Grants the user a set of cards that is normally limited to beta testers. Seems to require a new save game.");
                
            DarkMatterExponent = Config.Bind("DarkMatter", "Exponent", 0.9, "Exponent component of Big Bang Formula.");
            DarkMatterFactor = Config.Bind("DarkMatter", "Factor", 0.35, "Factor component of Big Bang Formula.");
            
            
            Harmony.CreateAndPatchAll(typeof(Plugin));
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is initialized!");
        }
        // General Section
        [HarmonyPatch(typeof(CardBag), MethodType.Constructor)]
        [HarmonyPostfix]
        static void ModifiedCardBag(CardBag __instance)
        {
            if (Instance.GrantBetaCards.Value)
            {
                __instance.AddBetaCard1();
                __instance.AddBetaCard2();
                __instance.AddBetaCard3();
            }
        }

        // Dark Matter Section
        [HarmonyPatch(typeof(BigBangManager), "CalculateDarkMatter")]
        [HarmonyPrefix]
        static bool ModifiedCalculateDarkMatter(BigBangManager __instance)
        {
            __instance.calculatedBigBangDarkMatter = __instance.CalculateBigBangDarkMatter();
            __instance.calculatedTotalDarkMatter = __instance.calculatedBigBangDarkMatter + __instance.currentTotalDarkMatter;
            __instance.calculatedDarkMatterIncrement = ((__instance.calculatedBigBangDarkMatter > ScientificNotation.zero) ? 
                __instance.calculatedBigBangDarkMatter : ScientificNotation.zero);

            
            return false; // Returning false in prefix patches skips running the original code
        }
        [HarmonyPatch(typeof(BigBangManager), "CalculateBigBangDarkMatter")]
        [HarmonyPrefix]
        static bool ModifiedCalculateBigBangDarkMatter(BigBangManager __instance, ref ScientificNotation __result)
        {
            __instance.researchScore = TechnoManager.shared.CalculateBigBangResearchScore();
            __instance.infraScore = TechnoManager.shared.CalculateBigBangInfraScore();
            ScientificNotation a = __instance.researchScore + __instance.infraScore;
            StatsManager.shared.CalculateBigBangDMStats(out __instance.energyScore, out __instance.travelDistanceScore, out __instance.maxDamageScore);
            ScientificNotation num = new ScientificNotation(Mathf.Max(0, __instance.energyScore + __instance.travelDistanceScore + __instance.maxDamageScore));
            double num2 = Math.Pow((a * num).Standard(), Instance.DarkMatterExponent.Value) * Instance.DarkMatterFactor.Value;
            if (num2 <= 0.0)
            {
                __result = ScientificNotation.zero;
            }
            else
            {
                __result = new ScientificNotation(num2);
            }
            return false;
        }

        [HarmonyPatch(typeof(BigBangPanel), "FixedUpdate")]
        [HarmonyPrefix]
        static bool ModifiedTurnOnBigBangTab(BigBangPanel  __instance)
        {
            {
                AccessTools.Field(typeof(BigBangPanel), "BIG_BANG_FORMULA").SetValue(__instance, 
                    "(((R + I) * (E + T + D)) ^ " + Instance.DarkMatterExponent.Value + ") * " + Instance.DarkMatterFactor.Value + " = <color=#bf2222><b>{0}</b></color>");
            }
            return true;
        }
    }
}
