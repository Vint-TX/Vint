namespace Vint.Core.Utils;

public static class TankUtils {
    public const float FrozenSpeedPercent = 15;
    
    public static float CalculateFrozenSpeed(float baseSpeed, float percent = FrozenSpeedPercent) => 
        baseSpeed * percent / 100;
}