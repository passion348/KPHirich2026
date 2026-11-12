using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace KursoviyProjectHirich
{
    public partial class MainWindow : Window
    {
        private GMapMarker? _markerA;
        private GMapMarker? _markerB;
        private PointLatLng? _pointA;
        private PointLatLng? _pointB;
        private double _currentDistanceKm = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (PanelHome == null) return;

            PanelHome.Visibility = Visibility.Collapsed;
            PanelOrders.Visibility = Visibility.Collapsed;
            PanelCreateOrder.Visibility = Visibility.Collapsed;
            PanelReportBug.Visibility = Visibility.Collapsed;
            PanelSettings.Visibility = Visibility.Collapsed;

            if (sender == BtnHome)
            {
                PanelHome.Visibility = Visibility.Visible;
            }
            else if (sender == BtnOrders)
            {
                PanelOrders.Visibility = Visibility.Visible;
                LoadOrders();
            }
            else if (sender == BtnCreateOrder)
            {
                PanelCreateOrder.Visibility = Visibility.Visible;
                LoadDrivers();
                UpdateOrderPageInfo();
            }
            else if (sender == BtnReportBug)
            {
                PanelReportBug.Visibility = Visibility.Visible;
            }
            else if (sender == BtnSettings)
            {
                PanelSettings.Visibility = Visibility.Visible;
                LoadSettingsData();
            }
        }

        private void LoadSettingsData()
        {
            try
            {
                using (var context = new DataContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == UserSession.CurrentUserId);
                    if (user != null)
                    {
                        TxtBoxSettingsName.Text = user.Name;
                        TxtBoxSettingsEmail.Text = user.Email;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження налаштувань: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            string currentPass = PwdBoxCurrent.Password;
            string newPass = PwdBoxNew.Password;

            if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Будь ласка, заповніть поля поточного та нового пароля.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new DataContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == UserSession.CurrentUserId);
                    if (user != null)
                    {
                        if (user.Password == currentPass)
                        {
                            user.Password = newPass;
                            context.SaveChanges();
                            MessageBox.Show("Пароль успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                            PwdBoxCurrent.Clear();
                            PwdBoxNew.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Поточний пароль введено невірно!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні пароля: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGoToReportFromSettings_Click(object sender, RoutedEventArgs e)
        {
            BtnReportBug.IsChecked = true;
            NavButton_Click(BtnReportBug, e);
        }

        private void BtnGoToCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            BtnCreateOrder.IsChecked = true;
            NavButton_Click(BtnCreateOrder, e);
        }

        private void LoadDrivers()
        {
            try
            {
                var drivers = GetDriversFromDb();
                CmbDrivers.ItemsSource = drivers;

                if (drivers.Count > 0 && CmbDrivers.SelectedIndex == -1)
                {
                    CmbDrivers.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження водіїв з бази: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                LstOrders.ItemsSource = GetOrdersFromDb();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження замовлень: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrderPageInfo()
        {
            DpOrderDate.DisplayDateStart = DateTime.Today;

            if (DpOrderDate.SelectedDate == null)
            {
                DpOrderDate.SelectedDate = DateTime.Today.AddDays(1);
            }

            LblOrderFrom.Text = TxtPointA.Text;
            LblOrderTo.Text = TxtPointB.Text;

            if (_pointA.HasValue && _pointB.HasValue)
            {
                _currentDistanceKm = GetDistanceInKm(_pointA.Value, _pointB.Value);
                LblDistance.Text = $"{_currentDistanceKm:F1} км";
            }
            else
            {
                _currentDistanceKm = 0;
                LblDistance.Text = "0 км";
            }

            RecalculateOrderDetails();
        }

        private void CmbDrivers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateOrderDetails();
        }

        private void RecalculateOrderDetails()
        {
            if (CmbDrivers.SelectedItem is Driver selectedDriver && _currentDistanceKm > 0)
            {
                var (totalPrice, hours, minutes) = CalculateOrderDetails(_currentDistanceKm, selectedDriver);
                LblTotalPrice.Text = $"{totalPrice:N0} грн";
                LblEstimatedTime.Text = $"{hours} год {minutes} хв";
            }
            else
            {
                LblTotalPrice.Text = "0 грн";
                LblEstimatedTime.Text = "0 год 0 хв";
            }
        }

        private void CargoParam_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtCargoWeight == null || TxtCargoVolume == null || LblWeightError == null || LblVolumeError == null || BtnConfirmOrder == null)
                return;

            var (weightExceeded, volumeExceeded) = ValidateCargoParams(TxtCargoWeight.Text, TxtCargoVolume.Text);

            LblWeightError.Visibility = weightExceeded ? Visibility.Visible : Visibility.Collapsed;
            TxtCargoWeight.BorderBrush = weightExceeded ? Brushes.Red : (Brush)new BrushConverter().ConvertFrom("#CBD5E1");

            LblVolumeError.Visibility = volumeExceeded ? Visibility.Visible : Visibility.Collapsed;
            TxtCargoVolume.BorderBrush = volumeExceeded ? Brushes.Red : (Brush)new BrushConverter().ConvertFrom("#CBD5E1");

            BtnConfirmOrder.IsEnabled = !weightExceeded && !volumeExceeded;
        }

        private void BtnConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (CmbDrivers.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть водія!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double.TryParse(TxtCargoWeight.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double weight);
            double.TryParse(TxtCargoVolume.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double volume);

            var (weightExceeded, volumeExceeded) = ValidateCargoParams(TxtCargoWeight.Text, TxtCargoVolume.Text);
            if (weightExceeded || volumeExceeded)
            {
                MessageBox.Show("Не вдалося створити замовлення! Параметри вантажу перевищують допустимі норми (макс. 20 тонн та 85 м³).",
                                "Помилка створення", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var selectedDriver = (Driver)CmbDrivers.SelectedItem;
                var selectedCargoItem = (ComboBoxItem)CmbCargoType.SelectedItem;

                double totalPrice = _currentDistanceKm * selectedDriver.PricePerKm;
                DateTime selectedDate = DpOrderDate.SelectedDate ?? DateTime.Today.AddDays(1);

                var newOrder = new Order
                {
                    UserId = UserSession.CurrentUserId,
                    PointA = TxtPointA.Text,
                    PointB = TxtPointB.Text,
                    CargoType = selectedCargoItem != null ? selectedCargoItem.Content.ToString() : "Звичайний вантаж",
                    WeightTons = weight,
                    VolumeM3 = volume,
                    DriverName = selectedDriver.DisplayInfo,
                    DistanceKm = _currentDistanceKm,
                    TotalPrice = totalPrice,
                    DeliveryDate = selectedDate,
                    Status = "Нове замовлення"
                };

                SaveOrderToDb(newOrder);

                MessageBox.Show("Замовлення успішно створено та збережено в базі даних!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                BtnOrders.IsChecked = true;
                NavButton_Click(BtnOrders, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні замовлення в БД: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSendBugReport_Click(object sender, RoutedEventArgs e)
        {
            string subject = TxtBugSubject.Text.Trim();
            string description = TxtBugDescription.Text.Trim();
            string category = ((ComboBoxItem)CmbBugCategory.SelectedItem)?.Content?.ToString() ?? "Загальне";
            string priority = ((ComboBoxItem)CmbBugPriority.SelectedItem)?.Content?.ToString() ?? "Звичайний";

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Будь ласка, заповніть тему та опис проблеми!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SaveBugReportToFile(category, priority, subject, description);

                MessageBox.Show("Дякуємо! Ваш репорт успішно надіслано адміністратору.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                TxtBugSubject.Clear();
                TxtBugDescription.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні репорту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainMap_Loaded(object sender, RoutedEventArgs e)
        {
#pragma warning disable SYSLIB0014
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
#pragma warning restore SYSLIB0014

            GMapProvider.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) CargoApp/1.0";

            MainMap.MapProvider = OpenStreetMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            MainMap.Position = new PointLatLng(50.4501, 30.5234);
            MainMap.MinZoom = 3;
            MainMap.MaxZoom = 18;
            MainMap.Zoom = 6;

            MainMap.ShowCenter = false;
            MainMap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            MainMap.CanDragMap = true;
            MainMap.DragButton = System.Windows.Input.MouseButton.Left;

            MainMap.MouseLeftButtonDown += MainMap_MouseLeftButtonDown;
        }

        private void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var point = e.GetPosition(MainMap);
            PointLatLng latLng = MainMap.FromLocalToLatLng((int)point.X, (int)point.Y);

            string locationName = GetAddressFromCoordinates(latLng.Lat, latLng.Lng);

            if (_pointA == null)
            {
                _pointA = latLng;
                TxtPointA.Text = locationName;
                SetMarkerA(_pointA.Value, locationName);
            }
            else if (_pointB == null)
            {
                _pointB = latLng;
                TxtPointB.Text = locationName;
                SetMarkerB(_pointB.Value, locationName);
            }
        }

        private void BtnBuildRoute_Click(object sender, RoutedEventArgs e)
        {
            BtnGoToCreateOrder.IsEnabled = false;

            string inputA = TxtPointA.Text.Trim();
            string inputB = TxtPointB.Text.Trim();

            if (string.IsNullOrEmpty(inputA) || string.IsNullOrEmpty(inputB))
            {
                MessageBox.Show("Заповніть обидва пункти (А та Б)!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PointLatLng? pointA = GetPointFromInput(inputA);
            PointLatLng? pointB = GetPointFromInput(inputB);

            if (pointA == null)
            {
                MessageBox.Show($"Не знайдено пункт А: {inputA}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pointB == null)
            {
                MessageBox.Show($"Не знайдено пункт Б: {inputB}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _pointA = pointA;
            _pointB = pointB;

            MainMap.Markers.Clear();

            SetMarkerA(_pointA.Value, inputA);
            SetMarkerB(_pointB.Value, inputB);

            MapRoute route = OpenStreetMapProvider.Instance.GetRoute(_pointA.Value, _pointB.Value, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                GMapRoute groute = new GMapRoute(route.Points)
                {
                    Shape = new System.Windows.Shapes.Path
                    {
                        Stroke = Brushes.DeepSkyBlue,
                        StrokeThickness = 5
                    }
                };
                MainMap.Markers.Add(groute);
                BtnGoToCreateOrder.IsEnabled = true;
            }

            MainMap.Position = _pointA.Value;
            MainMap.Zoom = 7;
        }

        private void BtnClearRoute_Click(object sender, RoutedEventArgs e)
        {
            TxtPointA.Clear();
            TxtPointB.Clear();

            _pointA = null;
            _pointB = null;
            _markerA = null;
            _markerB = null;

            MainMap.Markers.Clear();
            BtnGoToCreateOrder.IsEnabled = false;
        }

        private void SetMarkerA(PointLatLng point, string title)
        {
            _markerA = new GMapMarker(point)
            {
                Shape = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.Green,
                    Stroke = Brushes.White,
                    StrokeThickness = 3,
                    ToolTip = "Пункт А: " + title
                }
            };
            MainMap.Markers.Add(_markerA);
        }

        private void SetMarkerB(PointLatLng point, string title)
        {
            _markerB = new GMapMarker(point)
            {
                Shape = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.Red,
                    Stroke = Brushes.White,
                    StrokeThickness = 3,
                    ToolTip = "Пункт Б: " + title
                }
            };
            MainMap.Markers.Add(_markerB);
        }
    }
}