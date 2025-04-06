using OpenTK.Mathematics;

namespace Vint.Core.Battle.Simulations.Renderer.Utils;

public static class ColorUtils {
    public static Vector3 HexToRgb(string hex) {
        if (hex.StartsWith('#'))
            hex = hex[1..];

        int i = Convert.ToInt32(hex, 16);

        float r = (i >> 16 & 0xFF) / 255f;
        float g = (i >> 8 & 0xFF) / 255f;
        float b = (i & 0xFF) / 255f;

        return new Vector3(r, g, b);
    }

    public static string RgbToHex(Vector3 rgb) {
        int r = (int)(rgb.X * 255);
        int g = (int)(rgb.Y * 255);
        int b = (int)(rgb.Z * 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
