namespace MainProcessor
{
    public static class StringExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static string TrimEnd(this string str, string trimString)
        {
            if (str.EndsWith(trimString))
            {
                return str.Substring(0, str.Length - trimString.Length);
            }
            return str;
        }
    }
}
