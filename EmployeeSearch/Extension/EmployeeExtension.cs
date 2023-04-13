using EmployeeSearch.Model;

namespace EmployeeSearch.Extension
{
    public static class EmployeeExtension
    {
        private static readonly Dictionary<string, string> Chars = new Dictionary<string, string>
        {
            { "àáâãäå", "a" },
            { "èéêë", "e" },
            { "ìíîï", "i" },
            { "òóôõö", "o" },
            { "ùúûü", "u" },
            { "ñ", "n" },
            { "ýÿ", "y" },
            { "ç", "c" }
        };

        public static void CleanSpecialCharacters(this Employee employee)
        {
            foreach (KeyValuePair<string, string> kvp in Chars)
            {
                employee.FullName = new string(employee.FullName.Select(c => kvp.Key.Contains(c) ? kvp.Value.ToCharArray()[0] : c).ToArray());
            }
        }
    }
}
