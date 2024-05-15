using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Buffer;

public class ProtocolBuffer(
    OptionalMap optionalMap,
    IPlayerConnection connection
) {
    public MemoryStream Stream { get; private set; } = new();
    public BinaryReader Reader => new BigEndianBinaryReader(Stream);
    public BinaryWriter Writer => new BigEndianBinaryWriter(Stream);
    public OptionalMap OptionalMap { get; private set; } = optionalMap;

    public IPlayerConnection Connection { get; } = connection;

    public IEntity? GetSharedEntity(long id) => Connection.SharedEntities.SingleOrDefault(entity => entity.Id == id);

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

        byte[] array = Stream.ToArray();
        List<byte> bytes = new((int)(OptionalMap.Length + Stream.Length));

        bytes.AddRange(OptionalMap.GetBytes());
        bytes.AddRange(array);

        Connection.Logger.Verbose("Unwrapped {Size} bytes ({Hex})", bytes.Count, Convert.ToHexString(bytes.ToArray()));
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

        Connection.Logger.Verbose("Wrapped {Size} bytes ({Hex})", Stream.Length, Convert.ToHexString(Stream.ToArray()));
    }

    int GetSizeInBytes(int size) =>
        (int)Math.Ceiling(size / 8.0);
}
