namespace Vint.Core.Server.Game.Protocol.Codecs.Factories;

public interface ICodecFactory {
    ICodec? Create(Protocol protocol, ICodecInfo codecInfo);
}
