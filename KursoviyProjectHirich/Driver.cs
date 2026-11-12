using System.ComponentModel.DataAnnotations;

namespace KursoviyProjectHirich
{
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CarModel { get; set; }

        public double PricePerKm { get; set; }

        public double AverageSpeedKmH { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string DisplayInfo => $"{FullName} ({CarModel}) — {PricePerKm} грн/км";
    }
}