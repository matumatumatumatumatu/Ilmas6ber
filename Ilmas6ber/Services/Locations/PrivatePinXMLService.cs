using Ilmas6ber.Models.Locations;
using System.Globalization;
using System.Xml.Linq;
using static Ilmas6ber.Models.Locations.PrivatePinEnumLocationType;

namespace Ilmas6ber.Services.Locations
{
    public class PrivatePinXMLService
    {
        private readonly string _filePath;
        private readonly ElevationService _elevationService;

        public PrivatePinXMLService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "privatePin.xml");
            _elevationService = new ElevationService();
            SeedIfEmpty();
        }

        //hardcoded private pin
        //to delete later
        private void SeedIfEmpty()
        {
            if (File.Exists(_filePath)) return;

            var doc = new XDocument(
                new XElement("PrivatePins",
                    new XElement("PrivatePin",
                        new XAttribute("Id", "a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                        new XElement("Title", "Nice bench"),
                        new XElement("LocationType", "Bench"),
                        new XElement("Description", "Great view from here"),
                        new XElement("Longitude", 24.467395223296307),
                        new XElement("Latitude", 59.12955763289114),
                        new XElement("CreatedAt", DateTime.UtcNow.ToString("O")),
                        new XElement("ModifiedAt", DateTime.UtcNow.ToString("O")),
                        new XElement("LastVisited", ""),
                        new XElement("ImagePath", ""),
                        new XElement("Elevation","")
                    )
                )
            );
            
            doc.Save(_filePath);
        }

        public List<PrivatePinModel> Load()
        {
            if (!File.Exists(_filePath))
                return new List<PrivatePinModel>();

            var doc = XDocument.Load(_filePath);

            return doc.Root!.Elements("PrivatePin").Select(e => new PrivatePinModel
            {
                Id = Guid.Parse(e.Attribute("Id")!.Value),
                Title = e.Element("Title")?.Value ?? string.Empty,
                LocationType = Enum.Parse<LocationType>(e.Element("LocationType")!.Value),
                Description = e.Element("Description")?.Value ?? string.Empty,
                Longitude = double.Parse(e.Element("Longitude")!.Value, CultureInfo.InvariantCulture),
                Latitude = double.Parse(e.Element("Latitude")!.Value, CultureInfo.InvariantCulture),
                CreatedAt = DateTime.Parse(e.Element("CreatedAt")!.Value),
                ModifiedAt = DateTime.Parse(e.Element("ModifiedAt")!.Value),
                LastVisited = e.Element("LastVisited")?.Value is string lv && lv != "" ? DateTime.Parse(lv) : null,
                ImagePath = e.Element("ImagePath")?.Value is string ip && ip != "" ? ip : null,
                Elevation = e.Element("Elevation")?.Value is string el && el != ""?double.Parse(el, CultureInfo.InvariantCulture) : null
            }).ToList();
        }

        public void Save(IEnumerable<PrivatePinModel> pins)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("PrivatePins",
                        pins.Select(p =>
                            new XElement("PrivatePin",
                                new XAttribute("Id", p.Id),
                                new XElement("Title", p.Title),
                                new XElement("LocationType", p.LocationType),
                                new XElement("Description", p.Description),
                                new XElement("Longitude", p.Longitude),
                                new XElement("Latitude", p.Latitude),
                                new XElement("CreatedAt", p.CreatedAt.ToString("O")),
                                new XElement("ModifiedAt", p.ModifiedAt.ToString("O")),
                                new XElement("LastVisited", p.LastVisited?.ToString("O")),
                                new XElement("ImagePath", p.ImagePath),
                                new XElement("Elevation", p.Elevation?.ToString(CultureInfo.InvariantCulture) ?? "")
                            )
                        )
                    )
                );

                doc.Save(_filePath);
                Console.WriteLine($"[DEBUG] Saved to {_filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Save failed: {ex.Message}");
            }
        }

        public async Task Add(PrivatePinModel pin)
        {
            
            
                pin.Elevation = await _elevationService.GetElevation(pin.Latitude, pin.Longitude);
                pin.CreatedAt = DateTime.UtcNow;
                pin.ModifiedAt = DateTime.UtcNow;
                var list = Load();
                list.Add(pin);
                Save(list);
            
        }

        public void Delete(Guid id)
        {
            var list = Load();
            list.RemoveAll(p => p.Id == id);
            Save(list);
        }

        public void Update(PrivatePinModel updated)
        {
            updated.ModifiedAt = DateTime.UtcNow;
            var list = Load();
            var idx = list.FindIndex(p => p.Id == updated.Id);
            if (idx >= 0) list[idx] = updated;
            Save(list);
        }
        //temporary Task for testing elevation api without frontend
        public async Task BackfillElevationsAsync()
        {
            var list = Load();
            bool changed = false;

            foreach (var pin in list.Where(p => p.Elevation == null))
            {
                pin.Elevation = await _elevationService.GetElevation(pin.Latitude, pin.Longitude);
                changed = true;
            }

            if (changed) Save(list);
        }
    }
}