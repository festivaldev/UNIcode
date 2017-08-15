using System.Collections.Generic;

namespace UNIcode
{
    public static class Helper
    {
        private static Dictionary<string, int> fontWeights = new Dictionary<string, int> {
            { "ultralight", 100 },
            { "light", 200 },
            { "semilight", 300 },
            { "normal", 400 },
            { "regular", 400 },
            { "semibold", 600 },
            { "bold", 700 },
            { "ultrabold", 900 },
            { "black", 900 }
        };

        public static int GetFontWeightFromTypefaceName(string name) {
            name = name.Replace("-", "").Replace(" ", "").ToLower().Replace("demi", "semi").Replace("extra", "ultra").Replace("thin", "light").Replace("heavy", "black");
            var weight = 400;

            if (fontWeights.ContainsKey(name))
                weight = fontWeights[name];

            return weight;
        }

        public static IEnumerable<int> GetRangeInSteps(int start, int end, int step) {
            for (var i = start; i <= end; i += step)
                yield return i;
        }
    }
}
