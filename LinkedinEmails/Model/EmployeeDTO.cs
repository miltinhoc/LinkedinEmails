namespace LinkedinEmails.Model
{
    public class EmployeeDTO
    {
        public string FullName { get; set; }
        public List<string> EmailList { get; set; }

        public EmployeeDTO(string fullName, List<string> emailList)
        {
            FullName = fullName;
            EmailList = emailList;
        }
    }
}
