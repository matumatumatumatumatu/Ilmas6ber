using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;

namespace Ilmas6ber.Services.Cache
{
    public class MapCacheService
    {
        public HttpTileSource CachedTileSource { get; private set; }

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

            CachedTileSource = tileSource;
            CachedTileLayer = new TileLayer(tileSource) { Name = "Cached OSM" };
        }
    }
}