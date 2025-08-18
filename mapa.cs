using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bakalarka_Jelinek
{
    public class Mapa
    {
        public class Pamatka
        {
            public string Nazev { get; set; }
            public string Typ { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public string Adresa { get; set; }
            public bool IsUserLoggedIn { get; set; }
            public bool IsFavorite { get; set; }
            public string FavoriteIcon { get; set; }
            public List<string> Tagy { get; set; } = new();
            public double PrumerneHodnoceni { get; set; }
        }


        private static bool _firebaseReady = false;
        private static void EnsureFirebaseSafe()
        {
            if (_firebaseReady) return;
            try
            {
                FirebaseService.Init();
                _firebaseReady = true;
                Debug.WriteLine("Firebase inicializace OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Firebase inicializace selhala: {ex.Message}");
            }
        }

        
        private const string OverpassQueryPamatky = @"
[out:json][timeout:25];
area[""name""=""Pardubice""][""boundary""=""administrative""][""admin_level""=""8""]->.searchArea;
(
  node[""name""][""historic""][""historic""!=""memorial""][""historic""!=""wayside_cross""][""historic""!=""monument""](area.searchArea);
  way[""name""][""historic""][""historic""!=""memorial""][""historic""!=""wayside_cross""][""historic""!=""monument""](area.searchArea);
  relation[""name""][""historic""][""historic""!=""memorial""][""historic""!=""wayside_cross""][""historic""!=""monument""](area.searchArea);
  node[""name""][""tourism""=""attraction""](area.searchArea);
  way[""name""][""tourism""=""attraction""](area.searchArea);
  relation[""name""][""tourism""=""attraction""](area.searchArea);
);
out center tags;
";

        public static async Task<List<Pamatka>> ZiskejPamatkyVPardubicichAsync()
        {
            var vysledek = new List<Pamatka>();
            try
            {
                string response = await FetchOverpassDataAsync(OverpassQueryPamatky);
                if (!string.IsNullOrEmpty(response))
                {
                    vysledek = await ParseOverpassData(response, "pamatka");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba při načítání památek: {ex.Message}");
            }
            return vysledek;
        }

        
        private static async Task<string> FetchOverpassDataAsync(string query)
        {
            try
            {
                string encodedQuery = Uri.EscapeDataString(query);
                using var client = new HttpClient();
                return await client.GetStringAsync($"https://overpass-api.de/api/interpreter?data={encodedQuery}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HTTP chyba při volání Overpass API: {ex.Message}");
                return null;
            }
        }

        
        private static async Task<List<Pamatka>> ParseOverpassData(string jsonData, string typ)
        {
            var vysledek = new List<Pamatka>();
            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonData);
                var elements = jsonDoc.RootElement.GetProperty("elements");

                foreach (var element in elements.EnumerateArray())
                {
                    string name = element.TryGetProperty("tags", out var tags) &&
                                  tags.TryGetProperty("name", out var nameProp)
                                  ? nameProp.GetString()
                                  : "(Bez názvu)";

                    double lat = element.TryGetProperty("lat", out var latProp)
                        ? latProp.GetDouble()
                        : element.GetProperty("center").GetProperty("lat").GetDouble();

                    double lon = element.TryGetProperty("lon", out var lonProp)
                        ? lonProp.GetDouble()
                        : element.GetProperty("center").GetProperty("lon").GetDouble();

                    string adresa = "";
                    if (tags.TryGetProperty("addr:street", out var ulice))
                        adresa += ulice.GetString();
                    if (tags.TryGetProperty("addr:housenumber", out var cislo))
                        adresa += " " + cislo.GetString();
                    if (tags.TryGetProperty("addr:city", out var mesto))
                        adresa += ", " + mesto.GetString();
                    if (string.IsNullOrWhiteSpace(adresa))
                        adresa = "(Adresa neuvedena)";

                    var tagyList = new List<string>();
                    if (element.TryGetProperty("tags", out var tagsElement))
                    {
                        string[] duleziteTagy = { "historic", "tourism", "building", "amenity", "shop", "leisure" };
                        foreach (var tag in tagsElement.EnumerateObject())
                        {
                            if (!tag.Name.StartsWith("addr:") && tag.Name != "name" && duleziteTagy.Contains(tag.Name))
                            {
                                string value = tag.Value.GetString();
                                if (!string.IsNullOrWhiteSpace(value))
                                    tagyList.Add(value.ToLowerInvariant());
                            }
                        }
                    }

                    double prumerneHodnoceni = 0;
                    EnsureFirebaseSafe();
                    if (_firebaseReady)
                    {
                        try
                        {
                            var reviews = await FirebaseService.GetReviewsForPlaceAsync(typ, name, Math.Round(lat, 5), Math.Round(lon, 5));
                            if (reviews.Count > 0)
                                prumerneHodnoceni = reviews.Average(r => r.Rating);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Načítání recenzí selhalo: {ex.Message}");
                        }
                    }

                    vysledek.Add(new Pamatka
                    {
                        Nazev = name,
                        Typ = typ,
                        Adresa = adresa,
                        Lat = lat,
                        Lon = lon,
                        Tagy = tagyList.Distinct().ToList(),
                        PrumerneHodnoceni = prumerneHodnoceni
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba při zpracování JSON: {ex.Message}");
            }
            return vysledek;
        }

        
        private const string OverpassQueryRestaurace = @"
[out:json][timeout:25];
area[""name""=""Pardubice""][""boundary""=""administrative""][""admin_level""=""8""]->.searchArea;
(
  node[""amenity""=""restaurant""](area.searchArea);
  way[""amenity""=""restaurant""](area.searchArea);
  relation[""amenity""=""restaurant""](area.searchArea);
);
out center tags;
";
        public static Task<List<Pamatka>> ZiskejRestauraceVPardubicichAsync()
            => ParseWithQuery(OverpassQueryRestaurace, "restaurace");

        
        private const string OverpassQueryObchody = @"
[out:json][timeout:25];
area[""name""=""Pardubice""][""boundary""=""administrative""][""admin_level""=""8""]->.searchArea;
(
  node[""shop""][""name""](area.searchArea);
  way[""shop""][""name""](area.searchArea);
  relation[""shop""][""name""](area.searchArea);
);
out center tags;
";
        public static Task<List<Pamatka>> ZiskejObchodyVPardubicichAsync()
            => ParseWithQuery(OverpassQueryObchody, "obchod");

        
        private const string OverpassQueryBary = @"
[out:json][timeout:25];
area[""name""=""Pardubice""][""boundary""=""administrative""][""admin_level""=""8""]->.searchArea;
(
  node[""amenity""=""bar""](area.searchArea);
  way[""amenity""=""bar""](area.searchArea);
  relation[""amenity""=""bar""](area.searchArea);
);
out center tags;
";
        public static Task<List<Pamatka>> ZiskejBaryVPardubicichAsync()
            => ParseWithQuery(OverpassQueryBary, "bar");

        
        private static async Task<List<Pamatka>> ParseWithQuery(string query, string typ)
        {
            var vysledek = new List<Pamatka>();
            try
            {
                string response = await FetchOverpassDataAsync(query);
                if (!string.IsNullOrEmpty(response))
                {
                    vysledek = await ParseOverpassData(response, typ);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Chyba při načítání {typ}: {ex.Message}");
            }
            return vysledek;
        }
    }
}
