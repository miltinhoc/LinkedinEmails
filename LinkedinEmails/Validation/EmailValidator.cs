using DnsClient;
using LinkedinEmails.Smtp;
using System.Net;

namespace LinkedinEmails.Validation
{
    public class EmailValidator
    {
        private static string GetMxRecord(string domain)
        {
            try
            {
                LookupClientOptions options = new()
                {
                    UseCache = true,
                    ContinueOnDnsError = false,
                    EnableAuditTrail = true
                };

                LookupClient lookupClient = new(options);
                var result = lookupClient.Query(domain, QueryType.MX);

                if (result.HasError)
                {
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                    return string.Empty;
                }

                var mxRecords = result.Answers.MxRecords().OrderBy(record => record.Preference);

                return mxRecords.First().Exchange.Value;
            }
            catch
            {
            }
            return string.Empty;
        }

        private static string GetDomainFromEmail(string email)
        {
            int atIndex = email.IndexOf('@');
            return email.Substring(atIndex + 1);
        }

        public static async Task<bool> IsValidEmail(string email)
        {
            string domain = GetDomainFromEmail(email);
            string mxRecord = GetMxRecord(domain);

            if (!string.IsNullOrEmpty(mxRecord))
                return await CheckEmailWithSmtp(mxRecord, email);
            
            return false;
        }

        public static async Task<bool> CheckEmailWithSmtp(string mailServer, string email)
        {
            SmtpClient smtpClient = new();

            try
            {
                await smtpClient.ConnectAsync(mailServer, 25);
                smtpClient.SetupStreams();

                if (smtpClient.CheckServiceReady())
                {
                    if (smtpClient.SendMessage($"HELO {Dns.GetHostName()}") == SmtpResponseCode.Completed)
                    {
                        if (smtpClient.SendMessage("MAIL FROM:<linkedin2emails@gmail.com>") == SmtpResponseCode.Completed)
                        {
                            if (smtpClient.SendMessage($"RCPT TO:<{email}>") == SmtpResponseCode.Completed)
                            {
                                smtpClient.SendMessage("QUIT");
                                smtpClient.Close();

                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
            }

            if (smtpClient.TcpClient.Connected)
                smtpClient.SendMessage("QUIT");

            smtpClient.Close();

            return false;
        }
    }
}
