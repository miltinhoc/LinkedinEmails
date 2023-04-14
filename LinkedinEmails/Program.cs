using LinkedinEmails.CommandLine;

namespace LinkedinEmails
{
    internal class Program
    {
        private static Client _client;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
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
                    }
                    catch (Exception ex)
                    {
                        Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
                    }
                }

                await _client.Close();
            }
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _client.Close().Wait();
        }
    }
}