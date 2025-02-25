﻿namespace Vint.Core.Server.Game.Protocol.Codecs;

public interface ITypeCodecInfo : ICodecInfo {
    Type Type { get; }
    bool Nullable { get; }
    bool Varied { get; }
    HashSet<Attribute> Attributes { get; }
}
