using Vint.Utils;

namespace Vint.Core.Protocol.Codecs.Buffer;

public class ProtocolBuffer(OptionalMap optionalMap) {
    public MemoryStream Stream { get; private set; } = new();
    public BinaryReader Reader => new BigEndianBinaryReader(Stream);
    public BinaryWriter Writer => new BigEndianBinaryWriter(Stream);
    public OptionalMap OptionalMap { get; private set; } = optionalMap;

    public void Clear() {
        OptionalMap = new OptionalMap();

        Stream.Close();
        Stream = new MemoryStream();
    }

    public bool Unwrap(BinaryReader reader) {
        Clear();

        // Header
        byte firstByte = reader.ReadByte();
        byte secondByte = reader.ReadByte();

        if (firstByte != byte.MaxValue) throw new InvalidDataException($"Invalid first header byte: {firstByte}");
        if (secondByte != byte.MinValue) throw new InvalidDataException($"Invalid second header byte: {secondByte}");

        // Sizes
        int optionalMapBits = reader.ReadInt32();
        int optionalMapBytes = GetSizeInBytes(optionalMapBits);

        int dataSize = (int)reader.ReadUInt32();

        // Optional map
        OptionalMap = new OptionalMap(reader.ReadBytes(optionalMapBytes), optionalMapBits);

        // Data
        reader.BaseStream.CopyTo(Stream, limit: dataSize);
        Stream.Seek(0, SeekOrigin.Begin);

        return true;
    }

    public void Wrap(BinaryWriter writer) {
        // Header
        writer.Write(byte.MaxValue);
        writer.Write(byte.MinValue);

        // Sizes
        writer.Write(OptionalMap.Length);
        writer.Write((uint)Stream.Length);

        // Optional map
        writer.Write(OptionalMap.GetBytes());

        // Data
        Stream.Seek(0, SeekOrigin.Begin);
        Stream.CopyTo(writer.BaseStream);
    }

    int GetSizeInBytes(int size) =>
        (int)Math.Ceiling(size / 8.0);
}
