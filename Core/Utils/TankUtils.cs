namespace Vint.Core.Utils;

public static class TankUtils {
    public static float CalculateFrozenSpeed(float baseSpeed, float percent) => 
        baseSpeed * percent / 100;
}