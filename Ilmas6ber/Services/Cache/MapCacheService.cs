using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;

namespace Ilmas6ber.Services.Cache
{
    public class MapCacheService
    {
        private readonly string cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "osm_cache");
        public TileLayer CachedTileLayer { get; private set; }

        public MapCacheService()
        {
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            var fileCache = new FileCache(cacheDirectory, "png");

            var tileSource = new HttpTileSource(
                new GlobalSphericalMercator(0, 18),
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                name: "OSM",
                persistentCache: fileCache);

            CachedTileLayer = new TileLayer(tileSource) { Name = "Cached OSM" };
        }
    }
}