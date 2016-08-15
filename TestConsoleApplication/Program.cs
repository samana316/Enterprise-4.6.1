using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace TestConsoleApplication
{
    internal static class Program
    {
        [STAThread]
        public static void Main(
            string[] args)
        {
            var controller = new Controller();


        }

        private sealed class Controller
        {
            public async Task RunAsync(
                CancellationToken cancellationToken)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Out.WriteLineAsync("Blah...");
                }
                catch (Exception exception)
                {
                    await Error.WriteLineAsync(exception.ToString());
                }
            }
        }
    }
}
