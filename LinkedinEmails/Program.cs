using LinkedinEmails.CommandLine;
using LinkedinEmails.Logging;

namespace LinkedinEmails
{
    internal class Program
    {
        private static Client _client;
        private static CommandLineProcessor _processor;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            _processor = new();

            if (!_processor.ParseArguments(args))
                return;

            Header.Draw();

            if (_processor.ValidateEmails)
            {
                try
                {
                    await Client.ValidateAndSaveEmails(_processor.EmailsFile);
                }
                catch (Exception ex)
                {
                    Logger.Print(ex.Message, LogType.ERROR);
                }
            }
            else
            {
                await StartGathering();
            }
        }

        private static async Task StartGathering()
        {
            _client = new(_processor.Domain);
            await _client.InitAsync();

            if (await _client.TryLoginAsync(_processor.Email, _processor.Password))
            {
                try
                {
                    if (!string.IsNullOrEmpty(_processor.Pin))
                    {
                        await _client.InsertAuthenticationPin(_processor.Pin);
                    }

                    await _client.SetCompanyPageAsync(_processor.CompanyName);
                    await _client.SetSearchLastPageAsync();
                    await _client.SearchLoopAsync();

                    _client.GenerateAndSaveEmails();
                }
                catch (Exception ex)
                {
                    Logger.Print(ex.Message, LogType.ERROR);
                }
            }

            await _client.CloseAsync();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            if (_client != null)
                _client.CloseAsync().Wait();
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            if (_client != null)
                _client.CloseAsync().Wait();
        }
    }
}