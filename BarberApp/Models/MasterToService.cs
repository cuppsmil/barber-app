// Models/MasterToService.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace BarberApp.Models;

public class MasterToService
{
    public int Id { get; set; }

    [Column("master_id")]   // ← Явно указываем имя колонки в БД
    public int MasterId { get; set; }

    [Column("service_id")]  // ← Явно указываем имя колонки в БД
    public int ServiceId { get; set; }
}