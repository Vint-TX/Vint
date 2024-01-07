using System.Numerics;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class Vector3Codec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Vector3 vector3 = (Vector3)value;
        buffer.Writer.Write(vector3.X);
        buffer.Writer.Write(vector3.Y);
        buffer.Writer.Write(vector3.Z);
    }

    public override object Decode(ProtocolBuffer buffer) =>
        new Vector3(buffer.Reader.ReadSingle(), buffer.Reader.ReadSingle(), buffer.Reader.ReadSingle());
}