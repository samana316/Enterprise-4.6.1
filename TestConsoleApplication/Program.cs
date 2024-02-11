using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Linq;
using static System.Console;
using static Enterprise.Core.IO.AsyncFile;
using static System.Reactive.Linq.Observable;

namespace TestConsoleApplication
{
    internal static class Program
    {
        [STAThread]
        public static void Main(
            string[] args)
        {
            var controller = new Controller();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(30000);

                var cancellationToken = cancellationTokenSource.Token;

                var task1 = controller.RunAsync(cancellationToken);
                var task2 = controller.RunAsync(cancellationToken);

                var all = Task.WhenAll(task1, task2);
            }

            Task.Delay(8000).Wait();
            WriteLine("Done");
            ReadLine();
        }

        private sealed class Controller
        {
            public async Task RunAsync(
                CancellationToken cancellationToken)
            {
                try
                {
                    await this.DoRunAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    await Error.WriteLineAsync(exception.ToString());
                }
            }

            private async Task DoRunAsync(
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //const string path = @"C:\Temp\Temp2.txt";

                var source = "ABCDEFG".AsEnumerable().AsAsyncEnumerable();

                using (var enumerator = source.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(cancellationToken))
                    {
                        await Task.Delay(1000, cancellationToken);
                        await Out.WriteLineAsync(enumerator.Current);
                    }
                }
            }
        }
    }
}
