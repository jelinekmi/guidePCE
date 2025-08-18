using Microsoft.Maui.Controls;
using System.Globalization;
using System.Linq;

namespace Bakalarka_Jelinek
{
    public partial class ObchodyPage : ContentPage
    {
        private readonly Db _db = new Db();
        private List<Mapa.Pamatka> _obchodySeznam;
        private List<Mapa.Pamatka> _filtrovaneObchody;

        public ObchodyPage()
        {
            InitializeComponent();
            _ = NacistObchody();
        }

        private async Task NacistObchody()
        {
            try
            {
                _obchodySeznam = await Mapa.ZiskejObchodyVPardubicichAsync();

                if (_obchodySeznam.Count == 0)
                {
                    await DisplayAlert("Info", "Žádné obchody nenalezeny.", "OK");
                    return;
                }

                var oblibene = await _db.GetFavoritesByTypeAsync(User.CurrentUser?.Email, "obchod");

                foreach (var o in _obchodySeznam)
                {
                    o.IsUserLoggedIn = User.CurrentUser != null;
                    o.IsFavorite = oblibene.Any(f => f.Nazev == o.Nazev && f.Lat == o.Lat && f.Lon == o.Lon);
                    o.FavoriteIcon = o.IsFavorite ? "★" : "☆";
                }

                _filtrovaneObchody = new List<Mapa.Pamatka>(_obchodySeznam);
                ObchodyList.ItemsSource = _filtrovaneObchody;

                // Naplnění tagového Pickeru
                var vsechnyTagy = _obchodySeznam
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
                await DisplayAlert("Chyba", $"Nepodařilo se načíst obchody: {ex.Message}", "OK");
            }
        }

        private async void OnFavoriteClicked(object sender, EventArgs e)
        {
            if (User.CurrentUser == null)
            {
                await DisplayAlert("Chyba", "Pro přidání do oblíbených se musíte přihlásit.", "OK");
                return;
            }

            var button = sender as Button;
            var obchod = button?.BindingContext as Mapa.Pamatka;

            if (obchod == null)
            {
                await DisplayAlert("Chyba", "Nepodařilo se získat informace o obchodě.", "OK");
                return;
            }

            if (obchod.IsFavorite)
            {
                await _db.RemoveFavoriteAsync(User.CurrentUser.Email, obchod.Nazev, obchod.Typ, obchod.Lat, obchod.Lon);
                obchod.IsFavorite = false;
                obchod.FavoriteIcon = "☆";
            }
            else
            {
                await _db.AddFavoriteAsync(User.CurrentUser.Email, obchod.Nazev, obchod.Typ, obchod.Lat, obchod.Lon);
                obchod.IsFavorite = true;
                obchod.FavoriteIcon = "★";
            }

            ObchodyList.ItemsSource = null;
            ObchodyList.ItemsSource = _filtrovaneObchody;
        }

        private async void OnNavigateClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var obchod = button?.BindingContext as Mapa.Pamatka;

            if (obchod == null)
                return;

            string lat = obchod.Lat.ToString(CultureInfo.InvariantCulture);
            string lon = obchod.Lon.ToString(CultureInfo.InvariantCulture);

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

        private void OnTagFilterChanged(object sender, EventArgs e)
        {
            string vybranyTag = TagFilter.SelectedItem as string;
            if (string.IsNullOrEmpty(vybranyTag) || vybranyTag == "Vše")
            {
                _filtrovaneObchody = new List<Mapa.Pamatka>(_obchodySeznam);
            }
            else
            {
                _filtrovaneObchody = _obchodySeznam.Where(p => p.Tagy != null && p.Tagy.Contains(vybranyTag)).ToList();
            }

            ObchodyList.ItemsSource = null;
            ObchodyList.ItemsSource = _filtrovaneObchody;
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
