using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using Mapsui.Widgets.ScaleBar;
using System.Threading.Tasks;
using Mapsui.Styles.Thematics;
using Microsoft.Maui.Storage;

namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        private MemoryLayer _locationLayer;
        private PointFeature _locationFeature;
        private ImageStyle _pinStyle;
        MapControl mapControl = new Mapsui.UI.Maui.MapControl();
        private TextBoxWidget _coordinatesWidget;
        private bool _isCheckingLocation = false;
        private Location _lastLocation;
        private CancellationTokenSource _zoomCancellationToken;
        private bool _areZoomButtonsExpanded = false;

        public MainPage()
        {
            InitializeComponent();

            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            _locationFeature = new PointFeature(new MPoint(0, 0));

            _pinStyle = ImageStyles.CreatePinStyle();
            _pinStyle.Image = new Mapsui.Styles.Image
            {
                Source = "embedded://Ilmas6ber.Resources.Images.locationpin.png"
            };
            _pinStyle.SymbolScale = 0.5;
            _pinStyle.Enabled = true;

            _locationLayer = new MemoryLayer
            {
                Name = "UserLocation",
                Features = new[] { _locationFeature },
                Style = _pinStyle,
                MinVisible = double.MinValue,
                MaxVisible = double.MaxValue
            };

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
            Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");

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

            
            mapControl.Map.Navigator.OverrideZoomBounds = new MMinMax(5, 2250);

            await StartLocationListening();
        }
        //Rakenduse sulgemine
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopLocationListening();
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
    }
}