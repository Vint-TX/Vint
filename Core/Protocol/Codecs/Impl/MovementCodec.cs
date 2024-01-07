using System.Collections;
using Vint.Core.ECS.Movement;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class MovementCodec : MoveCodec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        Movement movement = (Movement)value;

        int position = 0;
        byte[] array = new byte[MovementSize];
        BitArray bitArray = new(array);

        WriteVector3(bitArray, ref position, movement.Position, PositionComponentBitsize, PositionFactor);
        WriteQuaternion(bitArray, ref position, movement.Orientation, OrientationComponentBitsize, OrientationPrecision);
        WriteVector3(bitArray, ref position, movement.Velocity, LinearVelocityComponentBitsize, VelocityFactor);
        WriteVector3(bitArray, ref position, movement.AngularVelocity, AngularVelocityComponentBitsize, AngularVelocityFactor);

        bitArray.CopyTo(array, 0);
        buffer.Writer.Write(array);

        if (position != bitArray.Length)
            throw new Exception("Movement pack mismatch");
    }

    public override object Decode(ProtocolBuffer buffer) {
        Movement movement = default;

        int position = 0;
        byte[] array = new byte[MovementSize];
        BitArray bitArray = new(array);

        _ = buffer.Reader.Read(array, 0, array.Length);
        CopyBits(array, bitArray);

        movement.Position = ReadVector3(bitArray, ref position, PositionComponentBitsize, PositionFactor);
        movement.Orientation = ReadQuaternion(bitArray, ref position, OrientationComponentBitsize, OrientationPrecision);
        movement.Velocity = ReadVector3(bitArray, ref position, LinearVelocityComponentBitsize, VelocityFactor);
        movement.AngularVelocity = ReadVector3(bitArray, ref position, AngularVelocityComponentBitsize, AngularVelocityFactor);

        if (position != bitArray.Length)
            throw new Exception("Movement unpack mismatch");

        return movement;
    }
}