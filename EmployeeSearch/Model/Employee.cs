using EmployeeSearch.Extension;

namespace EmployeeSearch.Model
{
    public class Employee
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public char FirstNameLetter { get; set; }
        public char LastNameLetter { get; set; }
        public List<string> EmailList { get; set; }

        public Employee (string fullName)
        {
            EmailList = new List<string>();
            FullName = fullName;

            SetFirstAndLastName();
            SetFirstAndLastLetter();
        }

        private void SetFirstAndLastName()
        {
            string[] names = FullName.Split(' ');

            FirstName = names.FirstOrDefault()?.ToLower().CleanSpecialCharacters();
            LastName = names.LastOrDefault()?.ToLower().CleanSpecialCharacters();
        }

        private void SetFirstAndLastLetter()
        {
            FirstNameLetter = FirstName.FirstOrDefault();
            LastNameLetter = LastName.FirstOrDefault();
        }

        public void AddEmails(List<string> emailList) => EmailList.AddRange(emailList);
    }
}
