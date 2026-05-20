using Ilmas6ber.Models.Locations;
using System.Xml.Linq;
using static Ilmas6ber.Models.Locations.PrivatePinEnumLocationType;

namespace Ilmas6ber.Services.Locations
{
    public class PrivatePinXMLService
    {
        private readonly string _filePath;

        public PrivatePinXMLService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "privatePin.xml");
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
                Longitude = double.Parse(e.Element("Longitude")!.Value),
                Latitude = double.Parse(e.Element("Latitude")!.Value),
                CreatedAt = DateTime.Parse(e.Element("CreatedAt")!.Value),
                ModifiedAt = DateTime.Parse(e.Element("ModifiedAt")!.Value),
                LastVisited = e.Element("LastVisited")?.Value is string lv ? DateTime.Parse(lv) : null,
                ImagePath = e.Element("ImagePath")?.Value
            }).ToList();
        }

        public void Save(IEnumerable<PrivatePinModel> pins)
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
                            new XElement("ImagePath", p.ImagePath)
                        )
                    )
                )
            );

            doc.Save(_filePath);
        }

        public void Add(PrivatePinModel pin)
        {
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
    }
}