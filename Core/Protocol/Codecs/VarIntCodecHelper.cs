namespace Vint.Core.Protocol.Codecs;

public static class VarIntCodecHelper {
    public static void Encode(BinaryWriter writer, int value) {
        switch (value) {
            case < 0:
                throw new OverflowException($"Cannot encode length: {value}");

            case < 0x80: {
                writer.Write((byte)((uint)value & 0x7F));
                break;
            }

            case < 0x4000: {
                long temp = (value & 0x3FFF) + 0x8000;
                writer.Write((byte)((temp & 0xFF00) >> 8));
                writer.Write((byte)(temp & 0xFF));
                break;
            }

            case < 0x400000: {
                long temp = (value & 0x3FFFFF) + 0xC00000;
                writer.Write((byte)((temp & 0xFF0000) >> 16));
                writer.Write((byte)((temp & 0xFF00) >> 8));
                writer.Write((byte)(temp & 0xFF));
                break;
            }

            default:
                throw new OverflowException($"Cannot encode length: {value}");
        }
    }

    public static int Decode(BinaryReader reader) {
        byte byte1 = reader.ReadByte();
        if ((byte1 & 0x80) == 0) return byte1;

        byte byte2 = reader.ReadByte();
        if ((byte1 & 0x40) == 0) return ((byte1 & 0x3F) << 8) + (byte2 & 0xFF);

        byte byte3 = reader.ReadByte();
        return ((byte1 & 0x3F) << 16) + ((byte2 & 0xFF) << 8) + (byte3 & 0xFF);
    }
}