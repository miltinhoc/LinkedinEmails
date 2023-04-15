using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LinkedinEmails.Smtp
{
    public class SmtpClient
    {
        public TcpClient TcpClient { get; private set; }
        public StreamReader StreamReader { get; private set; }
        public StreamWriter StreamWriter { get; private set; }

        public SmtpClient() => TcpClient = new TcpClient();

        public async Task ConnectAsync(string hostname, int port) => await TcpClient.ConnectAsync(hostname, port);

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

        public bool CheckServiceReady()
        {
            if (StreamReader != null)
                return CheckResponse(SmtpResponseCode.ServiceReady);

            return false;
        }

        public bool SendAndCheckResponse(string message, SmtpResponseCode expectedCode)
        {
            if (StreamWriter != null)
            {
                StreamWriter.WriteLine(message);
                return CheckResponse(expectedCode);
            }

            return false;
        }

        public void Close() => TcpClient?.Close();
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
