namespace Vint.Core.Protocol.Codecs.Buffer;

public class OptionalMap {
    public OptionalMap() => Data = new List<byte>();

    public OptionalMap(IEnumerable<byte> data, int length) {
        Data = data.ToList();
        Length = length;
    }

    List<byte> Data { get; }

    public int Length { get; private set; }
    public int Position { get; private set; }

    public void Add(bool isNull) {
        if (Position >= Length) {
            Data.Add(0);
            Length += 8;
        }

        Data[Position / 8] |= (byte)(Convert.ToInt32(isNull) << 7 - Position++ % 8);
    }

    public bool Get() {
        if (Position >= Length) {
            throw new IndexOutOfRangeException(
                "Index was out of range. Must be non-negative and less than the size of the OptionalMap.");
        }

        return Convert.ToBoolean(Data[Position / 8] >> 7 - Position++ % 8 & 1);
    }

    public void Rewind() => Position = 0;

    public byte[] GetBytes() => Data.ToArray();
}