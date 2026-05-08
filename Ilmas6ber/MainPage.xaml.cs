using Mapsui;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Widgets.InfoWidgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Styles;



namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        private MyLocationLayer _myLocationLayer;
        MapControl mapControl = new Mapsui.UI.Maui.MapControl();
        private TextBoxWidget _coordinatesWidget;
        private bool _isCheckingLocation = false;
        private Location _lastLocation;
        public MainPage()
        {
            InitializeComponent();

            // Tile layer (only once)
            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            // Location layer with custom PNG
            _myLocationLayer = new MyLocationLayer(mapControl.Map)
            {
                Style = new ImageStyle
                {
                    Image = new Mapsui.Styles.Image
                    {
                        Source = "embedded://Ilmas6ber.Resources.Images.locationpin.png"
                    },
                    SymbolScale = 0.5
                }
            };
            mapControl.Map?.Layers.Add(_myLocationLayer);

            // Widgets
            mapControl.Map?.Widgets.Add(new ScaleBarWidget(mapControl.Map)
            {
                TextAlignment = Alignment.Center,
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top
            });
            mapControl.Map?.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });

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

            Content = mapControl;
        }


        public async Task StartLocationListening()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationListeningRequest request = new GeolocationListeningRequest(
                    GeolocationAccuracy.Medium,
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
            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _myLocationLayer.UpdateMyLocation(new MPoint(location.Longitude, location.Latitude));
                _coordinatesWidget.Text = $"Lat: {location.Latitude}, Lon: {location.Longitude}";
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
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await StartLocationListening();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationListening();
        }


    }
}