using LinkedinEmails.Generator;
using LinkedinEmails.Model;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Text.RegularExpressions;
using LinkedinEmails.Constants;
using LinkedinEmails.Helper;
using LinkedinEmails.Validation;

namespace LinkedinEmails
{
    public class Client
    {
        private readonly BrowserFetcher _browserFetcher;
        private IBrowser _browser;
        private IPage _browserPage;
        private int _lastPage = 1;
        private string _searchPageLink;

        private readonly List<Employee> employeesList;
        private readonly EmailGenerator emailGenerator;
        private readonly EmailValidator emailValidator;

        /// <summary>
        /// Constructor
        /// </summary>
        public Client(string domain)
        {
            emailGenerator = new EmailGenerator(domain);
            employeesList = new List<Employee>();
            _browserFetcher = new BrowserFetcher();
        }

        /// <summary>
        /// Closes the browser page and the browser instance asynchronously, handling any exceptions that may occur.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
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
        /// Scrolls to the end of the given browser page asynchronously by executing a JavaScript function.
        /// </summary>
        /// <param name="page">The browser page on which the scroll action is to be performed.</param>
        /// <returns>A task that represents the asynchronous scroll operation.</returns>
        private static async Task ScrollToEndOfPageAsync(Page page)
        {
            await page.EvaluateFunctionAsync(LinkedinClasses.JsScrollDown);
        }

        /// <summary>
        /// Generates a JavaScript function as a string to retrieve the href attribute of an element using the given CSS selector.
        /// </summary>
        /// <param name="selector">The CSS selector to be used to query the target element.</param>
        /// <returns>A string containing the JavaScript function.</returns>
        private static string GenerateJsSearchPage(string selector)
        {
            return $"() => {{return document.querySelector('{selector}').href;}}";
        }

        /// <summary>
        /// Generates email addresses for a list of employees using the email generator and saves them to a file.
        /// </summary>
        public void GenerateAndSaveEmails()
        {
            List<EmployeeDTO> employeeDTOs = emailGenerator.Generate(employeesList);
            SaveFile(employeeDTOs);
        }

        /// <summary>
        /// Checks the availability of the Chromium driver's local revisions and downloads the default revision asynchronously if no revisions are found.
        /// </summary>
        /// <returns>A task that represents the asynchronous check and download operation.</returns>
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
        /// Initializes the browser instance and browser page asynchronously, setting up the required configurations, and ensuring that the Chromium driver is available.
        /// </summary>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
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
        /// Searches for a company's employees page using the given class name asynchronously, and stores the link to the page if found.
        /// </summary>
        /// <param name="className">The CSS class name to be used to query the target element.</param>
        /// <returns>A boolean result indicating if the employees page was found.</returns>
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
        /// Adds employees to the instance's employee list by converting each string in the input list to an Employee object.
        /// </summary>
        /// <param name="list">A list of strings, each representing an employee's name.</param>
        private void AddEmployees(List<string> list)
        {
            foreach (string employee in list)
            {
                employeesList.Add(new Employee(employee));
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
