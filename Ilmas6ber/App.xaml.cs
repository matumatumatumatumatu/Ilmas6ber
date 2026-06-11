using Ilmas6ber.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Ilmas6ber
{
    public partial class App : Application
    {
        private readonly AuthService _authService;

        public App(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell(_authService);
            var window = new Window(shell);

            // When the app is closing, clear session if "Remember Me" was not checked
            window.Destroying += (s, e) =>
            {
                _authService.ClearSessionIfNotRemembered();
            };

            return window;
        }
    }
}