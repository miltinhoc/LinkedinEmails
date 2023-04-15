using LinkedinEmails.EmailUtils;
using LinkedinEmails.Model;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Text.RegularExpressions;
using LinkedinEmails.Constants;
using LinkedinEmails.Helper;

namespace LinkedinEmails
{
    public class Client
    {
        private readonly BrowserFetcher _browserFetcher;
        private IBrowser _browser;
        private IPage _browserPage;
        private int _lastPage = 1;
        private string _searchPageLink;

        private readonly List<Employee> _employees;
        private readonly EmailGenerator emailGenerator;

        /// <summary>
        /// Constructor
        /// </summary>
        public Client(string domain)
        {
            emailGenerator = new EmailGenerator(domain);
            _employees = new List<Employee>();
            _browserFetcher = new BrowserFetcher();
        }

        public async Task Close()
        {
            try
            {
                await _browserPage.CloseAsync();
                await _browser.CloseAsync();
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static async Task ScrollToEndOfPageAsync(Page page)
        {
            await page.EvaluateFunctionAsync(LinkedinClasses.JsScrollDown);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        private static string GenerateJsSearchPage(string selector)
        {
            return $"() => {{return document.querySelector('{selector}').href;}}";
        }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateAndSaveEmails()
        {
            List<EmployeeDTO> employeeDTOs = emailGenerator.Generate(_employees);
            SaveFile(employeeDTOs);
        }

        private async Task CheckAndDownloadRevision()
        {
            List<string> revisions = _browserFetcher.LocalRevisions().ToList();

            if (revisions.Count == 0)
            {
                Logging.Logger.Print("downloading chromium driver...", Logging.LogType.INFO);
                await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            }
        }

        /// <summary>
        /// Downloads chromium driver if not already present and launches puppeteer
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            Logging.Logger.Print("initializing...", Logging.LogType.INFO);

            await CheckAndDownloadRevision();
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                DefaultViewport = new ViewPortOptions { Width = 1200, Height = 900}
            });

            _browserPage = await _browser.NewPageAsync();
            await _browserPage.SetUserAgentAsync(LinkedinClasses.UserAgent);

            Logging.Logger.Print("initialized with success", Logging.LogType.INFO);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private async Task<bool> FindCompanyEmployeesPageAsync(string className)
        {
            if (await WaitFor(className))
            {
                _searchPageLink = await _browserPage.EvaluateFunctionAsync<string>(GenerateJsSearchPage(className));
                Logging.Logger.Print("found company employees page", Logging.LogType.INFO);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Navigates to a LinkedIn company page based on the provided companyName, 
        /// waits for the employees link to load, and then retrieves and stores the company employees search page link
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public async Task SetCompanyPageAsync(string companyName)
        {
            await _browserPage.GoToAsync($"https://www.linkedin.com/company/{EscapeSpecialCharacters(companyName)}");


            bool foundCompanyEmployeesPage = await FindCompanyEmployeesPageAsync(LinkedinClasses.EmployeesLinkAllClassName) || 
                await FindCompanyEmployeesPageAsync(LinkedinClasses.EmployeesLinkClassName);

            if (!foundCompanyEmployeesPage)
            {
                Logging.Logger.Print("failed to find company employees page", Logging.LogType.INFO);
            }
        }

        private static string EscapeSpecialCharacters(string input)
        {
            string pattern = @"[^\w\.@-]";
            return Regex.Replace(input, pattern, m => Uri.EscapeDataString(m.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<bool> WaitFor(string selector, int timeout = 10000)
        {
            try
            {
                await _browserPage.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = timeout });
                return true;
            }catch(Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
                return false;
            }
        }

        /// <summary>
        /// Visits Linkedin Sign In page and tries to login
        /// </summary>
        /// <param name="email">Linkedin email</param>
        /// <param name="password">Linkedin password</param>
        /// <returns>True if the logins is successful, otherwise returns false</returns>
        public async Task<bool> TryLoginAsync(string email, string password)
        {
            try
            {
                Logging.Logger.Print("attempting to login...", Logging.LogType.INFO);

                if (!await VisitAndWaitAsync("https://www.linkedin.com/", LinkedinClasses.IdPasswordInput))
                {
                    Logging.Logger.Print("failed to load linkedin", Logging.LogType.ERROR);
                    return false;
                }

                await _browserPage.EvaluateExpressionAsync($"document.querySelector('{LinkedinClasses.IdEmailInput}').value ='{email}'");
                await _browserPage.EvaluateExpressionAsync($"document.querySelector('{LinkedinClasses.IdPasswordInput}').value ='{password}'");
                await _browserPage.EvaluateExpressionAsync($"document.querySelector('{LinkedinClasses.SelectorLoginButton}').click()");

                await _browserPage.WaitForSelectorAsync(LinkedinClasses.LinkedinNavbarClassName);
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
                return false;
            }

            Logging.Logger.Print("login successful", Logging.LogType.INFO);
            return true;
        }

        /// <summary>
        /// Determines the number of pages on the company employees list results
        /// </summary>
        /// <returns></returns>
        public async Task SetSearchLastPageAsync()
        {
            await _browserPage.GoToAsync(_searchPageLink);
            await ScrollToEndOfPageAsync((Page)_browserPage);
            await WaitFor(LinkedinClasses.PageSelectorClassName, 15000);

            try
            {
                string page = await _browserPage.EvaluateFunctionAsync<string>(LinkedinClasses.JsGetLastPage);
                _lastPage = Convert.ToInt32(page);

                Logging.Logger.Print($"found number of pages ({_lastPage})", Logging.LogType.INFO);
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SearchLoopAsync()
        {
            for (int i = 1; i < _lastPage + 1; i++)
            {
                if (i != 1)
                    ConsoleHelper.ClearLastConsoleLine();

                Logging.Logger.Print($"extracting page {i}/{_lastPage}", Logging.LogType.INFO);

                if (!await VisitAndWaitAsync($"{_searchPageLink}&page={i}", LinkedinClasses.ResultEntityClassName))
                {
                    continue;
                }

                List<string> temp = await _browserPage.EvaluateExpressionAsync<List<string>>(LinkedinClasses.JsGetEmployeeNames);
                AddEmployees(temp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list">Employees full name list</param>
        private void AddEmployees(List<string> list)
        {
            foreach (string employee in list)
            {
                _employees.Add(new Employee(employee));
            }
        }

        /// <summary>
        /// Visits a webpage and waits for a selector
        /// </summary>
        /// <param name="url"></param>
        /// <param name="selector"></param>
        /// <returns>True if it visits the webpage successfully and finds the selector, otherwise false</returns>
        private async Task<bool> VisitAndWaitAsync(string url, string selector, int timeout = 15000)
        {
            try
            {
                await _browserPage.GoToAsync(url);
                await _browserPage.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = timeout });
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves Employees list to file
        /// </summary>
        private static void SaveFile<T>(List<T> obj)
        {
            string filename = $"emails-{DateTime.Now:yyyy-dd-M-HH-mm-ss}.json";

            try
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(obj, Formatting.Indented));
                Logging.Logger.Print($"saved output to {filename}", Logging.LogType.INFO);
            }
            catch (Exception ex)
            {
                Logging.Logger.Print(ex.Message, Logging.LogType.ERROR);
            }
        }
    }
}
