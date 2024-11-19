﻿using System.Diagnostics;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;
using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class TemplateAccessorCodec : Codec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        if (value is not TemplateAccessor templateAccessor)
            throw new ArgumentException("Value must be TemplateAccessor");

        Protocol.GetCodec(typeof(long))
            .Encode(buffer, templateAccessor.Template.GetType().GetProtocolId());

        Protocol.GetCodec(new CodecInfoWithAttributes(typeof(string), true)).Encode(buffer, templateAccessor.ConfigPath!);
    }

    public override object Decode(ProtocolBuffer buffer) => throw new UnreachableException();
}
