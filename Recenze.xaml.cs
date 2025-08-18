using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Bakalarka_Jelinek
{
    public partial class Recenze : ContentPage
    {
        private readonly Mapa.Pamatka _misto;
        private int rating = 0;
        private List<Label> starLabels = new();

        
        private string? _photoBase64;
        private const int MaxPhotoBytes = 800 * 1024; 

        public Recenze(Mapa.Pamatka misto)
        {
            InitializeComponent();
            _misto = misto;
            MistoLabel.Text = $"Přidat recenzi pro: {_misto.Nazev}";

            // hvězdičky 1..5
            for (int i = 1; i <= 5; i++)
            {
                int index = i;
                var star = new Label
                {
                    Text = "☆",
                    FontSize = 32,
                    TextColor = Colors.Gold,
                    HorizontalOptions = LayoutOptions.Center,
                    GestureRecognizers =
                    {
                        new TapGestureRecognizer
                        {
                            Command = new Command(() => OnStarTapped(index))
                        }
                    }
                };

                starLabels.Add(star);
                StarContainer.Children.Add(star);
            }

            
            if (PhotoPreview != null) PhotoPreview.IsVisible = false;
            if (RemovePhotoButton != null) RemovePhotoButton.IsVisible = false;
            if (PhotoInfo != null) PhotoInfo.IsVisible = false;
        }

        private void OnStarTapped(int index)
        {
            rating = index;
            UpdateStars();
            RatingValueLabel.Text = $"Hodnocení: {rating} / 5";
        }

        private void UpdateStars()
        {
            for (int i = 0; i < starLabels.Count; i++)
            {
                starLabels[i].Text = (i < rating) ? "★" : "☆";
            }
        }

        
        private async void OnPickPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Vyberte fotku (JPG/PNG)",
                    FileTypes = FilePickerFileType.Images
                });

                if (file == null) return;

                
                byte[] bytes;
                using (var s = await file.OpenReadAsync())
                using (var ms = new MemoryStream())
                {
                    await s.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }

                
                if (bytes.Length > MaxPhotoBytes)
                {
                    await DisplayAlert(
                        "Fotka je příliš velká",
                        $"Soubor má {(bytes.Length / 1024)} KB. Zmenšete ho prosím (cílit ~600–800 KB) a zkuste znovu.",
                        "OK"
                    );
                    return;
                }

                _photoBase64 = Convert.ToBase64String(bytes);

                
                if (PhotoPreview != null)
                {
                    PhotoPreview.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    PhotoPreview.IsVisible = true;
                }

                if (PhotoInfo != null)
                {
                    PhotoInfo.Text = $"Velikost: {(bytes.Length / 1024)} KB";
                    PhotoInfo.IsVisible = true;
                }

                if (RemovePhotoButton != null)
                    RemovePhotoButton.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst fotku: {ex.Message}", "OK");
            }
        }

        
        private void OnRemovePhotoClicked(object sender, EventArgs e)
        {
            _photoBase64 = null;

            if (PhotoPreview != null)
            {
                PhotoPreview.Source = null;
                PhotoPreview.IsVisible = false;
            }

            if (PhotoInfo != null)
            {
                PhotoInfo.Text = "";
                PhotoInfo.IsVisible = false;
            }

            if (RemovePhotoButton != null)
                RemovePhotoButton.IsVisible = false;
        }

        private async void OnOdeslatClicked(object sender, EventArgs e)
        {
            string reviewText = ReviewEntry.Text?.Trim();

            if (rating == 0)
            {
                await DisplayAlert("Chyba", "Zadejte počet hvězdiček.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(reviewText))
            {
                bool pokracovat = await DisplayAlert("Pozor", "Text recenze je prázdný. Odeslat pouze hodnocení?", "Ano", "Ne");
                if (!pokracovat)
                    return;
            }

            if (User.CurrentUser == null)
            {
                await DisplayAlert("Chyba", "Pro přidání recenze se musíte přihlásit.", "OK");
                return;
            }

            await FirebaseService.AddReviewAsync(
                _misto.Typ,
                _misto.Nazev,
                _misto.Lat,
                _misto.Lon,
                User.CurrentUser.Email,
                reviewText ?? "",
                rating,
                _photoBase64 
            );

            await DisplayAlert("Hotovo", "Recenze byla přidána.", "OK");
            await Navigation.PopAsync();
        }
    }
}
