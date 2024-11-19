namespace Vint.Core.Protocol.Codecs.Info;

public interface ICodecInfo {
    public Type Type { get; }
    public bool Nullable { get; }
    public bool Varied { get; }
}
