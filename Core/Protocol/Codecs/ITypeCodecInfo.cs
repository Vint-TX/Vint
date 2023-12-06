namespace Vint.Core.Protocol.Codecs;

public interface ITypeCodecInfo : ICodecInfo {
    public Type Type { get; }
    public bool Nullable { get; }
    public bool Varied { get; }
    public HashSet<Attribute> Attributes { get; }
}