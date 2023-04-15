namespace LinkedinEmails.Extension
{
    public static class EmployeeExtension
    {
        private static readonly Dictionary<string, string> Chars = new()
        {
            { "àáâãäå", "a" },
            { "èéêë", "e" },
            { "ìíîï", "i" },
            { "òóôõö", "o" },
            { "ùúûü", "u" },
            { "ñ", "n" },
            { "ýÿ", "y" },
            { "ç", "c" },
            { ",.", "" }
        };

        public static string CleanSpecialCharacters(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            foreach (KeyValuePair<string, string> kvp in Chars)
            {
                try
                {
                    value = new string(value.Select(c => kvp.Key.Contains(c) ? kvp.Value[0] : c).ToArray());
                }
                catch { }
            }

            return value;
        }
    }
}
