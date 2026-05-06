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
            mapControl.Map?.Widgets.Add(new MouseCoordinatesWidget());
        }
    }
}