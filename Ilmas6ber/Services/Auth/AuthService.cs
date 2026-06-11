using Ilmas6ber.Domain;
using MySqlConnector;

namespace Ilmas6ber.Services.Auth
{
    /// <summary>
    /// Handles user authentication against the MySQL database.
    /// - Register: creates a new user with BCrypt-hashed password
    /// - Login: verifies credentials and returns the ApplicationUser
    /// - Session: stores/retrieves logged-in user ID via Preferences
    /// </summary>
    public class AuthService
    {
        private const string PrefKeyUserId = "logged_in_user_id";
        private const string PrefKeyRememberMe = "remember_me";

        /// <summary>
        /// Returns true if a user session exists (user previously logged in with "Remember Me").
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                bool rememberMe = Preferences.Default.Get(PrefKeyRememberMe, false);
                int userId = Preferences.Default.Get(PrefKeyUserId, 0);
                return rememberMe && userId > 0;
            }
        }

        /// <summary>
        /// Registers a new user account.
        /// Hashes the password with BCrypt before storing it in the database.
        /// Returns the created ApplicationUser, or throws if email already exists.
        /// </summary>
        public async Task<ApplicationUser> RegisterAsync(string email, string password, string displayName)
        {
            // Hash the password — BCrypt automatically generates a salt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            await using var conn = new MySqlConnection(Environment.ConnectionString);
            await conn.OpenAsync();

            // Insert the new user into the database
            await using var cmd = new MySqlCommand(
                @"INSERT INTO users (email, password_hash, display_name, profile_picture_id, team_color, xp_points)
                  VALUES (@email, @passwordHash, @displayName, 0, 0, 0)", conn);

            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
            cmd.Parameters.AddWithValue("@displayName", displayName);

            await cmd.ExecuteNonQueryAsync();

            // Get the auto-generated ID of the newly inserted user
            int newId = (int)cmd.LastInsertedId;

            var user = new ApplicationUser
            {
                Id = newId,
                Email = email,
                PasswordHash = passwordHash,
                DisplayName = displayName,
                ProfilePictureID = 0,
                TeamColor = false,
                xpPoints = 0,
                xpLevel = 0
            };

            return user;
        }

        /// <summary>
        /// Attempts to log in with the given email and password.
        /// Returns the ApplicationUser if credentials are valid, null otherwise.
        /// </summary>
        public async Task<ApplicationUser?> LoginAsync(string email, string password)
        {
            await using var conn = new MySqlConnection(Environment.ConnectionString);
            await conn.OpenAsync();

            // Query the user by email
            await using var cmd = new MySqlCommand(
                "SELECT id, email, password_hash, display_name, profile_picture_id, team_color, xp_points FROM users WHERE email = @email",
                conn);
            cmd.Parameters.AddWithValue("@email", email);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                // No user found with this email
                return null;
            }

            // Read the stored password hash from the database
            string storedHash = reader.GetString("password_hash");

            // Verify the entered password against the stored hash
            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
            {
                // Password doesn't match
                return null;
            }

            // Password matches — build and return the ApplicationUser
            var user = new ApplicationUser
            {
                Id = reader.GetInt32("id"),
                Email = reader.GetString("email"),
                PasswordHash = storedHash,
                DisplayName = reader.GetString("display_name"),
                ProfilePictureID = reader.GetInt32("profile_picture_id"),
                TeamColor = reader.GetBoolean("team_color"),
                xpPoints = reader.GetDouble("xp_points"),
            };

            return user;
        }

        /// <summary>
        /// Saves the login session to device preferences.
        /// If rememberMe is false, the session will be cleared when the app closes.
        /// </summary>
        public void SaveSession(int userId, bool rememberMe)
        {
            Preferences.Default.Set(PrefKeyUserId, userId);
            Preferences.Default.Set(PrefKeyRememberMe, rememberMe);
        }

        /// <summary>
        /// Loads the currently logged-in user from the database using the stored session ID.
        /// Returns null if no session exists or the user is not found.
        /// </summary>
        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            int userId = Preferences.Default.Get(PrefKeyUserId, 0);
            if (userId <= 0) return null;

            await using var conn = new MySqlConnection(Environment.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(
                "SELECT id, email, password_hash, display_name, profile_picture_id, team_color, xp_points FROM users WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("@id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ApplicationUser
            {
                Id = reader.GetInt32("id"),
                Email = reader.GetString("email"),
                PasswordHash = reader.GetString("password_hash"),
                DisplayName = reader.GetString("display_name"),
                ProfilePictureID = reader.GetInt32("profile_picture_id"),
                TeamColor = reader.GetBoolean("team_color"),
                xpPoints = reader.GetDouble("xp_points"),
            };
        }

        /// <summary>
        /// Updates the user's XP points in the database.
        /// Call this whenever XP changes so it persists.
        /// </summary>
        public async Task UpdateXpAsync(int userId, double xpPoints)
        {
            await using var conn = new MySqlConnection(Environment.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand(
                "UPDATE users SET xp_points = @xp WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@xp", xpPoints);
            cmd.Parameters.AddWithValue("@id", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Clears the login session. The user will see the LoginPage on next app start.
        /// </summary>
        public void Logout()
        {
            Preferences.Default.Remove(PrefKeyUserId);
            Preferences.Default.Remove(PrefKeyRememberMe);
        }

        /// <summary>
        /// Clears session if "Remember Me" was not checked.
        /// Call this when the app is closing.
        /// </summary>
        public void ClearSessionIfNotRemembered()
        {
            bool rememberMe = Preferences.Default.Get(PrefKeyRememberMe, false);
            if (!rememberMe)
            {
                Logout();
            }
        }
    }
}
