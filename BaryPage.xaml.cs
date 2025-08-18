using Microsoft.Maui.Controls;
using System.Globalization;
using System.Linq;

namespace Bakalarka_Jelinek
{
    public partial class BaryPage : ContentPage
    {
        private readonly Db _db = new Db();
        private List<Mapa.Pamatka> _barySeznam;
        private List<Mapa.Pamatka> _filtrovaneBary;

        public BaryPage()
        {
            FirebaseService.Init();
            InitializeComponent();
            _ = NacistBary();
        }

        private async Task NacistBary()
        {
            try
            {
                _barySeznam = await Mapa.ZiskejBaryVPardubicichAsync();

                if (_barySeznam.Count == 0)
                {
                    await DisplayAlert("Info", "Žádné bary nenalezeny.", "OK");
                    return;
                }

                var oblibene = await _db.GetFavoritesByTypeAsync(User.CurrentUser?.Email, "bar");

                foreach (var b in _barySeznam)
                {
                    b.IsUserLoggedIn = User.CurrentUser != null;
                    b.IsFavorite = oblibene.Any(f => f.Nazev == b.Nazev && f.Lat == b.Lat && f.Lon == b.Lon);
                    b.FavoriteIcon = b.IsFavorite ? "★" : "☆";
                }

                
                var vsechnyTagy = _barySeznam.SelectMany(p => p.Tagy ?? new List<string>())
                                             .Distinct()
                                             .OrderBy(t => t)
                                             .ToList();
                vsechnyTagy.Insert(0, "Vše");

                TagFilter.ItemsSource = vsechnyTagy;
                TagFilter.SelectedIndex = 0;

                _filtrovaneBary = new List<Mapa.Pamatka>(_barySeznam);
                BaryList.ItemsSource = _filtrovaneBary;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst bary: {ex.Message}", "OK");
            }
        }

        private async void OnFavoriteClicked(object sender, EventArgs e)
        {
            if (User.CurrentUser == null)
            {
                await DisplayAlert("Chyba", "Pro přidání do oblíbených se musíte přihlásit.", "OK");
                return;
            }

            if (sender is Button button && button.BindingContext is Mapa.Pamatka bar)
            {
                if (bar.IsFavorite)
                {
                    await _db.RemoveFavoriteAsync(User.CurrentUser.Email, bar.Nazev, bar.Typ, bar.Lat, bar.Lon);
                    bar.IsFavorite = false;
                    bar.FavoriteIcon = "☆";
                }
                else
                {
                    await _db.AddFavoriteAsync(User.CurrentUser.Email, bar.Nazev, bar.Typ, bar.Lat, bar.Lon);
                    bar.IsFavorite = true;
                    bar.FavoriteIcon = "★";
                }

                BaryList.ItemsSource = null;
                BaryList.ItemsSource = _filtrovaneBary;
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
                _filtrovaneBary = new List<Mapa.Pamatka>(_barySeznam);
            }
            else
            {
                _filtrovaneBary = _barySeznam.Where(p => p.Tagy != null && p.Tagy.Contains(vybranyTag)).ToList();
            }

            BaryList.ItemsSource = null;
            BaryList.ItemsSource = _filtrovaneBary;
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
