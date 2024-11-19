using System.Collections.Frozen;
using Serilog;
using Vint.Core.Exceptions;
using Vint.Core.Protocol.Codecs;
using Vint.Core.Protocol.Codecs.Factories;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol;

public class Protocol {
    public Protocol(FrozenDictionary<long, Type> types, FrozenDictionary<ICodecInfo, ICodec> codecs, FrozenSet<ICodecFactory> factories) {
        Types = types;
        Codecs = codecs;
        Factories = factories;

        InitAllCodecs();
    }

    ILogger Logger { get; } = Log.Logger.ForType(typeof(Protocol));

    FrozenDictionary<long, Type> Types { get; }
    FrozenDictionary<ICodecInfo, ICodec> Codecs { get; }
    FrozenSet<ICodecFactory> Factories { get; }

    void InitAllCodecs() {
        foreach (ICodec codec in Codecs.Values)
            codec.Init(this);
    }

    public ICodec GetCodec(CodecInfoWithAttributes codecInfoWithAttributes) {
        ICodecInfo codecInfo = codecInfoWithAttributes.CodecInfo;
        Type? underlyingType = Nullable.GetUnderlyingType(codecInfo.Type);

        if (underlyingType != null)
            codecInfo = new CodecInfo(underlyingType, codecInfo.Nullable, codecInfo.Varied);

        return Codecs.TryGetValue(codecInfo, out ICodec? codec)
            ? codec
            : CreateCodec(codecInfoWithAttributes);
    }

    public ICodec GetCodec(long protocolId) =>
        GetCodec(GetTypeById(protocolId));

    public ICodec GetCodec(Type type) =>
        GetCodec(new CodecInfo(type));

    ICodec GetCodec(ICodecInfo codecInfo) =>
        GetCodec(new CodecInfoWithAttributes(codecInfo));

    public Type GetTypeById(long id) =>
        Types.TryGetValue(id, out Type? type)
            ? type
            : throw new TypeNotRegisteredException(id);

    ICodec CreateCodec(CodecInfoWithAttributes codecInfo) {
        foreach (ICodecFactory codecFactory in Factories) {
            ICodec? codec = codecFactory.Create(this, codecInfo);

            if (codec == null) continue;

            Logger.Verbose("Created {Codec} with {Factory} for {Info}", codec, codecFactory.GetType().Name, codecInfo);

            codec.Init(this);
            return codec;
        }

        throw new ArgumentException($"Codec for {codecInfo} not found");
    }
}
