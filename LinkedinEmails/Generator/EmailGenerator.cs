using LinkedinEmails.Model;

namespace LinkedinEmails.Generator
{
    public class EmailGenerator
    {
        private readonly string _domain;

        public EmailGenerator(string domain) => _domain = domain;

        public List<EmployeeDTO> Generate(List<Employee> employees)
        {
            List<EmployeeDTO> employeesDTO = new();

            foreach (Employee employee in employees)
            {
                List<string> temporary = new()
                {
                    GetEmail(employee.FirstName, employee.LastNameLetter.ToString()),
                    GetEmail(employee.FirstName),
                    GetEmail(employee.LastName),
                    GetEmail(employee.FirstName, employee.LastName),
                    GetEmail(employee.FirstNameLetter, employee.LastName)
		};

                employeesDTO.Add(new EmployeeDTO(employee.FullName, temporary));
            }

            return employeesDTO;
        }

        private string GetEmail(string part) => $"{part}@{_domain}";
     
        private string GetEmail(string part1, string part2) => $"{part1}.{part2}@{_domain}";
    
    	private string GetEmail(char part1, string part2) => $"{part1}{part2}@{_domain}";
    }
}
