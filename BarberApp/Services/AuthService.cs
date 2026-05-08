using Dapper;
using Npgsql;

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

        // Для демо: сравниваем пароли как строки
        if (user.password_hash == password)
        {
            System.Diagnostics.Debug.WriteLine($">> ✅ Пароль верен");
            return user.id;
        }

        System.Diagnostics.Debug.WriteLine($">> ❌ Неверный пароль");
        return null;
    }

    public async Task<int?> RegisterAsync(string name, string phone, string login, string password)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(@"
            SELECT EXISTS(SELECT 1 FROM client WHERE login = @Login OR phone = @Phone)",
            new { Login = login, Phone = phone });

        if (exists)
            return null;

        var id = await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO client (name, phone, login, password_hash)
            VALUES (@Name, @Phone, @Login, @Password)
            RETURNING id",
            new { Name = name, Phone = phone, Login = login, Password = password });

        return id;
    }
}