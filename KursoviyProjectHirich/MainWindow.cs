using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using GMap.NET;
using GMap.NET.MapProviders;
using Microsoft.EntityFrameworkCore;

namespace KursoviyProjectHirich
{
    public partial class MainWindow
    {
        public string GetAddressFromCoordinates(double lat, double lng)
        {
            try
            {
                string url = string.Format(CultureInfo.InvariantCulture,
                    "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}&zoom=18&addressdetails=1",
                    lat, lng);

                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "CargoTransportationApp/1.0");
                    webClient.Encoding = System.Text.Encoding.UTF8;

                    string json = webClient.DownloadString(url);

                    if (json.Contains("\"road\":"))
                    {
                        string road = GetJsonValue(json, "road");
                        string houseNumber = GetJsonValue(json, "house_number");
                        string city = GetJsonValue(json, "city");
                        if (string.IsNullOrEmpty(city)) city = GetJsonValue(json, "town");
                        if (string.IsNullOrEmpty(city)) city = GetJsonValue(json, "village");

                        string address = road;
                        if (!string.IsNullOrEmpty(houseNumber)) address += ", " + houseNumber;
                        if (!string.IsNullOrEmpty(city)) address += " (" + city + ")";

                        return string.IsNullOrEmpty(address) ? $"{lat:F4}, {lng:F4}" : address;
                    }
                    else if (json.Contains("\"display_name\":"))
                    {
                        string displayName = GetJsonValue(json, "display_name");
                        string[] parts = displayName.Split(',');
                        return parts.Length >= 2 ? $"{parts[0].Trim()}, {parts[1].Trim()}" : displayName;
                    }
                }
            }
            catch
            {
            }

            return $"{lat.ToString("F4", CultureInfo.InvariantCulture)}, {lng.ToString("F4", CultureInfo.InvariantCulture)}";
        }

        private string GetJsonValue(string json, string key)
        {
            string pattern = $"\"{key}\":\"";
            int startIndex = json.IndexOf(pattern);
            if (startIndex == -1) return string.Empty;

            startIndex += pattern.Length;
            int endIndex = json.IndexOf("\"", startIndex);
            if (endIndex == -1) return string.Empty;

            return json.Substring(startIndex, endIndex - startIndex);
        }

        public PointLatLng? GetPointFromInput(string input)
        {
            string[] parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                string latStr = parts[0].Trim().Replace(',', '.');
                string lngStr = parts[1].Trim().Replace(',', '.');

                if (double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lngStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
                {
                    return new PointLatLng(lat, lng);
                }
            }

            return OpenStreetMapProvider.Instance.GetPoint(input, out _);
        }

        public double GetDistanceInKm(PointLatLng p1, PointLatLng p2)
        {
            double r = 6371;
            double dLat = ToRadians(p2.Lat - p1.Lat);
            double dLng = ToRadians(p2.Lng - p1.Lng);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(p1.Lat)) * Math.Cos(ToRadians(p2.Lat)) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        private double ToRadians(double val) => (Math.PI / 180) * val;

        public (double totalPrice, int hours, int minutes) CalculateOrderDetails(double distanceKm, Driver driver)
        {
            if (driver == null || distanceKm <= 0)
                return (0, 0, 0);

            double totalPrice = distanceKm * driver.PricePerKm;

            double drivingHours = distanceKm / driver.AverageSpeedKmH;

            int numberOfBreaks = (int)(drivingHours / 5.0);
            double totalRestHours = numberOfBreaks * 2.0;

            double totalHoursWithRest = drivingHours + totalRestHours;

            int hours = (int)Math.Floor(totalHoursWithRest);
            int minutes = (int)Math.Round((totalHoursWithRest - hours) * 60);

            return (totalPrice, hours, minutes);
        }

        public (bool weightExceeded, bool volumeExceeded) ValidateCargoParams(string weightText, string volumeText)
        {
            double.TryParse(weightText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double w);
            double.TryParse(volumeText.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double v);

            return (w > 20.0, v > 85.0);
        }

        public List<Driver> GetDriversFromDb()
        {
            using (var db = new DataContext())
            {
                return db.Drivers.ToList();
            }
        }

        public List<Order> GetOrdersFromDb()
        {
            using (var db = new DataContext())
            {
                return db.Orders
                         .AsNoTracking()
                         .Where(o => o.UserId == UserSession.CurrentUserId)
                         .OrderByDescending(o => o.CreatedAt)
                         .ToList();
            }
        }

        public void SaveOrderToDb(Order order)
        {
            using (var db = new DataContext())
            {
                db.Orders.Add(order);
                db.SaveChanges();
            }
        }

        public void SaveBugReportToFile(string category, string priority, string subject, string description)
        {
            string reportData = $"=== БАГ-РЕПОРТ [{DateTime.Now:dd.MM.yyyy HH:mm}] ===\n" +
                               $"Категорія: {category}\n" +
                               $"Пріоритет: {priority}\n" +
                               $"Тема: {subject}\n" +
                               $"Опис:\n{description}\n" +
                               $"===========================================\n\n";

            File.AppendAllText("bug_reports.txt", reportData);
        }
    }
}