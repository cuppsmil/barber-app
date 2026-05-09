using Dapper;
using Npgsql;
using BarberApp.Models;
using System.Data;
using BCrypt.Net;

namespace BarberApp.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = "Host=10.0.2.2;Port=5432;Database=hairsalon;Username=postgres;Password=bboystr2469";
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    // === САЛОНЫ ===
    public async Task<List<Salon>> GetSalonsAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, name, phone, address FROM salon ORDER BY id";
        var result = await connection.QueryAsync<Salon>(sql);
        return result.ToList();
    }

    // === МАСТЕРА ===
    public async Task<List<Master>> GetMastersAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, fio, phone, passport, grade FROM master ORDER BY id";
        var result = await connection.QueryAsync<Master>(sql);
        return result.ToList();
    }

    // === УСЛУГИ ===
    public async Task<List<ServiceItem>> GetServicesAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, name, duration, description FROM service ORDER BY id";
        var result = await connection.QueryAsync<ServiceItem>(sql);
        return result.ToList();
    }

    // === СВЯЗИ САЛОН-МАСТЕР ===
    public async Task<List<SalonToMaster>> GetSalonToMastersAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, salon_id, master_id FROM salontomaster";
        var rows = await connection.QueryAsync(sql);

        var result = new List<SalonToMaster>();
        foreach (var row in rows)
        {
            var item = new SalonToMaster
            {
                Id = row.id,
                SalonId = row.salon_id,
                MasterId = row.master_id
            };
            result.Add(item);
        }

        return result;
    }

    // === СВЯЗИ МАСТЕР-УСЛУГА ===
    public async Task<List<MasterToService>> GetMasterToServicesAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, master_id, service_id FROM mastertoservice";
        var rows = await connection.QueryAsync(sql);

        var result = new List<MasterToService>();
        foreach (var row in rows)
        {
            var item = new MasterToService
            {
                Id = row.id,
                MasterId = row.master_id,
                ServiceId = row.service_id
            };
            result.Add(item);
        }

        return result;
    }

    // === ПОЛУЧИТЬ ВСЕ ДАННЫЕ ДЛЯ ГЛАВНОЙ СТРАНИЦЫ ===
    public async Task<List<BarberShop>> GetBarberShopsAsync()
    {
        var salons = await GetSalonsAsync();
        var masters = await GetMastersAsync();
        var services = await GetServicesAsync();
        var salonMastersLinks = await GetSalonToMastersAsync();
        var masterServicesLinks = await GetMasterToServicesAsync();

        var result = new List<BarberShop>();

        foreach (var salon in salons)
        {
            var masterIdsInSalon = salonMastersLinks
                .Where(link => link.SalonId == salon.Id)
                .Select(link => link.MasterId)
                .ToList();

            var mastersInSalon = masters
                .Where(m => masterIdsInSalon.Contains(m.Id))
                .ToList();

            var serviceIdsInSalon = masterServicesLinks
                .Where(link => masterIdsInSalon.Contains(link.MasterId))
                .Select(link => link.ServiceId)
                .Distinct()
                .ToList();

            var servicesInSalon = services
                .Where(s => serviceIdsInSalon.Contains(s.Id))
                .ToList();

            result.Add(new BarberShop
            {
                Salon = salon,
                Masters = mastersInSalon,
                AllServices = servicesInSalon
            });
        }

        return result;
    }

    // === ПОЛУЧИТЬ ЗАНЯТЫЕ СЛОТЫ ===
    public async Task<List<TimeSpan>> GetBusySlotsAsync(int masterId, DateTime date)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT date FROM appointments
            WHERE master_id = @MasterId
            AND date >= @StartOfDay AND date < @EndOfDay";

        var appointments = await connection.QueryAsync<DateTime>(sql, new
        {
            MasterId = masterId,
            StartOfDay = date.Date,
            EndOfDay = date.Date.AddDays(1)
        });

        return appointments.Select(a => a.TimeOfDay).ToList();
    }

    // === ПРОВЕРКА ДОСТУПНОСТИ СЛОТА ===
    public async Task<bool> IsTimeSlotAvailableAsync(int masterId, DateTime date, TimeSpan time)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var appointmentDateTime = date.Date.Add(time);

        var exists = await connection.ExecuteScalarAsync<bool>(@"
            SELECT EXISTS(
                SELECT 1 FROM appointments
                WHERE master_id = @MasterId
                AND date >= @StartTime
                AND date < @EndTime
            )",
            new
            {
                MasterId = masterId,
                StartTime = appointmentDateTime,
                EndTime = appointmentDateTime.AddMinutes(60)
            });

        return !exists;
    }

    // === СОЗДАТЬ ЗАПИСЬ ===
    public async Task<int> CreateAppointmentAsync(int masterId, int clientId, int serviceId, DateTime date, decimal price)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var isBusy = await connection.ExecuteScalarAsync<bool>(@"
        SELECT EXISTS(SELECT 1 FROM appointments WHERE master_id = @MasterId AND date = @Date)",
            new { MasterId = masterId, Date = date });

        if (isBusy) throw new Exception("Это время уже занято!");

        var sql = @"
        INSERT INTO appointments (master_id, client_id, service_id, date, price, created_at)
        VALUES (@MasterId, @ClientId, @ServiceId, @Date, @Price, NOW())
        RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            MasterId = masterId,
            ClientId = clientId,
            ServiceId = serviceId,
            Date = date,
            Price = price
        });
    }

    // === ПОЛУЧИТЬ ИЛИ СОЗДАТЬ КЛИЕНТА ===
    public async Task<int> GetTestClientIdAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var existingId = await connection.ExecuteScalarAsync<int?>(
            "SELECT id FROM client WHERE phone = '+7 (999) 123-45-67'");

        if (existingId.HasValue)
            return existingId.Value;

        return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO client (name, phone, disc_id) 
            VALUES ('Артур Иванов', '+7 (999) 123-45-67', NULL)
            RETURNING id");
    }

    // === ИСТОРИЯ ЗАПИСЕЙ (ИСПРАВЛЕНО) ===
    public async Task<List<AppointmentItem>> GetClientHistoryAsync(int clientId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
        SELECT a.id, a.date, a.price, c.name as client_name, m.fio as master_name, 
               s.name as salon_name, srv.name as service_name
        FROM appointments a
        JOIN master m ON a.master_id = m.id
        JOIN salontomaster stm ON m.id = stm.master_id
        JOIN salon s ON stm.salon_id = s.id
        JOIN client c ON a.client_id = c.id
        JOIN service srv ON a.service_id = srv.id
        WHERE a.client_id = @ClientId
        ORDER BY a.date DESC";

        var items = new List<AppointmentItem>();
        var rows = await connection.QueryAsync(sql, new { ClientId = clientId });
        foreach (var r in rows)
        {
            items.Add(new AppointmentItem
            {
                Id = r.id,
                Date = r.date,
                Price = r.price,
                MasterName = r.client_name ?? "Клиент",
                SalonName = r.master_name ?? "Мастер",
                ServiceName = r.service_name ?? "Услуга"
            });
        }
        return items;
    }

    // === ИЗБРАННОЕ ===
    public async Task<List<Master>> GetFavoritesAsync(int clientId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT m.id, m.fio, m.phone, m.passport, m.grade 
            FROM master m
            JOIN favorites f ON m.id = f.master_id
            WHERE f.client_id = @ClientId
            ORDER BY m.fio";

        try
        {
            var result = await connection.QueryAsync<Master>(sql, new { ClientId = clientId });
            var list = result.ToList();
            System.Diagnostics.Debug.WriteLine($">> Найдено избранных мастеров: {list.Count}");
            return list;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки избранного: {ex.Message}");
            return new List<Master>();
        }
    }

    public async Task ToggleFavoriteAsync(int clientId, int masterId, bool isAdding)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        if (isAdding)
        {
            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM favorites WHERE client_id = @CId AND master_id = @MId)",
                new { CId = clientId, MId = masterId });

            if (!exists)
            {
                System.Diagnostics.Debug.WriteLine($">> Добавление в избранное: Master {masterId}");
                await connection.ExecuteAsync(
                    "INSERT INTO favorites (client_id, master_id) VALUES (@CId, @MId)",
                    new { CId = clientId, MId = masterId });
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($">> Удаление из избранного: Master {masterId}");
            await connection.ExecuteAsync(
                "DELETE FROM favorites WHERE client_id = @CId AND master_id = @MId",
                new { CId = clientId, MId = masterId });
        }
    }

    // === ОБНОВЛЕНИЕ ДАННЫХ ПРОФИЛЯ (ИСПРАВЛЕНО) ===
    // Если newPassword == null или пустая строка, пароль НЕ меняется
    public async Task UpdateCredentialsAsync(int clientId, string login, string? newPassword = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            // Обновляем ТОЛЬКО логин
            System.Diagnostics.Debug.WriteLine($">> Обновление логина для клиента {clientId}: {login}");
            var sql = "UPDATE client SET login = @Login WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = clientId, Login = login });
        }
        else
        {
            // Обновляем логин И пароль
            System.Diagnostics.Debug.WriteLine($">> Обновление логина и пароля для клиента {clientId}");
            var sql = "UPDATE client SET login = @Login, password_hash = @Password WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = clientId, Login = login, Password = newPassword });
        }
    }

    // === ПОЛУЧИТЬ ИНФОРМАЦИЮ О КЛИЕНТЕ ===
    public async Task<(string Name, string Phone, string Login)> GetClientInfoAsync(int clientId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var row = await connection.QueryFirstOrDefaultAsync(@"
            SELECT name, phone, login FROM client WHERE id = @Id", new { Id = clientId });

        if (row == null)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Клиент {clientId} не найден в БД");
            return ("Клиент", "", "");
        }

        // Используем динамический доступ к полям (Dapper возвращает dynamic)
        return (
            row.name ?? "Клиент",
            row.phone ?? "Не указан",
            row.login ?? "Не указан"
        );
    }
    // === АДМИН: ВХОД ===
    public async Task<(int Id, int SalonId)?> GetAdminAsync(string login, string password)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var row = await connection.QueryFirstOrDefaultAsync(@"
        SELECT id, salon_id FROM admin WHERE login = @Login AND password_hash = @Password",
            new { Login = login, Password = password });

        return row == null ? null : (row.id, row.salon_id);
    }

    // === АДМИН: ЗАПИСИ САЛОНА ===
    public async Task<List<AppointmentItem>> GetSalonAppointmentsAsync(int salonId)
    {
        System.Diagnostics.Debug.WriteLine($">>> Загрузка записей для салона {salonId}");

        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
        SELECT 
            a.id, 
            a.date, 
            COALESCE(a.price, 0) as price,
            c.name as client_name, 
            m.fio as master_name, 
            srv.name as service_name
        FROM appointments a
        INNER JOIN master m ON a.master_id = m.id
        INNER JOIN salontomaster stm ON m.id = stm.master_id
        INNER JOIN client c ON a.client_id = c.id
        INNER JOIN service srv ON a.service_id = srv.id
        WHERE stm.salon_id = @SalonId
        ORDER BY a.date DESC";

        try
        {
            var items = new List<AppointmentItem>();
            var rows = await connection.QueryAsync(sql, new { SalonId = salonId });

            foreach (var row in rows)
            {
                var item = new AppointmentItem
                {
                    Id = Convert.ToInt32(row.id),
                    Date = Convert.ToDateTime(row.date),
                    Price = row.price != null ? Convert.ToDecimal(row.price) : 0,
                    MasterName = row.client_name?.ToString() ?? "Клиент",
                    SalonName = row.master_name?.ToString() ?? "Мастер",
                    ServiceName = row.service_name?.ToString() ?? "Услуга"
                };

                items.Add(item);
                System.Diagnostics.Debug.WriteLine($">>> Запись: {item.Date} | Цена: {item.Price} ₽");
            }

            System.Diagnostics.Debug.WriteLine($">>> Всего записей: {items.Count}");
            return items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($">>> ОШИБКА: {ex.Message}");
            return new List<AppointmentItem>();
        }
    }

    // === АДМИН: МАСТЕРА САЛОНА ===
    public async Task<List<Master>> GetSalonMastersAsync(int salonId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return (await connection.QueryAsync<Master>(@"
        SELECT m.id, m.fio, m.phone, m.passport, m.grade 
        FROM master m
        JOIN salontomaster stm ON m.id = stm.master_id
        WHERE stm.salon_id = @SalonId", new { SalonId = salonId })).ToList();
    }

    // === АДМИН: КЛИЕНТЫ САЛОНА ===
    public async Task<List<SalonClient>> GetSalonClientsAsync(int salonId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
        SELECT DISTINCT 
            c.id, 
            c.name AS Name, 
            c.phone AS Phone, 
            c.login AS Login
        FROM client c
        JOIN appointments a ON c.id = a.client_id
        JOIN master m ON a.master_id = m.id
        JOIN salontomaster stm ON m.id = stm.master_id
        WHERE stm.salon_id = @SalonId
        ORDER BY c.name";

        return (await connection.QueryAsync<SalonClient>(sql, new { SalonId = salonId })).ToList();
    }

    // === АДМИН: УСЛУГИ САЛОНА ===
    public async Task<List<ServiceItem>> GetSalonServicesAsync(int salonId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        return (await connection.QueryAsync<ServiceItem>(@"
        SELECT DISTINCT s.id, s.name, s.duration, s.description
        FROM service s
        JOIN mastertoservice mts ON s.id = mts.service_id
        JOIN master m ON mts.master_id = m.id
        JOIN salontomaster stm ON m.id = stm.master_id
        WHERE stm.salon_id = @SalonId
        ORDER BY s.name", new { SalonId = salonId })).ToList();
    }
    public async Task<Salon?> GetSalonAsync(int id)
    {
        using var c = CreateConnection(); await c.OpenAsync();
        return await c.QueryFirstOrDefaultAsync<Salon>("SELECT * FROM salon WHERE id = @Id", new { Id = id });
    }
    public async Task<decimal> GetServicePriceAsync(int masterId, int serviceId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var price = await connection.ExecuteScalarAsync<decimal?>(@"
        SELECT price FROM mastertoservice 
        WHERE master_id = @MasterId AND service_id = @ServiceId",
            new { MasterId = masterId, ServiceId = serviceId });

        return price ?? 0;
    }
    // === ОБНОВЛЕНИЕ ПРОФИЛЯ (ИМЯ И ПАРОЛЬ) ===
    public async Task UpdateClientProfileAsync(int clientId, string newName, string? newPassword = null)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            // Только имя
            await connection.ExecuteAsync(
                "UPDATE client SET name = @Name WHERE id = @Id",
                new { Name = newName, Id = clientId });
        }
        else
        {
            // ✅ ХЭШИРУЕМ ПАРОЛЬ
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt(12));

            await connection.ExecuteAsync(
                "UPDATE client SET name = @Name, password_hash = @Password WHERE id = @Id",
                new { Name = newName, Password = hashedPassword, Id = clientId });
        }
    }
}