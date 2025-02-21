using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Codecs.Buffer;

public sealed class ProtocolBuffer(
    MemoryStream stream,
    OptionalMap optionalMap,
    IPlayerConnection connection
) : IAsyncDisposable, IDisposable {
    public ProtocolBuffer(IPlayerConnection connection) : this(new MemoryStream(), new OptionalMap(), connection) { }

    public MemoryStream Stream { get; } = stream;
    public OptionalMap OptionalMap { get; } = optionalMap;
    public IPlayerConnection Connection { get; } = connection;

    public BinaryReader Reader { get; } = new BigEndianBinaryReader(stream);
    public BinaryWriter Writer { get; } = new BigEndianBinaryWriter(stream);

    List<AsyncServiceScope> Scopes { get; } = [];

    public IEntity? GetSharedEntity(long id) => Connection.SharedEntities.SingleOrDefault(entity => entity.Id == id);

    public IServiceScope CreateServiceScope() {
        AsyncServiceScope serviceScope = Connection.ServiceProvider.CreateAsyncScope();
        Scopes.Add(serviceScope);
        return serviceScope;
    }

    public static ProtocolBuffer Unwrap(BinaryReader reader, IPlayerConnection connection) {
        // Header
        byte firstByte = reader.ReadByte();
        byte secondByte = reader.ReadByte();

        if (firstByte != byte.MaxValue) throw new InvalidDataException($"Invalid first header byte: {firstByte}");
        if (secondByte != byte.MinValue) throw new InvalidDataException($"Invalid second header byte: {secondByte}");

        // Sizes
        int optionalMapBitsSize = reader.ReadInt32();
        int optionalMapBytesSize = GetSizeInBytes(optionalMapBitsSize);
        int dataSize = (int)reader.ReadUInt32();

        // Optional map
        byte[] optionalMapBytes = reader.ReadBytes(optionalMapBytesSize);
        OptionalMap optionalMap = new(optionalMapBytes, optionalMapBitsSize);

        // Data
        MemoryStream stream = new(dataSize);
        reader.BaseStream.CopyTo(stream, limit: dataSize);
        stream.Seek(0, SeekOrigin.Begin);

        ProtocolBuffer buffer = new(stream, optionalMap, connection);

        if (!connection.Logger.IsEnabled(LogEventLevel.Verbose))
            return buffer;

        byte[] streamBytes = stream.ToArray();
        List<byte> bytes = new(optionalMapBytes.Length + streamBytes.Length);

        bytes.AddRange(optionalMapBytes);
        bytes.AddRange(streamBytes);

        connection.Logger.Verbose("Unwrapped {Size} bytes ({Hex})", bytes.Count, Convert.ToHexString(bytes.ToArray()));
        return buffer;
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

    static int GetSizeInBytes(int size) =>
        (int)Math.Ceiling(size / 8.0);

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing) {
        if (disposing) {
            Reader.Dispose();
            Writer.Dispose();
            Stream.Dispose();
        }
    }

    async ValueTask DisposeAsyncCore() {
        Reader.Dispose();
        await Writer.DisposeAsync();
        await Stream.DisposeAsync();

        foreach (AsyncServiceScope serviceScope in Scopes)
            await serviceScope.DisposeAsync();

        Scopes.Clear();
    }

    ~ProtocolBuffer() => Dispose(false);
}
