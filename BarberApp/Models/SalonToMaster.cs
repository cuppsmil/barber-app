// Models/SalonToMaster.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace BarberApp.Models;

public class SalonToMaster
{
    public int Id { get; set; }

    [Column("salon_id")]  // ← Явно указываем имя колонки в БД
    public int SalonId { get; set; }

    [Column("master_id")] // ← Явно указываем имя колонки в БД
    public int MasterId { get; set; }
}