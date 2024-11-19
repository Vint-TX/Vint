using System.Collections.Frozen;
using System.Numerics;
using System.Reflection;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Movement;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Codecs;
using Vint.Core.Protocol.Codecs.Factories;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Protocol;

public class ProtocolBuilder {
    public ProtocolBuilder() {
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
        Factories.Add(new TimeSpanCodecFactory());
        Factories.Add(new GroupComponentCodecFactory());
        Factories.Add(new StructCodecFactory());

        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.HasProtocolId())
            .ToList()
            .ForEach(type => Types[type.GetProtocolId()] = type);
    }

    Dictionary<long, Type> Types { get; } = [];
    Dictionary<ICodecInfo, ICodec> Codecs { get; } = [];
    HashSet<ICodecFactory> Factories { get; } = [];

    public Protocol Build() => new(Types.ToFrozenDictionary(), Codecs.ToFrozenDictionary(), Factories.ToFrozenSet());

    void Register<T>(ICodec codec) => Register(new CodecInfo(typeof(T)), codec);

    void Register(ICodecInfo codecInfo, ICodec codec) => Codecs[codecInfo] = codec;
}
