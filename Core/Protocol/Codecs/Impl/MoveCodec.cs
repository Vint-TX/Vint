using System.Collections;
using System.Numerics;
using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public abstract class MoveCodec : Codec {
    public const int MovementSize = 21;
    public const int PositionComponentBitsize = 17;
    public const int OrientationComponentBitsize = 13;
    public const int LinearVelocityComponentBitsize = 13;
    public const int AngularVelocityComponentBitsize = 13;
    public const float PositionFactor = 0.01f;
    public const float VelocityFactor = 0.01f;
    public const float AngularVelocityFactor = 0.005f;
    public const float OrientationPrecision = 2f / (1 << OrientationComponentBitsize);

    public const int WeaponRotationSize = 2;
    public const int WeaponRotationComponentBitsize = WeaponRotationSize * 8;
    public const float WeaponRotationFactor = 360f / (1 << WeaponRotationComponentBitsize);

    public const float HitEps = 0.001f;

    protected readonly static byte[] BufferForWeaponRotation = new byte[WeaponRotationSize];

    protected readonly static byte[] BufferForWeaponRotationEmpty = Array.Empty<byte>();

    protected readonly static BitArray BitsForWeaponRotation = new(BufferForWeaponRotation);

    protected readonly static BitArray BitsForWeaponRotationEmpty = new(BufferForWeaponRotationEmpty);

    protected MoveCodec() =>
        Logger = Log.Logger.ForType(GetType());

    ILogger Logger { get; }

    protected static void CopyBits(byte[] buffer, BitArray bits) {
        for (int i = 0; i < buffer.Length; i++) {
            for (int j = 0; j < 8; j++) {
                int index = i * 8 + j;
                bool value = (buffer[i] & 1 << j) != 0;
                bits.Set(index, value);
            }
        }
    }

    protected float ReadFloat(BitArray bits, ref int position, int size, float factor) {
        float num = (Read(bits, ref position, size) - (1 << size - 1)) * factor;

        if (num.IsValidFloat()) return num;

        Logger.Error("Invalid float: {Float}", num);
        return 0f;
    }

    protected void WriteFloat(BitArray bits, ref int position, float value, int size, float factor) {
        int offset = 1 << size - 1;
        Write(bits, ref position, size, PrepareValue(value, offset, factor));
    }

    int PrepareValue(float value, int offset, float factor) {
        int num = (int)(value / factor);
        int num2 = 0;

        if (num < -offset)
            Logger.Warning("Value too small: {Value} offset={Offset} factor={Factor}", value, offset, factor);
        else
            num2 = num - offset;

        if (num2 < offset) return num2;

        Logger.Warning("Value too big: {Value} offset={Offset} factor={Factor}", value, offset, factor);
        num2 = offset;

        return num2;
    }

    int Read(BitArray bits, ref int position, int bitsCount) {
        if (bitsCount > 32)
            throw new Exception($"Cannot read more that 32 bit at once (requested {bitsCount})");

        if (position + bitsCount > bits.Length)
            throw new Exception($"BitArea is out of data: requested {bitsCount} bits, available: {bits.Length - position}");

        int num = 0;

        for (int num2 = bitsCount - 1; num2 >= 0; num2--) {
            if (bits.Get(position))
                num += 1 << num2;

            position++;
        }

        return num;
    }

    void Write(BitArray bits, ref int position, int bitsCount, int value) {
        if (bitsCount > 32)
            throw new Exception($"Cannot write more that 32 bit at once (requested {bitsCount})");

        if (position + bitsCount > bits.Length)
            throw new Exception($"BitArea overflow: attempt to write {bitsCount} bits, space available: {bits.Length - position}");

        for (int num = bitsCount - 1; num >= 0; num--) {
            bool value2 = (value & 1 << num) != 0;
            bits.Set(position, value2);
            position++;
        }
    }

    protected Vector3 ReadVector3(BitArray bits, ref int position, int size, float factor) {
        Vector3 result = default;
        result.X = ReadFloat(bits, ref position, size, factor);
        result.Y = ReadFloat(bits, ref position, size, factor);
        result.Z = ReadFloat(bits, ref position, size, factor);
        return result;
    }

    protected Quaternion ReadQuaternion(BitArray bits, ref int position, int size, float factor) {
        Quaternion result = default;
        result.X = ReadFloat(bits, ref position, size, factor);
        result.Y = ReadFloat(bits, ref position, size, factor);
        result.Z = ReadFloat(bits, ref position, size, factor);
        result.W = MathF.Sqrt(1f - (result.X * result.X + result.Y * result.Y + result.Z * result.Z));

        if (float.IsNaN(result.W))
            result.W = 0f;

        return result;
    }

    protected void WriteVector3(BitArray bits, ref int position, Vector3 value, int size, float factor) {
        WriteFloat(bits, ref position, value.X, size, factor);
        WriteFloat(bits, ref position, value.Y, size, factor);
        WriteFloat(bits, ref position, value.Z, size, factor);
    }

    protected void WriteQuaternion(BitArray bits, ref int position, Quaternion value, int size, float factor) {
        int num = value.W >= 0f ? 1 : -1;
        WriteFloat(bits, ref position, value.X * num, size, factor);
        WriteFloat(bits, ref position, value.Y * num, size, factor);
        WriteFloat(bits, ref position, value.Z * num, size, factor);
    }

    protected BitArray GetBitsForWeaponRotation(bool hasWeaponRotation) => !hasWeaponRotation ? BitsForWeaponRotationEmpty : BitsForWeaponRotation;

    protected byte[] GetBufferForWeaponRotation(bool hasWeaponRotation) =>
        !hasWeaponRotation ? BufferForWeaponRotationEmpty : BufferForWeaponRotation;
}