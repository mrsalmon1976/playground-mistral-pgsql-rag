using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistral.Utils
{
    internal class ConsoleUtils
    {
        public static void WriteLine(string value, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            if (foreground.HasValue)
            {
                Console.ForegroundColor = foreground.Value;
            }
            if (background.HasValue)
            {
                Console.BackgroundColor = background.Value;
            }

            Console.WriteLine(value);
            Console.ResetColor();

        }
    }
}
