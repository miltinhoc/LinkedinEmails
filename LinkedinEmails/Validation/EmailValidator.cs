using LinkedinEmails.Smtp;
using System.Net;

namespace LinkedinEmails.Validation
{
    public class EmailValidator
    {
        public static async Task<bool> CheckEmailWithSmtp(string mailServer, string email)
        {
            SmtpClient smtpClient = new();

            try
            {
                await smtpClient.ConnectAsync(mailServer, 25);
                smtpClient.SetupStreams();

                if (smtpClient.CheckServiceReady())
                {
                    if (smtpClient.SendAndCheckResponse($"HELO {Dns.GetHostName()}", SmtpResponseCode.RequestedMailActionOkayCompleted))
                    {
                        if (smtpClient.SendAndCheckResponse("MAIL FROM:<ogameninjapp@gmail.com> ", SmtpResponseCode.RequestedMailActionOkayCompleted))
                        {
                            if (smtpClient.SendAndCheckResponse($"RCPT TO:<{email}>", SmtpResponseCode.RequestedMailActionOkayCompleted))
                            {
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

            smtpClient.Close();
            return false;
        }
    }
}
