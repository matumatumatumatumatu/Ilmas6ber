using Ilmas6ber.Services.Auth;
using MySqlConnector;

namespace Ilmas6ber;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private bool _isSignUpMode = false;

    public LoginPage(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
    }

    /// <summary>
    /// Switches to Login mode — hides signup-only fields.
    /// </summary>
    private void OnLoginTabClicked(object sender, EventArgs e)
    {
        _isSignUpMode = false;
        UpdateTabVisuals();
    }

    /// <summary>
    /// Switches to Sign Up mode — shows all fields including name and confirm password.
    /// </summary>
    private void OnSignUpTabClicked(object sender, EventArgs e)
    {
        _isSignUpMode = true;
        UpdateTabVisuals();
    }

    /// <summary>
    /// Footer "Sign Up" / "Login" text tapped — toggles mode.
    /// </summary>
    private void OnFooterActionTapped(object sender, TappedEventArgs e)
    {
        _isSignUpMode = !_isSignUpMode;
        UpdateTabVisuals();
    }

    /// <summary>
    /// Updates which fields are visible and tab button colors based on current mode.
    /// </summary>
    private void UpdateTabVisuals()
    {
        // Toggle signup-only fields
        FullNameEntry.IsVisible = _isSignUpMode;
        ConfirmPasswordEntry.IsVisible = _isSignUpMode;

        // Update tab button colors
        LoginTabButton.BackgroundColor = _isSignUpMode
            ? Color.FromArgb("#E5E7EB")
            : Color.FromArgb("#4F46E5");
        LoginTabButton.TextColor = _isSignUpMode
            ? Color.FromArgb("#374151")
            : Colors.White;

        SignUpTabButton.BackgroundColor = _isSignUpMode
            ? Color.FromArgb("#4F46E5")
            : Color.FromArgb("#E5E7EB");
        SignUpTabButton.TextColor = _isSignUpMode
            ? Colors.White
            : Color.FromArgb("#374151");

        // Update button text
        ContinueButton.Text = _isSignUpMode ? "Create Account" : "Login";

        // Update footer text
        FooterLabel.Text = _isSignUpMode
            ? "Already have an account?"
            : "Don't have an account?";
        FooterActionLabel.Text = _isSignUpMode ? "Login" : "Sign Up";

        // Clear any previous errors
        HideError();
    }

    /// <summary>
    /// Main action button — either logs in or registers depending on current mode.
    /// </summary>
    private async void OnContinueClicked(object sender, EventArgs e)
    {
        HideError();

        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";

        // Basic validation
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowError("Please enter your email address.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter your password.");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters.");
            return;
        }

        if (_isSignUpMode)
        {
            await HandleSignUp(email, password);
        }
        else
        {
            await HandleLogin(email, password);
        }
    }

    private async Task HandleLogin(string email, string password)
    {
        SetLoading(true);

        try
        {
            var user = await _authService.LoginAsync(email, password);

            if (user == null)
            {
                ShowError("Invalid email or password.");
                return;
            }

            // Save the session — respects "Remember Me" checkbox
            _authService.SaveSession(user.Id, RememberMeCheckBox.IsChecked);

            // Navigate to MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (MySqlException)
        {
            ShowError("Could not connect to server. Check your internet connection.");
        }
        catch (Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async Task HandleSignUp(string email, string password)
    {
        string fullName = FullNameEntry.Text?.Trim() ?? "";
        string confirmPassword = ConfirmPasswordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowError("Please enter your full name.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords do not match.");
            return;
        }

        SetLoading(true);

        try
        {
            var user = await _authService.RegisterAsync(email, password, fullName);

            // Save session after registration
            _authService.SaveSession(user.Id, RememberMeCheckBox.IsChecked);

            // Navigate to MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // MySQL error 1062 = duplicate entry (email already exists)
            ShowError("An account with this email already exists.");
        }
        catch (MySqlException)
        {
            ShowError("Could not connect to server. Check your internet connection.");
        }
        catch (Exception ex)
        {
            ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void HideError()
    {
        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
        ContinueButton.IsEnabled = !isLoading;
    }
}