using Ilmas6ber.Domain;
using Ilmas6ber.Services.Auth;

namespace Ilmas6ber;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _authService;
    private ApplicationUser? _currentUser;

    public ProfilePage(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserData();
    }

    private async Task LoadUserData()
    {
        try
        {
            _currentUser = await _authService.GetCurrentUserAsync();

            if (_currentUser != null)
            {
                DisplayNameLabel.Text = _currentUser.DisplayName;
                EmailLabel.Text = _currentUser.Email;
                XpPointsLabel.Text = $"{_currentUser.xpPoints} XP";
                
                // Calculate level using the same logic as MainPage
                int level = CalculateLevel(_currentUser.xpPoints);
                XpLevelLabel.Text = $"Level {level}";

                TeamColorLabel.Text = _currentUser.TeamColor ? "Red Team" : "Blue Team";
                TeamColorLabel.TextColor = _currentUser.TeamColor ? Colors.Red : Colors.Blue;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load profile: {ex.Message}", "OK");
        }
    }

    private int CalculateLevel(double xp)
    {
        if (xp < 200) return 1;
        if (xp < 400) return 2;
        if (xp < 600) return 3;
        if (xp < 800) return 4;
        if (xp < 1000) return 5;
        return 6;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Navigate back to the previous page in the navigation stack (MainPage)
        await Shell.Current.GoToAsync("..");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to log out?", "Yes", "No");
        if (confirm)
        {
            _authService.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}