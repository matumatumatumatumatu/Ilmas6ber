using Mapsui.Tiling;
using Mapsui.Extensions;

namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var map = new Mapsui.Map();
            map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
            mapView.Map = map;

            // Fix applied here:
            var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(24.7536, 59.437);
            mapView.Map.Navigator.CenterOnAndZoomTo(new Mapsui.MPoint(x, y), 5.0);
        }
    }
}