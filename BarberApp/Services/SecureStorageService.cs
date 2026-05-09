using System.Diagnostics;

namespace BarberApp.Services;

public class SecureStorageService
{
    // === КЛЮЧИ ДЛЯ КЛИЕНТОВ ===
    private const string KeyClientId = "client_id";
    private const string KeyClientName = "client_name";
    private const string KeyClientPhone = "client_phone";
    private const string KeyClientLogin = "client_login";
    private const string KeyClientPassword = "client_password";

    // === КЛЮЧИ ДЛЯ АДМИНОВ ===
    private const string KeyAdminId = "admin_id";
    private const string KeyAdminSalonId = "admin_salon_id";
    private const string KeyAdminLogin = "admin_login";

    // ==================== КЛИЕНТСКИЕ МЕТОДЫ ====================

    public async Task SaveCredentialsAsync(int clientId, string name, string login, string phone, string password)
    {
        await SecureStorage.Default.SetAsync(KeyClientId, clientId.ToString());
        await SecureStorage.Default.SetAsync(KeyClientName, name);
        await SecureStorage.Default.SetAsync(KeyClientLogin, login);
        await SecureStorage.Default.SetAsync(KeyClientPhone, phone);
        await SecureStorage.Default.SetAsync(KeyClientPassword, password);

        Debug.WriteLine($">> 🔐 Клиент сохранён: clientId={clientId}, login={login}");
    }

    public async Task<int?> GetClientIdAsync()
    {
        var val = await SecureStorage.Default.GetAsync(KeyClientId);
        return int.TryParse(val, out var id) ? id : null;
    }

    public async Task<string?> GetClientNameAsync() => await SecureStorage.Default.GetAsync(KeyClientName);
    public async Task<string?> GetClientPhoneAsync() => await SecureStorage.Default.GetAsync(KeyClientPhone);
    public async Task<string?> GetClientLoginAsync() => await SecureStorage.Default.GetAsync(KeyClientLogin);

    public async Task UpdateCredentialsAsync(string? name = null, string? password = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await SecureStorage.Default.SetAsync(KeyClientName, name);
            Debug.WriteLine($">> 🔐 Обновлено имя: {name}");
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            await SecureStorage.Default.SetAsync(KeyClientPassword, password);
            Debug.WriteLine(">> 🔐 Обновлён пароль");
        }
    }

    public async Task ClearCredentialsAsync()
    {
        SecureStorage.Default.Remove(KeyClientId);
        SecureStorage.Default.Remove(KeyClientName);
        SecureStorage.Default.Remove(KeyClientPhone);
        SecureStorage.Default.Remove(KeyClientLogin);
        SecureStorage.Default.Remove(KeyClientPassword);

        Debug.WriteLine(">> 🔓 Клиентские данные очищены");
    }

    // ==================== АДМИНСКИЕ МЕТОДЫ ====================

    public async Task SaveAdminCredentialsAsync(int adminId, int salonId, string login)
    {
        await SecureStorage.Default.SetAsync(KeyAdminId, adminId.ToString());
        await SecureStorage.Default.SetAsync(KeyAdminSalonId, salonId.ToString());
        await SecureStorage.Default.SetAsync(KeyAdminLogin, login);

        Debug.WriteLine($">> 🔐 Админ сохранён: adminId={adminId}, salonId={salonId}, login={login}");
    }

    public async Task<int?> GetAdminIdAsync()
    {
        var val = await SecureStorage.Default.GetAsync(KeyAdminId);
        return int.TryParse(val, out var id) ? id : null;
    }

    public async Task<int?> GetAdminSalonIdAsync()
    {
        var val = await SecureStorage.Default.GetAsync(KeyAdminSalonId);
        return int.TryParse(val, out var id) ? id : null;
    }

    public async Task<string?> GetAdminLoginAsync() => await SecureStorage.Default.GetAsync(KeyAdminLogin);

    public async Task ClearAdminCredentialsAsync()
    {
        SecureStorage.Default.Remove(KeyAdminId);
        SecureStorage.Default.Remove(KeyAdminSalonId);
        SecureStorage.Default.Remove(KeyAdminLogin);

        Debug.WriteLine(">> 🔓 Админские данные очищены");
    }
}