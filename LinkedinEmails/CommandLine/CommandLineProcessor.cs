using System.Text;

namespace LinkedinEmails.CommandLine
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandLineProcessor
    {
        private readonly string _usage = "Usage: EmployeeSearch.exe -e=<email> -p=<password> -c=<companyName> -d=<domain>";
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string CompanyName { get; private set; }
        public string Domain { get; private set; }

        /// <summary>
        /// Takes an array of command-line arguments (args) and checks if they are valid based on the expected format. 
        /// It expects three arguments: email, password, and company name, each prefixed with "-e=", "-p=", and "-c=", "-d=", respectively. 
        /// It extracts the values from the arguments and assigns them to the corresponding properties. 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>True if all the arguments are valid and correctly formatted, and False otherwise</returns>
        public bool ParseArguments(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine($" [*] Invalid number of arguments. {_usage}");
                return false;
            }

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case string temp when temp.StartsWith("-e="):
                        Email = arg.Substring(3);
                        break;
                    case string temp when temp.StartsWith("-p="):
                        Password = arg.Substring(3);
                        break;
                    case string temp when temp.StartsWith("-c="):
                        CompanyName = arg.Substring(3);
                        break;
                    case string temp when temp.StartsWith("-d="):
                        Domain = arg.Substring(3);
                        break;
                    default:
                        return false;
                }
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(CompanyName) || string.IsNullOrEmpty(Domain))
            {
                Console.WriteLine($" [*] Missing required argument(s). {_usage}");
                return false;
            }

            return true;
        }
    }
}
