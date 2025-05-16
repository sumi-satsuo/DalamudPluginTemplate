using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugin.Data
{
    internal class RegionLoader
    {
        public Dictionary<string, List<string>> RegionToZones { get; private set; } = new();

        public Dictionary<string, List<string>> BuildRegionToZoneMap()
        {
            var territorySheet = Plugin.DataManager.GetExcelSheet<TerritoryType>();
            if (territorySheet == null)
                return [];

            foreach (var territory in territorySheet)
            {
                // These values are nullable, so we need to check them
                var region = territory.PlaceNameRegion.Value.Name.ToString();
                var zone = territory.PlaceName.Value.Name.ToString();

                if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(zone))
                    continue;

                if (!RegionToZones.ContainsKey(region))
                    RegionToZones[region] = new List<string>();

                if (!RegionToZones[region].Contains(zone))
                    RegionToZones[region].Add(zone);
            }

            // Optional: Sort zones alphabetically per region
            foreach (var list in RegionToZones.Values)
                list.Sort();

            return RegionToZones;
        }

    }
}
