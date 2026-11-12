using System.Linq;
using System.Windows;

namespace KursoviyProjectHirich
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var db = new DataContext())
            {
                db.Database.EnsureCreated();

                if (!db.Drivers.Any())
                {
                    db.Drivers.AddRange(
                        new Driver { FirstName = "Олександр", LastName = "Іваненко", CarModel = "Mercedes-Benz Actros", PricePerKm = 30.0, AverageSpeedKmH = 65.0 },
                        new Driver { FirstName = "Василь", LastName = "Петренко", CarModel = "MAN TGX", PricePerKm = 24.5, AverageSpeedKmH = 60.0 },
                        new Driver { FirstName = "Дмитро", LastName = "Ковальов", CarModel = "Volvo FH", PricePerKm = 32.0, AverageSpeedKmH = 75.0 }
                    );

                    db.SaveChanges();
                }
            }
        }
    }
}