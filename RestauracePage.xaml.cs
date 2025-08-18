using Microsoft.Maui.Controls;
using System.Globalization;
using System.Linq;

namespace Bakalarka_Jelinek
{
    public partial class RestauracePage : ContentPage
    {
        private readonly Db _db = new Db();
        private List<Mapa.Pamatka> _restauraceSeznam;
        private List<Mapa.Pamatka> _filtrovaneRestaurace;

        public RestauracePage()
        {
            InitializeComponent();
            _ = NacistRestaurace();
        }

        private async Task NacistRestaurace()
        {
            try
            {
                _restauraceSeznam = await Mapa.ZiskejRestauraceVPardubicichAsync();

                if (_restauraceSeznam.Count == 0)
                {
                    await DisplayAlert("Info", "Žádné restaurace nenalezeny.", "OK");
                    return;
                }

                var oblibene = await _db.GetFavoritesByTypeAsync(User.CurrentUser?.Email, "restaurace");

                foreach (var r in _restauraceSeznam)
                {
                    r.IsUserLoggedIn = User.CurrentUser != null;
                    r.IsFavorite = oblibene.Any(f => f.Nazev == r.Nazev && f.Lat == r.Lat && f.Lon == r.Lon);
                    r.FavoriteIcon = r.IsFavorite ? "★" : "☆";
                }

                _filtrovaneRestaurace = new List<Mapa.Pamatka>(_restauraceSeznam);
                RestauraceList.ItemsSource = _filtrovaneRestaurace;

                var vsechnyTagy = _restauraceSeznam.SelectMany(p => p.Tagy ?? new List<string>()).Distinct().OrderBy(t => t).ToList();
                vsechnyTagy.Insert(0, "Vše");
                TagFilter.ItemsSource = vsechnyTagy;
                TagFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nepodařilo se načíst restaurace: {ex.Message}", "OK");
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
            var restaurace = button?.BindingContext as Mapa.Pamatka;

            if (restaurace == null)
            {
                await DisplayAlert("Chyba", "Nepodařilo se získat informace o restauraci.", "OK");
                return;
            }

            if (restaurace.IsFavorite)
            {
                await _db.RemoveFavoriteAsync(User.CurrentUser.Email, restaurace.Nazev, restaurace.Typ, restaurace.Lat, restaurace.Lon);
                restaurace.IsFavorite = false;
                restaurace.FavoriteIcon = "☆";
            }
            else
            {
                await _db.AddFavoriteAsync(User.CurrentUser.Email, restaurace.Nazev, restaurace.Typ, restaurace.Lat, restaurace.Lon);
                restaurace.IsFavorite = true;
                restaurace.FavoriteIcon = "★";
            }

            RestauraceList.ItemsSource = null;
            RestauraceList.ItemsSource = _filtrovaneRestaurace;
        }

        private async void OnNavigateClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var bar = button?.BindingContext as Mapa.Pamatka;

            if (bar == null)
                return;

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

        private void OnTagFilterChanged(object sender, EventArgs e)
        {
            string vybranyTag = TagFilter.SelectedItem as string;
            if (string.IsNullOrEmpty(vybranyTag) || vybranyTag == "Vše")
            {
                _filtrovaneRestaurace = new List<Mapa.Pamatka>(_restauraceSeznam);
            }
            else
            {
                _filtrovaneRestaurace = _restauraceSeznam.Where(p => p.Tagy != null && p.Tagy.Contains(vybranyTag)).ToList();
            }

            RestauraceList.ItemsSource = null;
            RestauraceList.ItemsSource = _filtrovaneRestaurace;
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
