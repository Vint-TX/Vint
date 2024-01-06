namespace Vint.Core.ECS.Movement;

public struct DiscreteTankControl {
    public const int BitLeft = 0;
    public const int BitRight = 1;
    public const int BitDown = 2;
    public const int BitUp = 3;
    public const int BitWeaponLeft = 4;
    public const int BitWeaponRight = 5;

    public byte Control { get; set; }

    public int MoveAxis {
        get => GetBit(BitUp) - GetBit(BitDown);
        set => SetDiscreteControl(value, BitDown, BitUp);
    }

    public int TurnAxis {
        get => GetBit(BitRight) - GetBit(BitLeft);
        set => SetDiscreteControl(value, BitLeft, BitRight);
    }

    public int WeaponControl {
        get => GetBit(BitWeaponRight) - GetBit(BitWeaponLeft);
        set => SetDiscreteControl(value, BitWeaponLeft, BitWeaponRight);
    }

    int GetBit(int bitNumber) => Control >> bitNumber & 1;

    void SetBit(int bitNumber, int value) {
        int num = ~(1 << bitNumber);
        Control = (byte)(Control & num | (value & 1) << bitNumber);
    }

    void SetDiscreteControl(int value, int negativeBit, int positiveBit) {
        SetBit(negativeBit, 0);
        SetBit(positiveBit, 0);

        if (value > 0)
            SetBit(positiveBit, 1);
        else if (value < 0)
            SetBit(negativeBit, 1);
    }
}