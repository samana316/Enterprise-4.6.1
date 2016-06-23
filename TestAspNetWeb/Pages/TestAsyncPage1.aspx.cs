using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Core.Common.Threading;
using Enterprise.Core.Linq;

namespace TestAspNetWeb.Pages
{
    public partial class TestAsyncPage1 : Page
    {
        private readonly object thisLock = new object();

        protected void Page_Load(
            object sender,
            EventArgs e)
        {

        }

        protected async void Button1_Click(
            object sender,
            EventArgs e)
        {
            this.Label1.Text = string.Empty;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(5000);

                var list = await this.CreateSource().ToListAsync(cancellationTokenSource.Token);

                list.ForEach(x => this.Label1.Text += x);
            }
        }

        protected void Button2_Click(
            object sender, 
            EventArgs e)
        {
            this.RegisterAsyncTask(new PageAsyncTask(this.Button2_ClickAsync));
        }

        private async Task Button2_ClickAsync()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(5000);

                await this.DisplayTextAsync(this.Label2, 1, cancellationTokenSource.Token);
            }
        }

        private async Task DisplayTextAsync(
            Label label,
            double millisecondsDelay,
            CancellationToken cancellationToken)
        {
            label.Text = string.Empty;
            await Task.Delay(TimeSpan.FromMilliseconds(millisecondsDelay));

            await this.CreateSource().ForEachAsync(item =>
            {
                Task.Delay(TimeSpan.FromMilliseconds(millisecondsDelay)).Wait();
                label.Text += item;
            }, cancellationToken);
        }

        private IAsyncEnumerable<char> CreateSource()
        {
            //return AsyncEnumerable.Repeat('X', 26);

            const string Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return AsyncEnumerable.Create<char>(async (yielder, cancellationToken) =>
            {
                using (await AsyncLock.LockAsync(thisLock))
                {
                    foreach (var c in Alphabets)
                    {
                        await yielder.ReturnAsync(c, cancellationToken);
                    }

                    await yielder.BreakAsync(cancellationToken);
                }
            });
        }
    }
}