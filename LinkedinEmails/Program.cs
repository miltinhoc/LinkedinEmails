using LinkedinEmails.CommandLine;

namespace LinkedinEmails
{
    internal class Program
    {
        private static Client _client;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            CommandLineProcessor processor = new();

            if (processor.ParseArguments(args))
            {
                Header.Draw();

                _client = new(processor.Domain);
                await _client.InitAsync();

                if (await _client.TryLoginAsync(processor.Email, processor.Password))
                {
                    try
                    {
                        await _client.SetCompanyPageAsync(processor.CompanyName);
                        await _client.SetSearchLastPageAsync();
                        await _client.SearchLoopAsync();

                        _client.GenerateAndSaveEmails();

                        if (processor.ValidateEmails)
                        {
                            await _client.ValidateAndSaveEmails();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
                    }
                }

                await _client.CloseAsync();
            }
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