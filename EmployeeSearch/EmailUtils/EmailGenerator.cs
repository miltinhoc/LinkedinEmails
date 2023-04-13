using EmployeeSearch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeSearch.EmailUtils
{
    public class EmailGenerator
    {
        private readonly string _domain;
        private readonly List<Employee> _employees;
        private readonly List<Email> _emails;

        public EmailGenerator(string domain, List<Employee> employees)
        {
            _domain = domain;
            _employees = employees;
        }

        public void Generate()
        {
            foreach (Employee employee in _employees)
            {
                List<string> temporary = new();

            }
        }

        private string GetEmail(string part)
        {
            return $"{part}@{_domain}";
        }

        private string GetEmail(string part1, string part2)
        {
            return $"{part1}.{part2}@{_domain}";
        }
    }
}
