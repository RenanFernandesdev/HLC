namespace HLC.Util
{
    internal class FormatData
    {
        public static string FormatTd(string field)
        {
            return $"'{field.Replace(".", ",").Replace("%", "").Replace("/", "-")}";
        }
    }
}
