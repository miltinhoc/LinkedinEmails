namespace LinkedinEmails.Model
{
    public class ValidEmployee
    {
        public string FullName { get; set; }
        public string Email { get; set; }

        public ValidEmployee(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
        }
    }
}
