using System.Net;
using System.Net.Sockets;

namespace LinkedinEmails.Validation
{
    public class EmailValidator
    {
        public static bool CheckEmailWithSmtp(string mailServer, string email)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(mailServer, 25);
                    using (var streamReader = new StreamReader(client.GetStream()))
                    using (var streamWriter = new StreamWriter(client.GetStream()) { AutoFlush = true })
                    {
                        if (!CheckResponse(streamReader, SmtpResponseCode.ServiceReady))
                        {
                            return false;
                        }

                        streamWriter.WriteLine("HELO " + Dns.GetHostName());
                        if (!CheckResponse(streamReader, SmtpResponseCode.RequestedMailActionOkayCompleted))
                        {
                            return false;
                        }

                        streamWriter.WriteLine("MAIL FROM:<ogameninjapp@gmail.com> ");
                        if (!CheckResponse(streamReader, SmtpResponseCode.RequestedMailActionOkayCompleted))
                        {
                            return false;
                        }

                        streamWriter.WriteLine("RCPT TO:<" + email + ">");
                        if (!CheckResponse(streamReader, SmtpResponseCode.RequestedMailActionOkayCompleted))
                        {
                            return false;
                        }

                        streamWriter.WriteLine("QUIT");
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool CheckResponse(StreamReader streamReader, SmtpResponseCode expectedCode)
        {
            string? response = streamReader.ReadLine();
            if (Enum.TryParse(response?.Substring(0, 3), out SmtpResponseCode code))
            {
                return code == expectedCode;
            }
            return false;
        }
    }

    public enum SmtpResponseCode
    {
        SyntaxErrorCommandUnrecognized = 500,
        SyntaxErrorInParametersOrArguments = 501,
        CommandNotImplemented = 502,
        BadSequenceOfCommands = 503,
        CommandParameterNotImplemented = 504,
        SystemStatusOrHelpReply = 211,
        ServiceReadyInNNNMinutes = 214,
        ServiceReady = 220,
        ServiceClosingTransmissionChannel = 221,
        RequestedMailActionOkayCompleted = 250,
        UserNotLocalWillForwardTo = 251,
        CannotVerifyUserWillAttemptDelivery = 252,
        StartMailInput = 354,
        ServiceNotAvailable = 421,
        RequestedMailActionNotTakenMailboxUnavailable = 450,
        RequestedActionAbortedLocalErrorInProcessing = 451,
        RequestedActionNotTakenInsufficientSystemStorage = 452,
        RequestedActionNotTakenMailboxUnavailablePermanent = 550,
        UserNotLocalPleaseTryForwardPath = 551,
        RequestedMailActionAbortedExceededStorageAllocation = 552,
        RequestedActionNotTakenMailboxNameNotAllowed = 553,
        TransactionFailed = 554,
    }
}
