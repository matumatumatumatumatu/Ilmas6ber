using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ilmas6ber.Services.Cache
{
    public class SatelliteMapCacheService
    {
        public HttpTileSource CachedTileSource { get; private set; }

        private readonly string cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "osm_satellite_cache");
        public TileLayer CachedTileLayer { get; private set; }

        public SatelliteMapCacheService()
        {
            if (!Directory.Exists(cacheDirectory))
                Directory.CreateDirectory(cacheDirectory);

            var fileCache = new FileCache(cacheDirectory, "png");

            var tileSource = new HttpTileSource(
                 new GlobalSphericalMercator(0, 18),
                "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}",
                name: "OSMs",
                persistentCache: fileCache);

            CachedTileSource = tileSource;
            CachedTileLayer = new TileLayer(tileSource) { Name = "Cached Satellite OSM" };
        }


    }
}
