using BarberApp.Views; // ✅ Для LoginPage

namespace BarberApp.Services;

public class SecureStorageService
{
    private const string ClientIdKey = "client_id";
    private const string ClientNameKey = "client_name";
    private const string ClientLoginKey = "client_login";
    private const string ClientPhoneKey = "client_phone";
    private const string ClientPasswordKey = "client_password";

    // ✅ Добавлен параметр 'phone'
    public async Task SaveCredentialsAsync(int clientId, string name, string login, string phone, string password)
    {
        await SecureStorage.Default.SetAsync(ClientIdKey, clientId.ToString());
        await SecureStorage.Default.SetAsync(ClientNameKey, name);
        await SecureStorage.Default.SetAsync(ClientLoginKey, login);
        await SecureStorage.Default.SetAsync(ClientPhoneKey, phone); // ✅ Теперь phone существует
        await SecureStorage.Default.SetAsync(ClientPasswordKey, password);
    }

    public async Task UpdateCredentialsAsync(string? name = null, string? login = null, string? password = null)
    {
        if (!string.IsNullOrEmpty(name))
            await SecureStorage.Default.SetAsync(ClientNameKey, name);
        if (!string.IsNullOrEmpty(login))
            await SecureStorage.Default.SetAsync(ClientLoginKey, login);
        if (!string.IsNullOrEmpty(password))
            await SecureStorage.Default.SetAsync(ClientPasswordKey, password);
    }

    public async Task<int?> GetClientIdAsync()
    {
        var idStr = await SecureStorage.Default.GetAsync(ClientIdKey);
        if (int.TryParse(idStr, out var id))
            return id;
        return null;
    }

    public async Task<string?> GetClientNameAsync() => await SecureStorage.Default.GetAsync(ClientNameKey);
    public async Task<string?> GetClientLoginAsync() => await SecureStorage.Default.GetAsync(ClientLoginKey);
    public async Task<string?> GetClientPhoneAsync() => await SecureStorage.Default.GetAsync(ClientPhoneKey);
    public async Task<string?> GetClientPasswordAsync() => await SecureStorage.Default.GetAsync(ClientPasswordKey);

    public async Task<bool> IsAuthenticatedAsync()
    {
        var clientId = await GetClientIdAsync();
        return clientId.HasValue;
    }

    public async Task ClearCredentialsAsync()
    {
        SecureStorage.Default.Remove(ClientIdKey);
        SecureStorage.Default.Remove(ClientNameKey);
        SecureStorage.Default.Remove(ClientLoginKey);
        SecureStorage.Default.Remove(ClientPhoneKey);
        SecureStorage.Default.Remove(ClientPasswordKey);
    }

    public async Task LogoutAsync()
    {
        await ClearCredentialsAsync();
        if (Application.Current?.MainPage != null)
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }

    // === АДМИН: СОХРАНЕНИЕ ===
    public async Task SaveAdminCredentialsAsync(int adminId, int salonId, string login)
    {
        await SecureStorage.Default.SetAsync("admin_id", adminId.ToString());
        await SecureStorage.Default.SetAsync("admin_salon_id", salonId.ToString());
        await SecureStorage.Default.SetAsync("admin_login", login);
    }

    // === АДМИН: ЧТЕНИЕ ===
    public async Task<int?> GetAdminIdAsync()
    {
        var val = await SecureStorage.Default.GetAsync("admin_id");
        return int.TryParse(val, out var id) ? id : null;
    }

    public async Task<int?> GetAdminSalonIdAsync()
    {
        var val = await SecureStorage.Default.GetAsync("admin_salon_id");
        return int.TryParse(val, out var id) ? id : null;
    }

    // === АДМИН: ВЫХОД ===
    public async Task ClearAdminCredentialsAsync()
    {
        SecureStorage.Default.Remove("admin_id");
        SecureStorage.Default.Remove("admin_salon_id");
        SecureStorage.Default.Remove("admin_login");
    }
}