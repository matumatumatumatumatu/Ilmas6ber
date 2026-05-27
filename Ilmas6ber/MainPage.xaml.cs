using BruTile.Predefined;
using BruTile.Web;
using Ilmas6ber.Services.Locations;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using Mapsui.Widgets.ScaleBar;
using Microsoft.Maui.Storage;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using MySqlConnector;


namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        private readonly PrivatePinXMLService _privatePinXMLService;
        private bool _isSatellite = false;
        private MemoryLayer _locationLayer;
        private MemoryLayer _privatePinlayer;
        private PointFeature _locationFeature;
        private ImageStyle _pinStyle;
        private ImageStyle _privatePinStyle;
        private ILayer? basicLayer;
        private ILayer? satelliteLayer;
        MapControl mapControl = new Mapsui.UI.Maui.MapControl();
        private TextBoxWidget _coordinatesWidget;
        private bool _isCheckingLocation = false;
        private Location _lastLocation;
        private CancellationTokenSource _zoomCancellationToken;
        private bool _areZoomButtonsExpanded = false;

        public MainPage(PrivatePinXMLService privatePinXMLService)
        {
            _privatePinXMLService = privatePinXMLService;
            InitializeComponent();
            //basic map source
            basicLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer();
            mapControl.Map?.Layers.Add(basicLayer);

            //satellite map source
            var tileSource = new HttpTileSource(
                 new GlobalSphericalMercator(0, 19),
                "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
                name: "Esri World Imagery"
            );
            satelliteLayer = new TileLayer(tileSource) { Name = "Satellite" };

            _locationFeature = new PointFeature(new MPoint(0, 0));

            _privatePinStyle = ImageStyles.CreatePinStyle();
            _privatePinStyle.Image = new Mapsui.Styles.Image
            {
                Source = "embedded://Ilmas6ber.Resources.Images.privatelocationpin.png"
            };
            _privatePinStyle.SymbolScale = 0.4;
            _privatePinStyle.Enabled = true;

            _pinStyle = ImageStyles.CreatePinStyle();
            _pinStyle.Image = new Mapsui.Styles.Image
            {
                Source = "embedded://Ilmas6ber.Resources.Images.locationpin.png"
            };
            _pinStyle.SymbolScale = 0.5;
            _pinStyle.Enabled = true;

            var privatePins = _privatePinXMLService.Load();
            _privatePinlayer = new MemoryLayer
            {
                Name="PrivatePin",
                Features = privatePins.Select(p =>
                {
                    var (x, y) = SphericalMercator.FromLonLat(p.Longitude, p.Latitude);
                    var feature = new PointFeature(x, y);
                    feature["Id"] = p.Id.ToString();
                    feature["Title"] = p.Title;
                    return feature;
                }).ToList(),
                Style = _privatePinStyle,
                MinVisible = double.MinValue,
                MaxVisible = double.MaxValue,
            };
            _locationLayer = new MemoryLayer
            {
                Name = "UserLocation",
                Features = new[] { _locationFeature },
                Style = _pinStyle,
                MinVisible = double.MinValue,
                MaxVisible = double.MaxValue
            };
            mapControl.Map?.Layers.Add(_privatePinlayer);
            mapControl.Map?.Layers.Add(_locationLayer);

            mapControl.Map?.Widgets.Add(new ScaleBarWidget(mapControl.Map)
            {
                TextAlignment = Alignment.Center,
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top
            });
            
            //Coordinates test textbox
            _coordinatesWidget = new TextBoxWidget
            {
                Text = "Waiting for location...",
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom,
                Margin = new MRect(100, 100),
                BackColor = Mapsui.Styles.Color.White,
                TextColor = Mapsui.Styles.Color.Black,
            };
            mapControl.Map?.Widgets.Add(_coordinatesWidget);
            //Coordinates test textbox
            
            MapGrid.Insert(0, mapControl);

            
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => CollapseZoomButtonsBottomRight();
            mapControl.GestureRecognizers.Add(tapGesture);
            mapControl.MapTapped += ViewOptions;



        }
        //Asukoha meetodid BEGIN
        public async Task StartLocationListening()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationListeningRequest request = new GeolocationListeningRequest(
                    GeolocationAccuracy.High,
                    TimeSpan.FromSeconds(10)
                );

                bool success = await Geolocation.Default.StartListeningForegroundAsync(request);

                if (success)
                {
                    Geolocation.Default.LocationChanged += OnLocationChanged;
                    Geolocation.Default.ListeningFailed += OnListeningFailed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting location listening: {ex.Message}");
                _isCheckingLocation = false;
                await EnsureLocationPermission();
            }
        }

        private void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
        {
            var location = e.Location;

            if (_lastLocation != null)
            {
                double movedMeters = Location.CalculateDistance(_lastLocation, location, DistanceUnits.Kilometers) * 1000;
                if (movedMeters < 10) return;
            }

            _lastLocation = location;
            

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var point = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);

                _locationFeature = new PointFeature(new MPoint(point.x, point.y));
                _locationLayer.Features = new[] { _locationFeature };
                _locationLayer.Style = _pinStyle;

                _locationLayer.DataHasChanged();
                
                mapControl.Refresh();
            });
        }

        private void OnListeningFailed(object sender, GeolocationListeningFailedEventArgs e)
        {
            Console.WriteLine($"Listening failed: {e.Error}");
            _isCheckingLocation = false;
        }

        public void StopLocationListening()
        {
            Geolocation.Default.LocationChanged -= OnLocationChanged;
            Geolocation.Default.ListeningFailed -= OnListeningFailed;
            Geolocation.Default.StopListeningForeground();
            _isCheckingLocation = false;
        }
        //Asukoha meetodid END
        //Klõpsamis funktsioonid BEGIN
        private async void ViewOptions(object? sender, MapEventArgs e)
        {
            var worldPos = e.WorldPosition;
            var (lon, lat) = SphericalMercator.ToLonLat(worldPos.X, worldPos.Y);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _coordinatesWidget.Text = $"Lat: {lat:F5}, Lon: {lon:F5}";

                string action = await Application.Current.MainPage.DisplayActionSheet(
                    $"Lat: {lat:F5}, Lon: {lon:F5}",
                    "Cancel",
                    null,
                    //"Add point",
                    "Open in navigation app",
                    "Copy coordinates"
                );

                switch (action)
                {
                    //case "Add point":
                    //    AddPinPoint(lon, lat);
                     //   break;
                    case "Open in navigation app":
                        await GetDirections(lon, lat);
                        break;
                    case "Copy coordinates":
                        await CopyCoordinates(lon, lat);
                        break;
                }
            });
        }
        private async Task AddPinPoint(double lon, double lat)
        {

        }
        private async Task GetDirections(double lon, double lat)
        {
            var location = new Location(lat, lon);
            var options = new MapLaunchOptions { Name = "Selected Location" };
            await Microsoft.Maui.ApplicationModel.Map.OpenAsync(location, options);
        }
        private async Task CopyCoordinates(double lon, double lat)
        {
            await Clipboard.SetTextAsync($"{lat:F5}, {lon:F5}");
        }
        //Klõpsamis funktsioonid END

        //Kaardi layerid BEGIN
        private void ToggleMapLayer()
        {
            if (mapControl.Map == null) return;

            _isSatellite = !_isSatellite;

            var toRemove = _isSatellite ? basicLayer! : satelliteLayer!;
            var toAdd = _isSatellite ? satelliteLayer! : basicLayer!;

            mapControl.Map.Layers.Remove(toRemove);
            mapControl.Map.Layers.Insert(0, toAdd);
        }

        private void OnToggleMapClicked(object sender, EventArgs e)
        {
            ToggleMapLayer();
            var btn = (Button)sender;
            btn.Text = _isSatellite ? "🌍" : "🛰️";
        }
        //Kaardi layerid END

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //Kaardi algus positsioon
            var estoniaCenter = SphericalMercator.FromLonLat(25.0, 58.8);
            mapControl.Map.Navigator.CenterOnAndZoomTo(
                new MPoint(estoniaCenter.x, estoniaCenter.y),
                resolution: 1000
            );
            //Kaardi piirid
            var swCorner = SphericalMercator.FromLonLat(20.0, 56.5);
            var neCorner = SphericalMercator.FromLonLat(30.0, 61.5);
            mapControl.Map.Navigator.OverridePanBounds = new MRect(
                swCorner.x, swCorner.y,
                neCorner.x, neCorner.y
            );

            
            mapControl.Map.Navigator.OverrideZoomBounds = new MMinMax(1, 2250);

            if (Window != null)
            {
                Window.Activated += OnWindowActivated;
            }

            await StartLocationListening();
        }
        //Rakenduse sulgemine
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationListening();

            if (Window != null)
            {
                Window.Activated -= OnWindowActivated;
            }
        }


        private void OnZoomButtonClickedBottomRight(object sender, EventArgs e)
        {
            
            if (!_areZoomButtonsExpanded)
            {
                ExpandZoomButtonsBottomRight();
            }
            else
            {
                CollapseZoomButtonsBottomRight();
            }
        }

        private async void OnZoomInClicked(object sender, EventArgs e)
        {
            await SmoothZoomIn();
        }

        private async void OnZoomOutClicked(object sender, EventArgs e)
        {
            await SmoothZoomOut();
        }

        private void OnOverlayTappedBottomRight(object sender, TappedEventArgs e)
        {
            
            if (_areZoomButtonsExpanded)
            {
                CollapseZoomButtonsBottomRight();
            }
        }

        private async void ExpandZoomButtonsBottomRight()
        {
            _areZoomButtonsExpanded = true;
            ZoomInButtonBottomRight.IsEnabled = true;
            ZoomOutButtonBottomRight.IsEnabled = true;

            
            await Task.WhenAll(
                ZoomInButtonBottomRight.FadeTo(1, 200, Easing.CubicOut),
                ZoomOutButtonBottomRight.FadeTo(1, 200, Easing.CubicOut)
            );
        }

        private async void CollapseZoomButtonsBottomRight()
        {
            _areZoomButtonsExpanded = false;

            
            await Task.WhenAll(
                ZoomInButtonBottomRight.FadeTo(0, 200, Easing.CubicIn),
                ZoomOutButtonBottomRight.FadeTo(0, 200, Easing.CubicIn)
            );

            ZoomInButtonBottomRight.IsEnabled = false;
            ZoomOutButtonBottomRight.IsEnabled = false;
        }

        private async Task SmoothZoomIn()
        {
            _zoomCancellationToken?.Cancel();
            _zoomCancellationToken = new CancellationTokenSource();

            try
            {
                var currentResolution = mapControl.Map?.Navigator.Viewport.Resolution ?? 1;
                var targetResolution = currentResolution / 2.0;
                const int duration = 300;
                const int steps = 30;
                var stepDuration = duration / (double)steps;

                for (int i = 0; i < steps; i++)
                {
                    if (_zoomCancellationToken.Token.IsCancellationRequested)
                        return;

                    var progress = (i + 1) / (double)steps;
                    var easeProgress = EaseInOutCubic(progress);
                    var newResolution = currentResolution - (currentResolution - targetResolution) * easeProgress;

                    mapControl.Map?.Navigator.ZoomTo(newResolution);
                    mapControl.Refresh();

                    await Task.Delay((int)stepDuration, _zoomCancellationToken.Token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task SmoothZoomOut()
        {
            _zoomCancellationToken?.Cancel();
            _zoomCancellationToken = new CancellationTokenSource();

            try
            {
                var currentResolution = mapControl.Map?.Navigator.Viewport.Resolution ?? 1;
                var targetResolution = currentResolution * 2.0;
                const int duration = 300;
                const int steps = 30;
                var stepDuration = duration / (double)steps;

                for (int i = 0; i < steps; i++)
                {
                    if (_zoomCancellationToken.Token.IsCancellationRequested)
                        return;

                    var progress = (i + 1) / (double)steps;
                    var easeProgress = EaseInOutCubic(progress);
                    var newResolution = currentResolution + (targetResolution - currentResolution) * easeProgress;

                    mapControl.Map?.Navigator.ZoomTo(newResolution);
                    mapControl.Refresh();

                    await Task.Delay((int)stepDuration, _zoomCancellationToken.Token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private static double EaseInOutCubic(double progress)
        {
            return progress < 0.5
                ? 4 * progress * progress * progress
                : 1 - Math.Pow(-2 * progress + 2, 3) / 2;
        }
        private async Task<bool> EnsureLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            // 1. If already granted, proceed
            if (status == PermissionStatus.Granted)
            {
                
                return true;
            }

            // 2. If denied previously, the system prompt will not show up again.
            // We must explain the issue and send them to the device settings page.
            if (status == PermissionStatus.Denied)
            {
                bool openSettings = await DisplayAlert(
                    "Permission Required",
                    "Location access was denied. Please enable it in the app settings to use this feature.",
                    "Go to Settings",
                    "Cancel");

                if (openSettings)
                {
                    AppInfo.Current.ShowSettingsUI();
                    
                }
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;

        }
        private async void OnWindowActivated(object sender, EventArgs e)
        {
            // Explicitly check permissions silently without re-triggering endless prompt loops
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                // The user just granted permission in settings and came back
                await FetchDeviceLocation();
            }
        }

        private async Task FetchDeviceLocation()
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

                if (location != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                    StartLocationListening();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions like GPS hardware being turned off
            }
        }

        public async Task<List<string>> GetDataAsync()
        {
            var results = new List<string>();

            await using var conn = new MySqlConnection(Environment.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = new MySqlCommand("SELECT name FROM my_table", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(reader.GetString(0));
            }

            return results;
        }

    }
}