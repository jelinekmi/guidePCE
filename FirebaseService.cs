using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bakalarka_Jelinek
{
    public static class FirebaseService
    {
        private static FirestoreDb firestoreDb;

        public static void Init()
        {
            
            var resourceName = "Bakalarka_Jelinek.Resources.recenze-80ebf-firebase-adminsdk-fbsvc-fa4de2f7f2.json";
            var assembly = typeof(FirebaseService).Assembly;

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException($"Nelze načíst {resourceName} jako embedded resource.");

            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            
            string tempPath = Path.Combine(FileSystem.AppDataDirectory, "firebase-key.json");
            File.WriteAllText(tempPath, json);

            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);

           
            firestoreDb = FirestoreDb.Create("recenze-80ebf");
        }
        private static (double lat, double lon) RoundCoord(double lat, double lon, int digits = 5)
    => (Math.Round(lat, digits), Math.Round(lon, digits));


        public static async Task AddReviewAsync(string typ, string nazev, double lat, double lon, string userEmail, string reviewText, int rating, string? photoBase64 = null)
        {
            var (rlat, rlon) = RoundCoord(lat, lon);
            DocumentReference docRef = firestoreDb.Collection("reviews").Document(); 
            var data = new Dictionary<string, object>
            {
                { "typ", typ },
                { "nazev", nazev },
                { "lat", lat },
                { "lon", lon },
                { "email", userEmail },
                { "review", reviewText },
                { "rating", rating },
                { "timestamp", Timestamp.GetCurrentTimestamp() }
            };

            if (!string.IsNullOrWhiteSpace(photoBase64))
                data["photoBase64"] = photoBase64;

            await docRef.SetAsync(data);
        }

        public static async Task<List<Review>> GetReviewsForPlaceAsync(string typ, string nazev, double lat, double lon)
        {
            var reviews = new List<Review>();
           

            Query query = firestoreDb.Collection("reviews")
                .WhereEqualTo("typ", typ)
                .WhereEqualTo("nazev", nazev);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                
                var r = new Review
                {
                    Id = doc.Id,
                    Email = doc.ContainsField("email") ? doc.GetValue<string>("email") : "",
                    ReviewText = doc.ContainsField("review") ? doc.GetValue<string>("review") : "",
                    Rating = doc.ContainsField("rating") ? doc.GetValue<double>("rating") : 0,
                    Lat = doc.ContainsField("lat") ? doc.GetValue<double>("lat") : 0,
                    Lon = doc.ContainsField("lon") ? doc.GetValue<double>("lon") : 0,
                    Nazev = doc.ContainsField("nazev") ? doc.GetValue<string>("nazev") : "",
                    Typ = doc.ContainsField("typ") ? doc.GetValue<string>("typ") : "",
                    Timestamp = doc.ContainsField("timestamp")
                        ? doc.GetValue<Timestamp>("timestamp")
                        : Timestamp.FromDateTime(DateTime.UtcNow), 
                        PhotoBase64 = doc.ContainsField("photoBase64") ? doc.GetValue<string>("photoBase64") : null

                };

                
                if (Math.Abs(r.Lat - lat) < 0.001 && Math.Abs(r.Lon - lon) < 0.001)
                {
                    reviews.Add(r); 
                }
            }

            return reviews;
        }

        public static async Task<List<Review>> GetReviewsByUserAsync(string email)
        {
            var reviews = new List<Review>();

            Query query = firestoreDb.Collection("reviews")
                .WhereEqualTo("email", email);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var review = new Review
                {
                    Id = doc.Id,
                    Email = doc.ContainsField("email") ? doc.GetValue<string>("email") : "",
                    ReviewText = doc.ContainsField("review") ? doc.GetValue<string>("review") : "",
                    Rating = doc.ContainsField("rating") ? doc.GetValue<double>("rating") : 0,
                    Timestamp = doc.ContainsField("timestamp") ? doc.GetValue<Timestamp>("timestamp") : null,
                    Lat = doc.ContainsField("lat") ? doc.GetValue<double>("lat") : 0,
                    Lon = doc.ContainsField("lon") ? doc.GetValue<double>("lon") : 0,
                    Nazev = doc.ContainsField("nazev") ? doc.GetValue<string>("nazev") : "",
                    Typ = doc.ContainsField("typ") ? doc.GetValue<string>("typ") : "",
                    PhotoBase64 = doc.ContainsField("photoBase64") ? doc.GetValue<string>("photoBase64") : null

                };

                reviews.Add(review);
            }

            return reviews;
        }

        public static async Task DeleteReviewAsync(string reviewId)
        {
            var db = firestoreDb;
            await db.Collection("reviews").Document(reviewId).DeleteAsync();
        }



      
    }
}
