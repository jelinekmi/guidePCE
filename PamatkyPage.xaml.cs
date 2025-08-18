using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Bakalarka_Jelinek
{
    public partial class PamatkyPage : ContentPage
    {
        private readonly Db _db = new Db();
        private List<Mapa.Pamatka> _pamatkySeznam;
        private List<Mapa.Pamatka> _filtrovanePamatky;

        public PamatkyPage()
        {
            InitializeComponent();
            _ = NacistPamatky();
        }

        private async Task NacistPamatky()
        {
            try
            {
                _pamatkySeznam = await Mapa.ZiskejPamatkyVPardubicichAsync();

                if (_pamatkySeznam.Count == 0)
                {
                    await DisplayAlert("Info", "Žádné památky nenalezeny.", "OK");
                    return;
                }

                var oblibene = await _db.GetFavoritesByTypeAsync(User.CurrentUser?.Email, "pamatka");

                foreach (var p in _pamatkySeznam)
                {
                    p.IsUserLoggedIn = User.CurrentUser != null;
                    p.IsFavorite = oblibene.Any(f => f.Nazev == p.Nazev && f.Lat == p.Lat && f.Lon == p.Lon);
                    p.FavoriteIcon = p.IsFavorite ? "★" : "☆";
                }

                _filtrovanePamatky = new List<Mapa.Pamatka>(_pamatkySeznam);
                PamatkyList.ItemsSource = _filtrovanePamatky;

                var vsechnyTagy = _pamatkySeznam
                                    .SelectMany(p => p.Tagy)
                                    .Where(tag => tag != "yes") 
                                    .Distinct()
                                    .OrderBy(t => t)
                                    .ToList();

                vsechnyTagy.Insert(0, "Vše");
                TagFilter.ItemsSource = vsechnyTagy;
                TagFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst památky: {ex.Message}", "OK");
            }
        }

        private async void OnFavoriteClicked(object sender, EventArgs e)
        {
            if (User.CurrentUser == null)
            {
                await DisplayAlert("Chyba", "Pro přidání do oblíbených se musíte přihlásit.", "OK");
                return;
            }

            if (sender is Button button && button.BindingContext is Mapa.Pamatka pamatka)
            {
                if (pamatka.IsFavorite)
                {
                    await _db.RemoveFavoriteAsync(User.CurrentUser.Email, pamatka.Nazev, "pamatka", pamatka.Lat, pamatka.Lon);
                    pamatka.IsFavorite = false;
                    pamatka.FavoriteIcon = "☆";
                }
                else
                {
                    await _db.AddFavoriteAsync(User.CurrentUser.Email, pamatka.Nazev, "pamatka", pamatka.Lat, pamatka.Lon);
                    pamatka.IsFavorite = true;
                    pamatka.FavoriteIcon = "★";
                }

                PamatkyList.ItemsSource = null;
                PamatkyList.ItemsSource = _filtrovanePamatky;
            }
        }

        private async void OnNavigateClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Mapa.Pamatka bar)
            {
                string lat = bar.Lat.ToString(CultureInfo.InvariantCulture);
                string lon = bar.Lon.ToString(CultureInfo.InvariantCulture);

                string url = $"https://www.google.com/maps/dir/?api=1&destination={lat},{lon}";

                try
                {
                    await Launcher.Default.OpenAsync(url);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Chyba", $"Nelze otevřít mapu: {ex.Message}", "OK");
                }
            }
        }

        private void OnTagFilterChanged(object sender, EventArgs e)
        {
            string vybranyTag = TagFilter.SelectedItem as string;
            if (string.IsNullOrEmpty(vybranyTag) || vybranyTag == "Vše")
            {
                _filtrovanePamatky = new List<Mapa.Pamatka>(_pamatkySeznam);
            }
            else
            {
                _filtrovanePamatky = _pamatkySeznam.Where(p => p.Tagy != null && p.Tagy.Contains(vybranyTag)).ToList();
            }

            PamatkyList.ItemsSource = null;
            PamatkyList.ItemsSource = _filtrovanePamatky;
        }

        private async void OnRecenzeClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Mapa.Pamatka misto)
            {
                await Navigation.PushAsync(new RecenzeListPage(misto));
            }
        }


    }
}
