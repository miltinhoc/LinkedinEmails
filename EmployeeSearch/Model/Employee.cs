namespace EmployeeSearch.Model
{
    public class Employee
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public char FirstNameLetter { get; set; }
        public char LastNameLetter { get; set; }

        public Employee (string fullName)
        {
            FullName = fullName;
        }
    }
}
