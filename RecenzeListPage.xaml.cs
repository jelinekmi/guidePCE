using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bakalarka_Jelinek
{
    public partial class RecenzeListPage : ContentPage
    {
        private readonly Mapa.Pamatka _misto;

        public RecenzeListPage(Mapa.Pamatka misto)
        {
            InitializeComponent();
            _misto = misto;
            HeaderLabel.Text = $"Recenze: {_misto.Nazev}";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadReviewsAsync();
        }

        private async Task LoadReviewsAsync()
        {
            try
            {
                var rlat = Math.Round(_misto.Lat, 5);
                var rlon = Math.Round(_misto.Lon, 5);

                var raw = await FirebaseService.GetReviewsForPlaceAsync(_misto.Typ, _misto.Nazev, rlat, rlon);

                
                var vms = raw
                    .OrderByDescending(r => r.Timestamp?.ToDateTime())
                    .Select(r => new ReviewItemVM(r))
                    .ToList();

                ReviewsList.ItemsSource = vms;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst recenze: {ex.Message}", "OK");
            }
        }

        private async void OnAddReviewClicked(object sender, EventArgs e)
        {
            
            await Navigation.PushAsync(new Recenze(_misto));
        }

       
        private sealed class ReviewItemVM
        {
            public string Id { get; }
            public string Email { get; }
            public string ReviewText { get; }
            public double Rating { get; }
            public DateTime? Date { get; }
            public string DateFormatted => Date?.ToString("dd.MM.yyyy HH:mm") ?? "";
            public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoBase64);
            public string PhotoBase64 { get; }
            public ImageSource PhotoSource =>
                HasPhoto ? ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(PhotoBase64))) : null;
            public string Stars => BuildStars(Rating);

            public ReviewItemVM(Review r)
            {
                Id = r.Id;
                Email = r.Email;
                ReviewText = r.ReviewText;
                Rating = r.Rating;
                Date = r.Timestamp?.ToDateTime();
                PhotoBase64 = r.PhotoBase64;
            }

            private static string BuildStars(double rating)
            {
                
                int full = (int)Math.Round(rating);
                full = Math.Max(0, Math.Min(5, full));
                return new string('★', full) + new string('☆', 5 - full);
            }
        }
    }
}
