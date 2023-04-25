using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LinkedinEmails.Smtp
{
    // TODO: check for Greylisting && for Spamhaus blacklist
    public class SmtpClient
    {
        public TcpClient TcpClient { get; private set; }
        public StreamReader StreamReader { get; private set; }
        public StreamWriter StreamWriter { get; private set; }
        public SslStream SslStream { get; private set; }

        public async Task ConnectAsync(string hostname, int port)
        {
            TcpClient = new TcpClient();
            await TcpClient.ConnectAsync(hostname, port);
        }

        public void SetupStreams()
        {
            if (TcpClient != null)
            {
                StreamReader = new StreamReader(TcpClient.GetStream());
                StreamWriter = new StreamWriter(TcpClient.GetStream()) { AutoFlush = true };
            }
        }

        private bool CheckResponse(SmtpResponseCode expectedCode)
        {
            string? response = StreamReader.ReadLine();

            if (Enum.TryParse(response?[..3], out SmtpResponseCode code))
                return code == expectedCode;

            return false;
        }

        private SmtpResponseCode GetResponseCode()
        {
            string? response = StreamReader.ReadLine();

            if (Enum.TryParse(response?[..3], out SmtpResponseCode code))
                return code;

            return SmtpResponseCode.Unknown;
        }
        
        public static TimeSpan? ParseGreylistDuration(string serverResponse)
        {
            Regex regex = new(@"(\d+)\s*(minute(s)?|second(s)?|hour(s)?)", RegexOptions.IgnoreCase);
            Match match = regex.Match(serverResponse);

            if (match.Success)
            {
                int duration = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value.ToLower();

                if (unit.Contains("second"))
                    return TimeSpan.FromSeconds(duration);
                else if (unit.Contains("minute"))
                    return TimeSpan.FromMinutes(duration);
                else if (unit.Contains("hour"))
                    return TimeSpan.FromHours(duration);
            }

            return null;
        }

        public static bool IsGreylisted(string serverResponse)
        {
            Regex regex = new Regex(@"^4\d\d\s");
            if (regex.IsMatch(serverResponse))
            {
                string[] greylistKeywords = { "greylist", "greylisted", "temporary", "try again later" };

                if (Array.Exists(greylistKeywords, keyword => serverResponse.ToLower().Contains(keyword)))
                    return true;
            }

            return false;
        }

        public bool CheckServiceReady()
        {
            if (StreamReader != null)
                return CheckResponse(SmtpResponseCode.ServiceReady);

            return false;
        }

        public SmtpResponseCode SendMessage(string message)
        {
            if (StreamWriter != null)
            {
                StreamWriter.WriteLine(message);
                return GetResponseCode();
            }

            return SmtpResponseCode.Unknown;
        }

        public void Close() => TcpClient?.Close();
    }

    public enum SmtpResponseCode
    {
        Unknown = 0,
        CommandUnrecognized = 500,
        InParametersOrArguments = 501,
        CommandNotImplemented = 502,
        BadSequenceOfCommands = 503,
        CommandParameterNotImplemented = 504,
        SystemStatusOrHelpReply = 211,
        ServiceReadyInNNNMinutes = 214,
        ServiceReady = 220,
        ServiceClosingTransmissionChannel = 221,
        Completed = 250,
        UserNotLocalWillForwardTo = 251,
        CannotVerifyUserWillAttemptDelivery = 252,
        StartMailInput = 354,
        ServiceNotAvailable = 421,
        MailboxUnavailable = 450,
        LocalErrorInProcessing = 451,
        InsufficientSystemStorage = 452,
        MailboxUnavailablePermanent = 550,
        UserNotLocalPleaseTryForwardPath = 551,
        ExceededStorageAllocation = 552,
        MailboxNameNotAllowed = 553,
        TransactionFailed = 554
    }
}
