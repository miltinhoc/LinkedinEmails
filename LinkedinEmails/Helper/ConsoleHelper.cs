namespace LinkedinEmails.Helper
{
    public class ConsoleHelper
    {
        /// <summary>
        /// Clears the last line written to the Console
        /// </summary>
        public static void ClearLastConsoleLine()
        {
            int currentLineCursor = Console.CursorTop - 1;
            if (currentLineCursor >= 0)
            {
                Console.SetCursorPosition(0, currentLineCursor);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
            }
        }
    }
}
