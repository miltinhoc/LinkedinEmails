namespace LinkedinEmails.Model
{
    public class Email
    {
        public List<string> Emails { get; set; }
        public string EmployeeName { get; set; }

        public Email(List<string> emails, string employeeName)
        {
            Emails = emails;
            EmployeeName = employeeName;
        }
    }
}
