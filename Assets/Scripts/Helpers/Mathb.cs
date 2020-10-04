public static class Mathb
{
    public static byte Clamp(byte value, byte min, byte max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static byte RoundToByte(float value)
    {
        return (byte)(value + 0.5f);
    }
}