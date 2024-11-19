using Vint.Core.Protocol.Attributes;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Info;

namespace Vint.Core.Protocol.Codecs.Impl;

public class TimeSpanCodec : Codec {
    public TimeSpanCodec(Type numberType, TimeSpanKind kind) {
        NumberType = numberType;
        Kind = kind;
        Codec = Protocol.GetCodec(NumberType);
    }

    ICodec Codec { get; }
    Type NumberType { get; }
    TimeSpanKind Kind { get; }

    public override void Encode(ProtocolBuffer buffer, object value) {
        TimeSpan timeSpan = (TimeSpan)value;
        object time = Convert.ChangeType(GetTimeOfKind(timeSpan, Kind), NumberType);

        Codec.Encode(buffer, time);
    }

    public override object Decode(ProtocolBuffer buffer) {
        double time = (double)Codec.Decode(buffer);
        return GetTimeSpanFromKind(time, Kind);
    }

    static double GetTimeOfKind(TimeSpan timeSpan, TimeSpanKind kind) => kind switch {
        TimeSpanKind.Ticks => timeSpan.Ticks,
        TimeSpanKind.Microseconds => timeSpan.TotalMicroseconds,
        TimeSpanKind.Milliseconds => timeSpan.TotalMilliseconds,
        TimeSpanKind.Seconds => timeSpan.TotalSeconds,
        TimeSpanKind.Minutes => timeSpan.TotalMinutes,
        TimeSpanKind.Hours => timeSpan.TotalHours,
        TimeSpanKind.Days => timeSpan.TotalDays,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    static TimeSpan GetTimeSpanFromKind(double time, TimeSpanKind kind) => kind switch {
        TimeSpanKind.Ticks => TimeSpan.FromTicks((long)time),
        TimeSpanKind.Microseconds => TimeSpan.FromMicroseconds(time),
        TimeSpanKind.Milliseconds => TimeSpan.FromMilliseconds(time),
        TimeSpanKind.Seconds => TimeSpan.FromSeconds(time),
        TimeSpanKind.Minutes => TimeSpan.FromMinutes(time),
        TimeSpanKind.Hours => TimeSpan.FromHours(time),
        TimeSpanKind.Days => TimeSpan.FromDays(time),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}
