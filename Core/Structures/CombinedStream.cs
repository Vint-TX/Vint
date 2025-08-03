using JetBrains.Annotations;

namespace Vint.Core.Structures;

[PublicAPI]
public class CombinedStream(
    Stream first,
    Stream second
) : Stream {
    bool _closed;
    bool _firstConsumed;

    public override bool CanRead => !_closed;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        ObjectDisposedException.ThrowIf(_closed, this);
        ValidateBufferArguments(buffer, offset, count);

        int read = 0;

        if (!_firstConsumed) {
            read += first.Read(buffer, offset, count);
            if (read == count) return read;
            _firstConsumed = true;
        }

        int remaining = count - read;
        read += second.Read(buffer, offset + read, remaining);
        return read;
    }

    public override int Read(Span<byte> buffer) {
        ObjectDisposedException.ThrowIf(_closed, this);

        int count = buffer.Length;
        int read = 0;

        if (!_firstConsumed) {
            read += first.Read(buffer);
            if (read == count) return read;
            _firstConsumed = true;
        }

        read += second.Read(buffer[read..]);
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        ObjectDisposedException.ThrowIf(_closed, this);
        ValidateBufferArguments(buffer, offset, count);
        cancellationToken.ThrowIfCancellationRequested();

        int read = 0;

        if (!_firstConsumed) {
            read += await first.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
            if (read == count) return read;
            _firstConsumed = true;
        }

        int remaining = count - read;
        read += await second.ReadAsync(buffer.AsMemory(offset + read, remaining), cancellationToken);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        ObjectDisposedException.ThrowIf(_closed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (buffer.IsEmpty)
            return 0;

        int count = buffer.Length;
        int read = 0;

        if (!_firstConsumed) {
            read += await first.ReadAsync(buffer, cancellationToken);
            if (read == count) return read;
            _firstConsumed = true;
        }

        read += await second.ReadAsync(buffer[read..], cancellationToken);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush() { }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override void Close() {
        _closed = true;
        base.Close();
    }
}
