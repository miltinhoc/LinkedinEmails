namespace LinkedinEmails.Constants
{
    public class LinkedinClasses
    {
        public static readonly string EmployeesLinkClassName = ".org-top-card-secondary-content__see-all-link";
        public static readonly string EmployeesLinkAllClassName = ".org-top-card-secondary-content__see-all-independent-link a";
        public static readonly string EmployeesLinkInsightClassName = ".org-top-card-secondary-content__insights-improvement a";
        public static readonly string EmployeesGenericClassName = ".mt1 a[href]";
        public static readonly string LinkedinNavbarClassName = ".global-nav__me";
        public static readonly string ResultEntityClassName = ".entity-result__divider";
        public static readonly string SelectorLoginButton = "button.btn__primary--large";
        public static readonly string PageSelectorClassName = ".artdeco-pagination__indicator";
        public static readonly string IdEmailInput = "#username";
        public static readonly string IdPasswordInput = "#password";
        public static readonly string PinInputClassName = ".input_verification_pin";
        public static readonly string IdPinSubmitButton = "#two-step-submit-button";

        public static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36";
        public static readonly string JsGetEmployeeNames = "Array.from(document.querySelectorAll('.entity-result__title-text a span[dir] span[aria-hidden]')).map(a => a.innerText);";
        public static readonly string JsGetLastPage = "() => {var elem = document.querySelectorAll('.artdeco-pagination__indicator.artdeco-pagination__indicator--number');return elem[elem.length-1].getAttribute('data-test-pagination-page-btn');}";
        public static readonly string JsScrollDown = "() => {window.scrollTo({top: document.body.scrollHeight,left: 0,behavior: 'smooth'});}";
    }
}
