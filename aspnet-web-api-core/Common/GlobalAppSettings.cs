namespace AspNetWebApiCore.Common
{
    public static class GlobalAppSettings
    {
        // API Token Authentication Stuff
        public static string? ApiSecretKey{ get; set; }
        public static string? Issuer { get; set; }     
        public static string? EncryptKey { get; set; }
      
    }

    // ------------------------------------------------------------
    // Role Based Settings configuration
    // ------------------------------------------------------------

    // record JwtSettings(string Issuer); 
    // Note: We can use record instead of class.
    // A record is like a class, but primarily meant for storing data.
    //In C#, a record is a special type introduced in C# 9 that is designed to make working with immutable data models easier and more concise.

    public class UserAccount
    {
        public string RoleName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool Enabled { get; set; } = true;     
        public string TokenLifetimeMinutes { get; set; }
        public string Audience { get; init; } = string.Empty;
        public string UserCategory { get; set; } = ""; // Patient or HealthCareUnit
    }
     
    public interface IRoles
    {
        UserAccount? FindByUsername(string username);
        IEnumerable<UserAccount> All();
    }

    class JsonRoles : IRoles
    {
        private readonly Dictionary<string, UserAccount> _users;
        public JsonRoles(IEnumerable<UserAccount> users)
        {
            _users = users.ToDictionary(u => u.Username, u => u, StringComparer.OrdinalIgnoreCase);
        }
        public UserAccount? FindByUsername(string username) =>
            _users.TryGetValue(username, out var user) ? user : null;
        public IEnumerable<UserAccount> All() => _users.Values;
    }
}
