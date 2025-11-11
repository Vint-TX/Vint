using System.Numerics;
using JetBrains.Annotations;
using Redzen.Random;

namespace Vint.Core.Utils;

public static class MathUtils {
    [Pure]
    public static int Map(int value, int inMin, int inMax, int outMin, int outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    [Pure]
    public static long Map(long value, long inMin, long inMax, long outMin, long outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    [Pure]
    public static float Map(float value, float inMin, float inMax, float outMin, float outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    [Pure]
    public static double Map(double value, double inMin, double inMax, double outMin, double outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    [Pure]
    public static decimal Map(decimal value, decimal inMin, decimal inMax, decimal outMin, decimal outMax) =>
        (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    [Pure]
    public static bool RollTheDice(double chance, Random? random = null) => (random ?? Random.Shared).NextDouble() <= chance;

    [Pure]
    public static bool RollTheDice(double chance, IRandomSource random) => random.NextDouble() <= chance;

    extension<T>(IEnumerable<T> valueList) where T : INumber<T>, IRootFunctions<T> {
        [Pure]
        public T StandardDeviation() {
            T m = T.Zero;
            T s = T.Zero;
            int k = 0;

            foreach (T value in valueList) {
                k++;

                T tmpM = m;
                m += (value - tmpM) / T.CreateChecked(k);
                s += (value - tmpM) * (value - m);
            }

            return T.Sqrt(s / T.CreateChecked(k));
        }
    }

    extension<T>(IEnumerable<T> valueList) {
        [Pure]
        public TNum StandardDeviationBy<TNum>([InstantHandle] Func<T, TNum> selector)
            where TNum : INumber<TNum>, IRootFunctions<TNum> =>
            valueList.Select(selector).StandardDeviation();

        [Pure]
        public TNum StandardDeviationBy<TNum>([InstantHandle] Func<T, int, TNum> selector)
            where TNum : INumber<TNum>, IRootFunctions<TNum> =>
            valueList.Select(selector).StandardDeviation();
    }
}
