using System.Collections;
using Vint.Core.ECS.Movement;
using Vint.Core.Protocol.Codecs.Buffer;

namespace Vint.Core.Protocol.Codecs.Impl;

public class MoveCommandCodec : MoveCodec {
    public override void Encode(ProtocolBuffer buffer, object value) {
        MoveCommand moveCommand = (MoveCommand)value;

        bool movementHasValue = moveCommand.Movement.HasValue;
        bool weaponRotationHasValue = moveCommand.WeaponRotation.HasValue;
        bool isDiscrete = moveCommand.IsDiscrete();

        buffer.OptionalMap.Add(movementHasValue);
        buffer.OptionalMap.Add(weaponRotationHasValue);
        buffer.OptionalMap.Add(isDiscrete);

        if (isDiscrete) {
            DiscreteTankControl discreteTankControl = new() {
                MoveAxis = (int)moveCommand.TankControlVertical!,
                TurnAxis = (int)moveCommand.TankControlHorizontal!,
                WeaponControl = (int)moveCommand.WeaponRotationControl!
            };

            buffer.Writer.Write(discreteTankControl.Control);
        } else {
            buffer.Writer.Write(moveCommand.TankControlVertical!.Value);
            buffer.Writer.Write(moveCommand.TankControlHorizontal!.Value);
            buffer.Writer.Write(moveCommand.WeaponRotationControl!.Value);
        }

        if (movementHasValue)
            Protocol.GetCodec(new TypeCodecInfo(typeof(Movement))).Encode(buffer, moveCommand.Movement!);

        if (weaponRotationHasValue) {
            int position = 0;
            byte[] bytes = GetBufferForWeaponRotation(weaponRotationHasValue);
            BitArray bits = GetBitsForWeaponRotation(weaponRotationHasValue);

            WriteFloat(bits, ref position, moveCommand.WeaponRotation!.Value, WeaponRotationComponentBitsize, WeaponRotationFactor);

            bits.CopyTo(bytes, 0);
            buffer.Writer.Write(bytes);

            if (position != bits.Length)
                throw new Exception("Move command pack mismatch");
        }

        buffer.Writer.Write(moveCommand.ClientTime);
    }

    public override object Decode(ProtocolBuffer buffer) {
        MoveCommand moveCommand = default;

        bool movementHasValue = buffer.OptionalMap.Get();
        bool weaponRotationHasValue = buffer.OptionalMap.Get();
        bool isDiscrete = buffer.OptionalMap.Get();

        if (isDiscrete) {
            DiscreteTankControl discreteTankControl = new() {
                Control = buffer.Reader.ReadByte()
            };

            moveCommand.TankControlHorizontal = discreteTankControl.TurnAxis;
            moveCommand.TankControlVertical = discreteTankControl.MoveAxis;
            moveCommand.WeaponRotationControl = discreteTankControl.WeaponControl;
        } else {
            moveCommand.TankControlVertical = buffer.Reader.ReadSingle();
            moveCommand.TankControlHorizontal = buffer.Reader.ReadSingle();
            moveCommand.WeaponRotationControl = buffer.Reader.ReadSingle();
        }

        if (movementHasValue)
            moveCommand.Movement = (Movement)Protocol.GetCodec(new TypeCodecInfo(typeof(Movement))).Decode(buffer);

        int position = 0;
        byte[] bytes = GetBufferForWeaponRotation(weaponRotationHasValue);
        BitArray bits = GetBitsForWeaponRotation(weaponRotationHasValue);

        _ = buffer.Reader.Read(bytes, 0, bytes.Length);
        CopyBits(bytes, bits);

        if (weaponRotationHasValue)
            moveCommand.WeaponRotation = ReadFloat(bits, ref position, WeaponRotationComponentBitsize, WeaponRotationFactor);

        if (position != bits.Length)
            throw new Exception("Move command unpack mismatch");

        moveCommand.ClientTime = buffer.Reader.ReadInt32();
        return moveCommand;
    }
}