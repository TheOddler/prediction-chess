using System.Linq;

public static class GeneralExtensions
{
    public static bool In<T>(this T needle, params T[] haystack)
    {
        return haystack.Contains(needle);
    }
}