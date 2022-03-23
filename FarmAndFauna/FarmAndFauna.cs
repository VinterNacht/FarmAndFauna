using FarmAndFauna.config;
using HarmonyLib;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace FarmAndFauna
{
    public class FarmAndFauna : ModSystem
    {
        private Harmony _harmony = new Harmony("harmoniousfaf");

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            PatchGame();
            try
            {
                var Config = api.LoadModConfig<FarmAndFaunaConfig>("fromgoldencombs.json");
                if (Config != null)
                {
                    api.Logger.Notification("Mod Config successfully loaded.");
                    FarmAndFaunaConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("No Mod Config specified. Falling back to default settings");
                    FarmAndFaunaConfig.Current = FarmAndFaunaConfig.GetDefault();
                }
            }
            catch
            {
                FarmAndFaunaConfig.Current = FarmAndFaunaConfig.GetDefault();
                api.Logger.Error("Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                if (FarmAndFaunaConfig.Current.animalGrowthTimeDivisor <= 0)
                    FarmAndFaunaConfig.Current.animalGrowthTimeDivisor = 1;
                api.StoreModConfig(FarmAndFaunaConfig.Current, "FarmAndFauna.json");
            }
        }

        public override void Dispose()
        {
            var harmony = new Harmony("harmoniousfaf");
            harmony.UnpatchAll("harmoniousfaf");
        }

        private void PatchGame()
        {
            Mod.Logger.Event("Applying Harmony patches");
            var harmony = new Harmony("harmoniousfaf");
            var original = typeof(EntityBehaviorGrow).GetMethod("Initalize");
            var patches = Harmony.GetPatchInfo(original);
            if (patches != null && patches.Owners.Contains("harmoniousfaf"))
            {
                return;
            }
            harmony.PatchAll();
        }

        private void UnPatchGame()
        {
            Mod.Logger.Event("Unapplying Harmony patches");

            _harmony.UnpatchAll();
        }

    }

    [HarmonyPatch]
    [HarmonyPatch(typeof(EntityBehaviorGrow), "Initialize")]
    //internal sealed class EntityBehaviorGrowPatches
    //Changes how core game calculates animal growth times

    class EntityBehaviorGrowPatches
    {
        [HarmonyPrefix]
        static bool Patch_EntityBehaviorGrow_Initialize_Prefix(
            EntityBehaviorGrow __instance,
            ref float __result,
            Random ___rand)
        {
            System.Diagnostics.Debug.WriteLine("Harmony Patch 1 Run");
            var hourstogrow = __instance.CallMethod<JsonObject>("AsFloat");
            if (hourstogrow == null)
            {
                __result = 99999999;
                return false;
            }
            else if (FarmAndFaunaConfig.Current.setAnimalGrowthToOneYear == true)
            {
                System.Diagnostics.Debug.WriteLine("Animal Growth One Year Set To True");
                __result = __instance.entity.World.Calendar.DaysPerYear * FarmAndFaunaConfig.Current.animalGrowthTimeDivisor;
                return false;
            }             
            return true;
         }
    }
}

    public static class HarmonyReflectionExtensions
    {
        /// <summary>
        ///     Calls a method within an instance of an object, via reflection. This can be an internal or private method within another assembly.
        /// </summary>
        /// <typeparam name="T">The return type, expected back from the method.</typeparam>
        /// <param name="instance">The instance to call the method from.</param>
        /// <param name="method">The name of the method to call.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>The return value of the reflected method call.</returns>
        public static T CallMethod<T>(this object instance, string method, params object[] args)
        {
            return (T)AccessTools.Method(instance.GetType(), method).Invoke(instance, args);
        }
}