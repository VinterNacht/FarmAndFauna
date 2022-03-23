using System;
using System.Collections.Generic;
using System.Text;

namespace FarmAndFauna.config
{
    class FarmAndFaunaConfig
    {
        public bool setAnimalGrowthToOneYear = false;
        public int animalGrowthTimeDivisor = 0;

        public FarmAndFaunaConfig()
        { }

        public static FarmAndFaunaConfig Current { get; set; }

        public static FarmAndFaunaConfig GetDefault()
        {
            FarmAndFaunaConfig defaultConfig = new();

            defaultConfig.setAnimalGrowthToOneYear = false;
            defaultConfig.animalGrowthTimeDivisor = 1;
            return defaultConfig;
        }
    }
}
