using Ilmas6ber.Services.Auth;

namespace Ilmas6ber
{
    public partial class AppShell : Shell
    {
        public AppShell(AuthService authService)
        {
            InitializeComponent();

            // If the user is already logged in, navigate directly to MainPage
            if (authService.IsLoggedIn)
            {
                // Switch to MainPage tab immediately
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await GoToAsync("//MainPage");
                });
            }
        }
    }
}
