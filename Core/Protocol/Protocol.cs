using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Movement;
using Vint.Core.ECS.Templates;
using Vint.Core.Exceptions;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs;
using Vint.Core.Protocol.Codecs.Factories;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Protocol;

public class Protocol {
    public Protocol() {
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

        Register<ICommand>(new CommandCodec()
            .Register<InitTimeCommand>(CommandCode.InitTime)
            .Register<CloseCommand>(CommandCode.Close)
            .Register<ComponentAddCommand>(CommandCode.ComponentAdd)
            .Register<ComponentChangeCommand>(CommandCode.ComponentChange)
            .Register<ComponentRemoveCommand>(CommandCode.ComponentRemove)
            .Register<EntityShareCommand>(CommandCode.EntityShare)
            .Register<EntityUnshareCommand>(CommandCode.EntityUnshare)
            .Register<SendEventCommand>(CommandCode.SendEvent));

        Register<Vector3>(new Vector3Codec());
        Register<MoveCommand>(new MoveCommandCodec());
        Register<Movement>(new MovementCodec());

        Factories.Add(new OptionalCodecFactory());
        Factories.Add(new VariedCodecFactory());
        Factories.Add(new ArrayCodecFactory());
        Factories.Add(new ListCodecFactory());
        Factories.Add(new HashSetCodecFactory());
        Factories.Add(new DictionaryCodecFactory());
        Factories.Add(new EnumCodecFactory());
        Factories.Add(new GroupComponentCodecFactory());
        Factories.Add(new StructCodecFactory());

        Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsDefined(typeof(ProtocolIdAttribute)))
            .ToList()
            .ForEach(type => Types[type.GetCustomAttribute<ProtocolIdAttribute>()!.Id] = type);
    }

    ILogger Logger { get; } = Log.Logger.ForType(typeof(Protocol));

    Dictionary<long, Type> Types { get; } = new();
    Dictionary<ICodecInfo, ICodec> Codecs { get; } = new();
    List<ICodecFactory> Factories { get; } = [];

    void Register<T>(ICodec codec) => InitAndRegister(new TypeCodecInfo(typeof(T)), codec);

    void InitAndRegister(ICodecInfo codecInfo, ICodec codec) {
        codec.Init(this);
        Codecs[codecInfo] = codec;
    }

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
        if (Types.TryGetValue(id, out Type? type)) return type;

        throw new TypeNotRegisteredException(id);
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    ICodec CreateCodec(ICodecInfo codecInfo) {
        foreach (ICodecFactory codecFactory in Factories) {
            ICodec? codec = codecFactory.Create(this, codecInfo);

            if (codec == null) continue;

            Logger.Verbose("Created {Codec} with {Factory} for {Info}",
                codec,
                codecFactory.GetType()
                    .Name,
                codecInfo);

            codec.Init(this);

            return codec;
        }

        throw new ArgumentException($"Codec for {codecInfo} not found");
    }
}
