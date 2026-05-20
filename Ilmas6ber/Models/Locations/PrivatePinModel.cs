using System;
using System.Collections.Generic;
using System.Text;
using static Ilmas6ber.Models.Locations.PrivatePinEnumLocationType;

namespace Ilmas6ber.Models.Locations
{
    public class PrivatePinModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public LocationType LocationType { get; set; }
        public string Description { get; set; }
        public Mapsui.MPoint Coordinates { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? LastVisited { get; set; }
        public string? ImagePath { get; set; }
    }
}
