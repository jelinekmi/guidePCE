using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bakalarka_Jelinek
{
    [FirestoreData]
    public class Review
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string ReviewText { get; set; }

        [FirestoreProperty]
        public double Rating { get; set; }

        [FirestoreProperty]
        public double Lat { get; set; }

        [FirestoreProperty]
        public double Lon { get; set; }

        [FirestoreProperty]
        public string Nazev { get; set; }

        [FirestoreProperty]
        public string Typ { get; set; }

        [FirestoreProperty]
        public Timestamp? Timestamp { get; set; }

        [FirestoreProperty]
        public string Datum => Timestamp?.ToDateTime().ToString("dd.MM.yyyy HH:mm") ?? "";

        public string? PhotoBase64 { get; set; }

        public Review() { }
    }
}