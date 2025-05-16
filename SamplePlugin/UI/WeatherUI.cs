using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace SamplePlugin.HUD
{
    internal class WeatherUI : Window
    {
        private Dictionary<string, List<string>> regions;
        private string? selectedRegion;
        private string? selectedZone;

        public WeatherUI(Dictionary<string, List<string>> regionData)
            : base("Weather Menu", ImGuiWindowFlags.AlwaysAutoResize)
        {
            regions = regionData;
        }

        public override void Draw()
        {
            DrawRegionSelector();
        }
        public TerritoryType? GetTerritoryForZone(string zoneName)
        {
            var territorySheet = Plugin.DataManager.GetExcelSheet<TerritoryType>();
            if (territorySheet == null)
                return null;

            foreach (var territory in territorySheet)
            {
                var name = territory.PlaceName.Value.Name.ToString();
                if (name == zoneName)
                    return territory;
            }

            return null;
        }

        private void DrawRegionSelector()
        {
            if (ImGui.BeginCombo("Region", selectedRegion ?? "Select a region"))
            {
                foreach (var region in regions.Keys)
                {
                    if (ImGui.Selectable(region, selectedRegion == region))
                    {
                        selectedRegion = region;
                        selectedZone = null; // reset selected zone
                    }
                }
                ImGui.EndCombo();
            }

            if (!string.IsNullOrEmpty(selectedRegion) && regions.TryGetValue(selectedRegion, out var zones))
            {
                if (ImGui.BeginCombo("Zone", selectedZone ?? "Select a zone"))
                {
                    foreach (var zone in zones)
                    {
                        if (ImGui.Selectable(zone, selectedZone == zone))
                        {
                            selectedZone = zone;
                        }
                    }
                    ImGui.EndCombo();
                }
            }

            if (!string.IsNullOrEmpty(selectedZone))
            {
                ImGui.Text($"You picked: {selectedRegion} > {selectedZone}");

                var territory = GetTerritoryForZone(selectedZone);
                if (territory != null)
                {
                    var weather = Plugin.WeatherManager.GetCurrentWeather(territory.Value.RowId);
                    var weatherName = weather?.CurrentWeather?.Value?.Name?.ToString();

                    if (!string.IsNullOrEmpty(weatherName))
                    {
                        ImGui.Text($"🌤️ Current Weather: {weatherName}");
                    }
                    else
                    {
                        ImGui.Text("⚠️ Could not fetch weather.");
                    }
                }
                else
                {
                    ImGui.Text("⚠️ Could not find territory for zone.");
                }
            }
        }
    }
}
