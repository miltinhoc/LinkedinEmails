using EmployeeSearch.EmailUtils;
using EmployeeSearch.Model;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Text.RegularExpressions;

namespace EmployeeSearch
{
    public class Client
    {
        private readonly BrowserFetcher _browserFetcher;
        private readonly ViewPortOptions _viewPortOptions;
        private IBrowser _browser;
        private IPage _browserPage;

        private static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36";
        private static readonly string JsGetEmployeeNames = "Array.from(document.querySelectorAll('.entity-result__title-text a span[dir] span[aria-hidden]')).map(a => a.innerText);";
        private static readonly string JsGetLastPage = "() => {var elem = document.querySelectorAll('.artdeco-pagination__indicator.artdeco-pagination__indicator--number');return elem[elem.length-1].getAttribute('data-test-pagination-page-btn');}";
        
        private static readonly string EmployeesLinkClassName = ".org-top-card-secondary-content__see-all-link";
        private static readonly string EmployeesLinkAllClassName = ".org-top-card-secondary-content__see-all-independent-link a";
        private static readonly string LinkedinNavbarClassName = ".global-nav__me";
        private static readonly string ResultEntityClassName = ".entity-result__item";

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

            _viewPortOptions = new ViewPortOptions
            {
                Width = 1200,
                Height = 900
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static async Task ScrollToEndOfPageAsync(Page page)
        {
            await page.EvaluateFunctionAsync(@"() => {
                window.scrollTo({
                    top: document.body.scrollHeight,
                    left: 0,
                    behavior: 'smooth'
                });
            }");
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

        /// <summary>
        /// Downloads chromium driver if not already present and launches puppeteer
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            Logger.Log.Print("initializing...", Logger.LogType.INFO);
            await _browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                DefaultViewport = _viewPortOptions
            });

            _browserPage = await _browser.NewPageAsync();
            await _browserPage.SetUserAgentAsync(UserAgent);

            Logger.Log.Print("initialized with success", Logger.LogType.INFO);
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
                Logger.Log.Print("found company employees page", Logger.LogType.INFO);
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
            bool foundCompanyEmployeesPage = await FindCompanyEmployeesPageAsync(EmployeesLinkAllClassName) || await FindCompanyEmployeesPageAsync(EmployeesLinkClassName);

            if (!foundCompanyEmployeesPage)
            {
                Logger.Log.Print("failed to find company employees page", Logger.LogType.INFO);
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
                Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
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
                Logger.Log.Print("attempting to login...", Logger.LogType.INFO);

                if (!await VisitAndWaitAsync("https://www.linkedin.com/", "#session_password"))
                {
                    Logger.Log.Print("failed to load linkedin", Logger.LogType.ERROR);
                    return false;
                }

                await _browserPage.EvaluateExpressionAsync($"document.querySelector('#session_key').value ='{email}'");
                await _browserPage.EvaluateExpressionAsync($"document.querySelector('#session_password').value ='{password}'");
                await _browserPage.EvaluateExpressionAsync("document.querySelector('button.sign-in-form__submit-btn--full-width').click()");

                await _browserPage.WaitForSelectorAsync(LinkedinNavbarClassName);
            }
            catch (Exception ex)
            {
                Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
                return false;
            }

            Logger.Log.Print("login successful", Logger.LogType.INFO);
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
            await WaitFor(".artdeco-pagination__indicator", 15000);

            try
            {
                string page = await _browserPage.EvaluateFunctionAsync<string>(JsGetLastPage);
                _lastPage = Convert.ToInt32(page);

                Logger.Log.Print($"found number of pages ({_lastPage})", Logger.LogType.INFO);
            }
            catch (Exception ex)
            {
                Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
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
                Logger.Log.Print($"extracting page {i}/{_lastPage}", Logger.LogType.INFO);

                if (!await VisitAndWaitAsync($"{_searchPageLink}&page={i}", ResultEntityClassName))
                {
                    Logger.Log.Print($"failed to get page {i}/{_lastPage}", Logger.LogType.WARN);
                    continue;
                }

                List<string> temp = await _browserPage.EvaluateExpressionAsync<List<string>>(JsGetEmployeeNames);
                AddEmployees(temp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
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
                Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves Employees list to file
        /// </summary>
        public void SaveFile<T>(List<T> obj)
        {
            string filename = $"emails-{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.json";

            try
            {
                File.WriteAllText(filename, JsonConvert.SerializeObject(obj, Formatting.Indented));
                Logger.Log.Print($"saved output to {filename}", Logger.LogType.INFO);
            }
            catch (Exception ex)
            {
                Logger.Log.Print(ex.Message, Logger.LogType.ERROR);
            }
        }
    }
}
