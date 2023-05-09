#region

using System;

#endregion

/// <summary>
///     Helper class for checking if a float is between two floats.
/// </summary>
[Serializable]
public class Interval
{
    public float Minimum;
    public float Maximum;

    /// <summary>
    ///     Creates a new interval
    /// </summary>
    /// <param name="f">The minimum of this interval.</param>
    /// <param name="f1">The maximum of this interval.</param>
    public Interval(float f, float f1)
    {
        Maximum = f;
        Minimum = f1;
    }

    public bool Contains(float f)
    {
        return f >= Minimum && f <= Maximum;
    }
}