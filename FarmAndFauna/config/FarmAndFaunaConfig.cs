using System;
using System.Collections.Generic;
using System.Text;

namespace FarmAndFauna.config
{
    class FarmAndFaunaConfig
    {
        public bool setAnimalGrowthToOneYear = false;
        public int animalGrowthTimeMultiplier = 0;

        public FarmAndFaunaConfig()
        { }

        public static FarmAndFaunaConfig Current { get; set; }

        public static FarmAndFaunaConfig GetDefault()
        {
            FarmAndFaunaConfig defaultConfig = new();

            defaultConfig.setAnimalGrowthToOneYear = false;
            defaultConfig.animalGrowthTimeMultiplier = 1;
            return defaultConfig;
        }
    }
}
