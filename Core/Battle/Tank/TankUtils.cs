namespace Vint.Core.Battle.Tank;

public static class TankUtils {
    public static float CalculateFrozenSpeed(float baseSpeed, float percent) =>
        baseSpeed * percent / 100;
}
