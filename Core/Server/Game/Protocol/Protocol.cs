using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using Serilog;
using Serilog.Events;
using Vint.Core.Battle.Tank.Movement;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Exceptions;
using Vint.Core.Logging;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Server.Game.Protocol.Codecs;
using Vint.Core.Server.Game.Protocol.Codecs.Factories;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;
using Vint.Core.Server.Game.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol;

public class Protocol {
    public Protocol() {
        Dictionary<ICodecInfo, ICodec> codecs = [];

        Register<bool>(new BoolCodec());
        Register<sbyte>(new SByteCodec());
        Register<byte>(new ByteCodec());
        Register<short>(new ShortCodec());
        Register<ushort>(new UShortCodec());
        Register<int>(new IntCodec());
        Register<uint>(new UIntCodec());
        Register<long>(new LongCodec());
        Register<ulong>(new ULongCodec());
        Register<float>(new FloatCodec());
        Register<double>(new DoubleCodec());
        Register<string>(new StringCodec());
        Register<DateTime>(new DateTimeCodec());
        Register<DateTimeOffset>(new DateTimeOffsetCodec());

        Register<TemplateAccessor>(new TemplateAccessorCodec());
        Register<IEntity>(new EntityCodec());

        Register<ICommand>(new CommandCodec(
            (CommandCode.InitTime, typeof(InitTimeCommand)),
            (CommandCode.Close, typeof(CloseCommand)),
            (CommandCode.ComponentAdd, typeof(ComponentAddCommand)),
            (CommandCode.ComponentChange, typeof(ComponentChangeCommand)),
            (CommandCode.ComponentRemove, typeof(ComponentRemoveCommand)),
            (CommandCode.EntityShare, typeof(EntityShareCommand)),
            (CommandCode.EntityUnshare, typeof(EntityUnshareCommand)),
            (CommandCode.SendEvent, typeof(SendEventCommand))
        ));

        Register<Vector3>(new Vector3Codec());
        Register<MoveCommand>(new MoveCommandCodec());
        Register<Movement>(new MovementCodec());

        Codecs = codecs.ToFrozenDictionary();

        Types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsDefined(typeof(ProtocolIdAttribute)) && !type.IsAssignableTo(typeof(EntityTemplate)))
            .ToFrozenDictionary(type => type.GetProtocolId());

        Factories = [
            new OptionalCodecFactory(),
            new VariedCodecFactory(),
            new ArrayCodecFactory(),
            new ListCodecFactory(),
            new HashSetCodecFactory(),
            new DictionaryCodecFactory(),
            new EnumCodecFactory(),
            new GroupComponentCodecFactory(),
            new StructCodecFactory()
        ];

        return;
        void Register<T>(ICodec codec) => InitAndRegister(new TypeCodecInfo(typeof(T)), codec);

        void InitAndRegister(ICodecInfo codecInfo, ICodec codec) {
            codec.Init(this);
            codecs[codecInfo] = codec;
        }
    }

    ILogger Logger { get; } = Log.Logger.ForType<Protocol>();

    FrozenDictionary<long, Type> Types { get; }
    FrozenDictionary<ICodecInfo, ICodec> Codecs { get; }
    ImmutableArray<ICodecFactory> Factories { get; }

    [SuppressMessage("ReSharper", "InvertIf")]
    public ICodec GetCodec(ICodecInfo codecInfo) {
        if (codecInfo is TypeCodecInfo typeCodecInfo) {
            Type? underlyingType = Nullable.GetUnderlyingType(typeCodecInfo.Type);

            if (underlyingType != null) {
                codecInfo = new TypeCodecInfo(underlyingType, typeCodecInfo.Nullable, typeCodecInfo.Varied, typeCodecInfo.Attributes);
            }
        }

        return Codecs.TryGetValue(codecInfo, out ICodec? codec)
            ? codec
            : CreateCodec(codecInfo);
    }

    public Type GetTypeById(long id) {
        if (Types.TryGetValue(id, out Type? type))
            return type;

        throw new TypeNotRegisteredException(id);
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    ICodec CreateCodec(ICodecInfo codecInfo) {
        foreach (ICodecFactory codecFactory in Factories) {
            ICodec? codec = codecFactory.Create(this, codecInfo);

            if (codec == null) continue;

            if (Logger.IsEnabled(LogEventLevel.Verbose))
                Logger.Verbose("Created {Codec} with {Factory} for {Info}", codec, codecFactory.GetType().Name, codecInfo);

            codec.Init(this);
            return codec;
        }

        throw new ArgumentException($"Codec for {codecInfo} not found");
    }
}
