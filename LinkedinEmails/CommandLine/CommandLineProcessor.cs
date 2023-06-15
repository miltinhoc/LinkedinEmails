namespace LinkedinEmails.CommandLine
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandLineProcessor
    {
        private readonly string _usage = " [*] Usage: LinkedinEmails [-options]";
        public string Email { get; private set; }
        public string Password { get; private set; }
        public string CompanyName { get; private set; }
        public string Domain { get; private set; }
        public bool ValidateEmails { get; private set; }
        public string EmailsFile { get; private set; }
        public string Pin { get; private set; }

        /// <summary>
        /// Takes an array of command-line arguments (args) and checks if they are valid based on the expected format. 
        /// It expects three arguments: email, password, and company name, each prefixed with "-e", "-p", "-c", "-d", "-v", "-f" and "-h", respectively. 
        /// It extracts the values from the arguments and assigns them to the corresponding properties. 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>True if all the arguments are valid and correctly formatted, and False otherwise</returns>
        public bool ParseArguments(string[] args)
        {
            if (args.Length == 1 && (args[0] == "-h"))
            {
                ShowHelp();
                return false;
            }

            if (args.Length == 3)
            {
                ValidateEmails = false;
                EmailsFile = string.Empty;

                for (int i = 0; i < args.Length; i += 2)
                {
                    switch (args[i])
                    {
                        case "-v":
                            i--;
                            ValidateEmails = true;
                            break;
                        case "-f":
                            EmailsFile = args[i + 1];
                            break;
                        default:
                            Console.WriteLine($" [*] Invalid argument(s).\n{_usage}");
                            return false;
                    }
                }

                if (!ValidateEmails || string.IsNullOrEmpty(EmailsFile))
                {
                    Console.WriteLine($" [*] Missing required argument(s).\n{_usage}");
                    return false;
                }

                return true;
            }

            if (args.Length < 8)
            {
                Console.WriteLine($" [*] Invalid number of arguments.\n{_usage}");
                return false;
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-e":
                        Email = args[i + 1];
                        break;
                    case "-p":
                        Password = args[i + 1];
                        break;
                    case "-c":
                        CompanyName = args[i + 1];
                        break;
                    case "-d":
                        Domain = args[i + 1];
                        break;
                    case "-pin":
                        Pin = args[i + 1];
                        break;
                    default:
                        return false;
                }
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(CompanyName) || string.IsNullOrEmpty(Domain))
            {
                Console.WriteLine($" [*] Missing required argument(s).\n{_usage}");
                return false;
            }

            return true;
        }

        private static void ShowHelp()
        {
            string c = @"Usage: LinkedinEmails [-options]

options:
	-e <email>		your linkedin account email
	-p <password>		your linkedin account password
	-c <company name>	linkedin company email 
	-d <company domain>	email domain	
	-v			tries to validate the emails
	-f <file path>		generated emails filepath
    -pin <auth pin>     authentication pin
	-h			show this help message and exit";

            Console.WriteLine(c);
        }
    }
}
