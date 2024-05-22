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
        Register(typeof(bool), new BoolCodec());
        Register(typeof(sbyte), new SByteCodec());
        Register(typeof(byte), new ByteCodec());
        Register(typeof(short), new ShortCodec());
        Register(typeof(ushort), new UShortCodec());
        Register(typeof(int), new IntCodec());
        Register(typeof(uint), new UIntCodec());
        Register(typeof(long), new LongCodec());
        Register(typeof(ulong), new ULongCodec());
        Register(typeof(float), new FloatCodec());
        Register(typeof(double), new DoubleCodec());
        Register(typeof(string), new StringCodec());
        Register(typeof(DateTime), new DateTimeCodec());
        Register(typeof(DateTimeOffset), new DateTimeOffsetCodec());

        Register(typeof(TemplateAccessor), new TemplateAccessorCodec());
        Register(typeof(IEntity), new EntityCodec());

        Register(typeof(ICommand),
            new CommandCodec()
                .Register<InitTimeCommand>(CommandCode.InitTime)
                .Register<CloseCommand>(CommandCode.Close)
                .Register<ComponentAddCommand>(CommandCode.ComponentAdd)
                .Register<ComponentChangeCommand>(CommandCode.ComponentChange)
                .Register<ComponentRemoveCommand>(CommandCode.ComponentRemove)
                .Register<EntityShareCommand>(CommandCode.EntityShare)
                .Register<EntityUnshareCommand>(CommandCode.EntityUnshare)
                .Register<SendEventCommand>(CommandCode.SendEvent));

        Register(typeof(Vector3), new Vector3Codec());
        Register(typeof(MoveCommand), new MoveCommandCodec());
        Register(typeof(Movement), new MovementCodec());

        Factories.Add(new OptionalCodecFactory());
        Factories.Add(new VariedCodecFactory());
        Factories.Add(new ArrayCodecFactory());
        Factories.Add(new ListCodecFactory());
        Factories.Add(new HashSetCodecFactory());
        Factories.Add(new DictionaryCodecFactory());
        Factories.Add(new EnumCodecFactory());
        Factories.Add(new GroupComponentCodecFactory());
        Factories.Add(new StructCodecFactory());

        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsDefined(typeof(ProtocolIdAttribute)))
            .ToList()
            .ForEach(type => Types[type.GetCustomAttribute<ProtocolIdAttribute>()!.Id] = type);
    }

    ILogger Logger { get; } = Log.Logger.ForType(typeof(Protocol));

    Dictionary<long, Type> Types { get; } = new();
    Dictionary<ICodecInfo, ICodec> Codecs { get; } = new();
    List<ICodecFactory> Factories { get; } = [];

    public void Register(Type type, Codec codec) => InitAndRegister(new TypeCodecInfo(type), codec);

    public void InitAndRegister(ICodecInfo codecInfo, Codec codec) {
        codec.Init(this);
        Codecs[codecInfo] = codec;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public ICodec GetCodec(ICodecInfo codecInfo) {
        if (codecInfo is TypeCodecInfo typeCodecInfo) {
            Type? underlyingType = Nullable.GetUnderlyingType(typeCodecInfo.Type);

            if (underlyingType != null) {
                codecInfo = new TypeCodecInfo(underlyingType,
                    typeCodecInfo.Nullable,
                    typeCodecInfo.Varied,
                    typeCodecInfo.Attributes);
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

            //Logger.Verbose("Created {Codec} with {Factory} for {Info}", codec, codecFactory, codecInfo);

            codec.Init(this);

            return codec;
        }

        throw new ArgumentException($"Codec for {codecInfo} not found");
    }
}
