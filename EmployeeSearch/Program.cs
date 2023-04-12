using EmployeeSearch.CommandLine;

namespace EmployeeSearch
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CommandLineProcessor processor = new();

            if (processor.ParseArguments(args))
            {
                Client client = new();
                await client.InitAsync();

                if (await client.TryLoginAsync(processor.Email, processor.Password))
                {
                    await client.SetCompanyPageAsync(processor.CompanyName);
                    await client.SetSearchLastPageAsync();

                    await client.SearchLoopAsync();
                    client.SaveFile();
                }
            }
        }
    }
}