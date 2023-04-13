using System.Runtime.CompilerServices;

namespace LinkedinEmails.Logger
{
    /// <summary>
    /// 
    /// </summary>
    public class Log
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="callerName"></param>
        /// <param name="lineNumber"></param>
        public static void Print(string message, LogType type, [CallerMemberName] string callerName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Console.Write($" {DateTime.Now}");

            DrawWithColor($" [{type}]", TypeColor(type));
            DrawWithColor($" [{callerName}:{lineNumber}]", ConsoleColor.Magenta);

            Console.Write($" {message}\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        private static void DrawWithColor(string text, ConsoleColor color)
        {
            ConsoleColor current = Console.ForegroundColor;
            Console.ForegroundColor = color;

            Console.Write(text);
            Console.ForegroundColor = current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ConsoleColor TypeColor(LogType type)
        {
            return type switch
            {
                LogType.INFO => ConsoleColor.Cyan,
                LogType.WARN => ConsoleColor.Yellow,
                LogType.ERROR => ConsoleColor.Red,
                LogType.FATAL => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };
        }
    }

    public enum LogType
    {
        INFO,
        WARN,
        ERROR,
        FATAL
    }
}
