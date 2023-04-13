using LinkedinEmails.CommandLine;

namespace LinkedinEmails
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CommandLineProcessor processor = new();

            if (processor.ParseArguments(args))
            {
                Client client = new(processor.Domain);
                await client.InitAsync();

                if (await client.TryLoginAsync(processor.Email, processor.Password))
                {
                    try
                    {
                        await client.SetCompanyPageAsync(processor.CompanyName);
                        await client.SetSearchLastPageAsync();
                        await client.SearchLoopAsync();

                        client.GenerateAndSaveEmails();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
                    }
                }
            }
        }
    }
}