namespace Vint.Core.Protocol.Codecs.Factories;

public interface ICodecFactory {
    ICodec? Create(Protocol protocol, ICodecInfo codecInfo);
}