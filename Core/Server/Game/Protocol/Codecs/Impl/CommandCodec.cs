using System.Collections.Frozen;
using Serilog;
using Vint.Core.Logging;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;
using Vint.Core.Server.Game.Protocol.Commands;

namespace Vint.Core.Server.Game.Protocol.Codecs.Impl;

public class CommandCodec : Codec {
    public CommandCodec(params (CommandCode code, Type type)[] codes) {
        CommandToCode = codes.ToFrozenDictionary(
            tuple => tuple.type,
            tuple => tuple.code
        );

        CodeToCommand = codes.ToFrozenDictionary(
            tuple => tuple.code,
            tuple => tuple.type
        );
    }

    ILogger Logger { get; } = Log.Logger.ForType<CommandCodec>();
    FrozenDictionary<Type, CommandCode> CommandToCode { get; }
    FrozenDictionary<CommandCode, Type> CodeToCommand { get; }

    public override void Encode(ProtocolBuffer buffer, object value) {
        Type type = value.GetType();
        CommandCode code = CommandToCode[type];

        Protocol
            .GetCodec(new TypeCodecInfo(typeof(CommandCode)))
            .Encode(buffer, code);

        Protocol
            .GetCodec(new TypeCodecInfo(type))
            .Encode(buffer, value);
    }

    public override object Decode(ProtocolBuffer buffer) {
        CommandCode code = (CommandCode)Protocol
            .GetCodec(new TypeCodecInfo(typeof(CommandCode)))
            .Decode(buffer);

        Type type = CodeToCommand[code];

        //Logger.Debug("Decoding command of type {Type}", code);

        return Protocol
            .GetCodec(new TypeCodecInfo(type))
            .Decode(buffer);
    }
}
