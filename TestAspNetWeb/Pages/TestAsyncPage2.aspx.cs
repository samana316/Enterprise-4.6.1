using System;
using System.Threading.Tasks;
using System.Web.UI;

namespace TestAspNetWeb.Pages
{
    public partial class TestAsyncPage2 : Page
    {
        protected void Page_Load(
            object sender, 
            EventArgs e)
        {
            this.Button1_Click(this, EventArgs.Empty);
        }

        protected async void Button1_Click(
            object sender, 
            EventArgs e)
        {
            await Task.Yield();
            await Task.Delay(500);

            throw new InvalidOperationException("Manual throw from Button1_Click()");
        }
    }
}