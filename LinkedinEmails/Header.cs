using System.Reflection;

namespace LinkedinEmails
{
    public class Header
    {
        private static readonly Dictionary<string, ConsoleColor> logo = new();
        private static bool isInit;
        private static readonly ConsoleColor color = ConsoleColor.Red;

        public static void Init()
        {
            isInit = true;
            logo.Add(@"", Console.ForegroundColor);
            logo.Add(@"               .:-=++++==-:", color);
            logo.Add(@"            -*%@@@@@@@@@@@%@#+:", color);
            logo.Add(@"         :*%@@@%%%**%%%%@@%%@@@@+.", color);
            logo.Add(@"       .*@@@@%%%%#  .*%%@@%@@@@%@@+", color);
            logo.Add(@"      =@@@@%%%%%@#  . :#@@%@@@%%@@@#:", color);
            logo.Add(@"     =@@@@@@@@%###==++--#@@%@@@@@%%%%.", color);
            logo.Add(@"    :@@@%@@@@%:        ..:--+*%@%@%@%#", color);
            logo.Add(@"    *@@@@%@@@%#: .--:.  .++=: .+%%%%@@-", color);
            logo.Add(@"    %@@@%@@@@@@%#+-:::  +@=:%+  %%%@%@*", color);
            logo.Add(@"    %@@@%@@@%%%#=.  ..  +@#=:  =%%%@%@*", color);
            logo.Add(@"    *@@@@%@@@%+  =*%%%%%%#  -*%%%%@%%@=", color);
            logo.Add(@"    -@@@@%%++#  *%@%*++#%= :%%%@%@%@%%", color);
            logo.Add(@"     +@@%@%--*  #%%%-:. -= .%@@@@@%%%:", color);
            logo.Add(@"      +@@@@%@%= .+#%%%+ .#. -*%@@@%%:", color);
            logo.Add(@"       :#@@@%%%#-.  .  :#%#=::*%@@*.", color);
            logo.Add(@"         -#@@@linkedinEmails%@@@*.", color);
            logo.Add(@"           .=#@@@@@@@%@@@@%@%*=.", color);
            logo.Add(@"               .-=+*****+=-.  ", color);
            logo.Add($" [miltinh0c] (v{Assembly.GetExecutingAssembly().GetName().Version})\n", ConsoleColor.White);
        }

        public static void Draw()
        {
            if (!isInit)
            {
                Init();
            }
            Console.BackgroundColor = ConsoleColor.Black;
            ConsoleColor startColor = ConsoleColor.White;

            foreach (KeyValuePair<string, ConsoleColor> keyValue in logo)
            {
                if (Console.ForegroundColor != keyValue.Value)
                    Console.ForegroundColor = keyValue.Value;

                Console.WriteLine(keyValue.Key);
            }

            Console.ForegroundColor = startColor;
        }
    }
}
