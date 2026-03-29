using System.Globalization;

public static class CsvParsing
{
    private static readonly NumberStyles FloatStyles = NumberStyles.Float | NumberStyles.AllowThousands;

    public static bool TryParseFloat(string value, out float result)
    {
        return float.TryParse(value, FloatStyles, CultureInfo.InvariantCulture, out result);
    }
}
