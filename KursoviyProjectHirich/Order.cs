using System;
using System.ComponentModel.DataAnnotations;

namespace KursoviyProjectHirich
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PointA { get; set; }
        public string PointB { get; set; }
        public string CargoType { get; set; }
        public double WeightTons { get; set; }
        public double VolumeM3 { get; set; }
        public string DriverName { get; set; }
        public double DistanceKm { get; set; }
        public double TotalPrice { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Нове замовлення";
        public string DisplayInfo => $"{PointA} ➔ {PointB} | Дата: {DeliveryDate:dd.MM.yyyy} | {CargoType} ({WeightTons}т / {VolumeM3}м³) | Водій: {DriverName} | {TotalPrice:N0} грн";
    }
}