using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Ilmas6ber
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            var location = new Location(59.437, 24.7536); // Tallinn
            var span = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(5));

            map.MoveToRegion(span);
        }
    }
}
