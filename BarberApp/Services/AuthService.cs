using Dapper;
using Npgsql;
using BCrypt.Net;
namespace BarberApp.Services;

public class AuthService
{
    private readonly string _connectionString;

    public AuthService()
    {
        _connectionString = "Host=10.0.2.2;Port=5432;Database=hairsalon;Username=postgres;Password=bboystr2469";
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<int?> LoginAsync(string login, string password)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var user = await connection.QueryFirstOrDefaultAsync(@"
        SELECT id, password_hash FROM client WHERE login = @Login",
            new { Login = login });

        if (user == null)
        {
            System.Diagnostics.Debug.WriteLine($">> ❌ Пользователь '{login}' не найден");
            return null;
        }

        // ✅ ПРОВЕРКА ПАРОЛЯ ЧЕРЕЗ BCRYPT
        try
        {
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.password_hash);

            if (isPasswordValid)
            {
                System.Diagnostics.Debug.WriteLine($">> ✅ Пароль верен (хэш совпадает)");
                return user.id;
            }
        }
        catch (Exception ex)
        {
            // Если хэш в базе не в формате BCrypt (старые записи)
            System.Diagnostics.Debug.WriteLine($">> ⚠️ Ошибка проверки хэша: {ex.Message}");

            // 🔧 Временный фолбэк для старых паролей (удали после миграции)
            if (user.password_hash == password)
            {
                System.Diagnostics.Debug.WriteLine($">> ✅ Пароль верен (старый формат)");
                return user.id;
            }
        }

        System.Diagnostics.Debug.WriteLine($">> ❌ Неверный пароль");
        return null;
    }



    public async Task<int?> RegisterAsync(string name, string phone, string login, string password)
    {
    using var connection = CreateConnection();
    await connection.OpenAsync();

    // Проверка существования
    var exists = await connection.ExecuteScalarAsync<bool>(
        "SELECT EXISTS(SELECT 1 FROM client WHERE login = @Login)",
        new { Login = login });

    if (exists) return null;

    // ✅ ХЭШИРУЕМ ПАРОЛЬ
    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

    var sql = @"
        INSERT INTO client (name, phone, login, password_hash, created_at)
        VALUES (@Name, @Phone, @Login, @Password, NOW())
        RETURNING id";

    return await connection.ExecuteScalarAsync<int>(sql, new
    {
        Name = name,
        Phone = phone,
        Login = login,
        Password = hashedPassword // ✅ Сохраняем хэш
    });
    }

}