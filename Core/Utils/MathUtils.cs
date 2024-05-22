using Redzen.Random;

namespace Vint.Core.Utils;

public static class MathUtils {
    public static int Map(int value, int inMin, int inMax, int outMin, int outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    public static long Map(long value, long inMin, long inMax, long outMin, long outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    public static float Map(float value, float inMin, float inMax, float outMin, float outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    public static double Map(double value, double inMin, double inMax, double outMin, double outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    public static decimal Map(decimal value, decimal inMin, decimal inMax, decimal outMin, decimal outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    public static bool RollTheDice(double chance, Random? random = null) => (random ?? Random.Shared).NextDouble() <= chance;

    public static bool RollTheDice(double chance, IRandomSource random) => random.NextDouble() <= chance;
}
