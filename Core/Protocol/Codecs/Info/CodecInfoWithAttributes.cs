using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Info;

public class CodecInfoWithAttributes {
    public CodecInfoWithAttributes(Type type, bool nullable = false, bool varied = false, params Attribute[] attributes) :
        this(new CodecInfo(type, nullable, varied), attributes) { }

    public CodecInfoWithAttributes(ICodecInfo codecInfo, params Attribute[] attributes) :
        this(codecInfo, (IEnumerable<Attribute>)attributes) { }

    public CodecInfoWithAttributes(Type type, bool nullable, bool varied, IEnumerable<Attribute> attributes) :
        this(new CodecInfo(type, nullable, varied), attributes) { }

    public CodecInfoWithAttributes(ICodecInfo codecInfo, IEnumerable<Attribute> attributes) {
        CodecInfo = codecInfo;

        foreach (Attribute attribute in attributes)
            AddAttribute(attribute);
    }

    public ICodecInfo CodecInfo { get; }
    Dictionary<Type, Attribute> Attributes { get; } = [];

    public void AddAttribute(Attribute attribute) => Attributes[attribute.GetType()] = attribute;

    public bool HasAttribute(Type type) => Attributes.ContainsKey(type);

    public Attribute GetAttribute(Type type) => Attributes[type];

    public bool HasAttribute<T>() where T : Attribute => HasAttribute(typeof(T));

    public T GetAttribute<T>() where T : Attribute => (T)GetAttribute(typeof(T));

    public override string ToString() => $"CodecInfoWithAttributes {{ " +
                                         $"CodecInfo: {CodecInfo}; " +
                                         $"Attributes: {{ {Attributes.ToString(false)} }} }}";
}
