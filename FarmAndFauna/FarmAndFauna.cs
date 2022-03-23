using FarmAndFauna.config;
using HarmonyLib;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
                if (FarmAndFaunaConfig.Current.animalGrowthTimeMultiplier <= 0)
                    FarmAndFaunaConfig.Current.animalGrowthTimeMultiplier = 1;
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
        [HarmonyPostfix]
        static void Patch_EntityBehaviorGrow_Initialize_Prefix(
            EntityBehaviorGrow __instance,
            EntityProperties properties,
            JsonObject typeAttributes
            )
        {
            // Make sure the patch only works with animals, not plants that grow.
            if (__instance.entity is not EntityAgent) return;

            // If the annual growth setting is disabled, then multiply by animalGrowthTimeMultiplier.
            var hoursToGrow = typeAttributes["hoursToGrow"].AsFloat(96f) * FarmAndFaunaConfig.Current.animalGrowthTimeMultiplier;
            __instance.SetProperty("HoursToGrow", hoursToGrow);
            if (!FarmAndFaunaConfig.Current.setAnimalGrowthToOneYear) 
            return;

            // Change the `HoursToGrow` property within the EntityBehaviorGrow instance.
            var value = __instance.entity.World.Calendar.DaysPerYear * FarmAndFaunaConfig.Current.animalGrowthTimeMultiplier;
            __instance.SetProperty("HoursToGrow", value);
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

        /// <summary>
        ///     Sets a property within the calling instanced object. This can be an internal or private property within another assembly.
        /// </summary>
        /// <param name="instance">The instance in which the property resides.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="setVal">The value to set the property to.</param>
        public static void SetProperty(this object instance, string propertyName, object setVal)
        {
            AccessTools.Property(instance.GetType(), propertyName).SetValue(instance, setVal);
        }
}