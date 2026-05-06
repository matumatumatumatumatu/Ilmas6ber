using Mapsui;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using Mapsui.Widgets.InfoWidgets;


namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        bool _isCheckingLocation;
        public MainPage()
        {
            InitializeComponent();

            var mapControl = new Mapsui.UI.Maui.MapControl();
            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
            Content = mapControl;
            mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            mapControl.Map?.Widgets.Add(new ScaleBarWidget(mapControl.Map)
            {
                TextAlignment = Alignment.Center,
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top
            });
            mapControl.Map?.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
            var button = new ButtonWidget
            {
                Text = "Locate",
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top,
                Margin = new MRect(20, 100),
            };
            button.Tapped += (sender, args) =>
            {
                GetCurrentLocation();
            };
            mapControl.Map?.Widgets.Add(button);



        }
        public async Task GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

               CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                // Unable to get location
            }
            finally
            {
                _isCheckingLocation = false;
            }

        }
        private async void OnGetLocationClicked(object sender, EventArgs e)
        {
            await GetCurrentLocation();
        }
    }
}