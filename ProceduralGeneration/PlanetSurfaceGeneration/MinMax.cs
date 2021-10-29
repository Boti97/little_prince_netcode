public class MinMax
{
    public MinMax()
    {
        Min = float.MaxValue;
        Max = float.MinValue;
    }

    public float Min { get; private set; }
    public float Max { get; private set; }

    public void AddValue(float value)
    {
        if (value > Max)
        {
            Max = value;
        }

        if (value < Min)
        {
            Min = value;
        }
    }
}