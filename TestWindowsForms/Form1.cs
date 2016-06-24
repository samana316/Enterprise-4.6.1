using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Core.Linq.Reactive;
using TestWindowsForms.Controllers;

namespace TestWindowsForms
{
    public partial class Form1 : Form
    {
        private IAsyncObserver<long> observer;

        private IAsyncObservable<long> source;

        private long count;

        private IDisposable subscription;

        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
        }

        protected virtual IAsyncObservable<long> CreateTimer()
        {
            var interval = TimeSpan.FromSeconds(1);

            return AsyncObservable.Timer(TimeSpan.Zero, interval);
        }

        private async void StartButton_Click(
            object sender, 
            EventArgs e)
        {
            this.StartButton.Enabled = false;
            this.StopButton.Enabled = true;
            this.ResetButton.Enabled = false;

            if (this.source == null)
            {
                this.source = this.CreateTimer();
            }

            if (this.observer == null)
            {
                this.observer = AsyncObserver.Create<long>(this.OnNextAsync);
            }

            if (this.cancellationTokenSource == null)
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                this.subscription = await source.SubscribeAsync(
                    this.observer, this.cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            
            }
        }

        private void StopButton_Click(
            object sender, 
            EventArgs e)
        {
            this.StartButton.Enabled = true;
            this.StopButton.Enabled = false;
            this.ResetButton.Enabled = true;

            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            if (this.observer != null)
            {
                this.observer.OnCompleted();
                this.observer = null;
            }

            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            if (this.source != null)
            {
                var disposable = this.source as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                this.source = null;
            }
        }

        private void ResetButton_Click(
            object sender,
            EventArgs e)
        {
            this.count = -1;
            this.OnNext();
        }

        private Task OnNextAsync(
            long value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(new Action(this.OnNext));
        }

        private void OnNext()
        {
            if (this.InvokeRequired)
            {
                Action onNext = this.OnNextImpl;
                this.Invoke(onNext);
            }
            else
            {
                this.OnNextImpl();
            }
        }

        private void OnNextImpl()
        {
            try
            {
                this.count++;
                this.CounterLabel.Text = count.ToString();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                this.StopButton_Click(this.StopButton, EventArgs.Empty);
            }
        }
    }
}
