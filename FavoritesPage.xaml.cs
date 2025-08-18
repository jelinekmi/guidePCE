using System.Globalization;

namespace Bakalarka_Jelinek;

public partial class FavoritesPage : ContentPage
{
    private readonly Db _db = new Db();
    private List<Mapa.Pamatka> _vsechnyOblibene = new();
    private List<Mapa.Pamatka> _filtrovane = new();

    public FavoritesPage()
    {
        InitializeComponent();
        _ = LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        if (User.CurrentUser == null)
        {
            await DisplayAlert("Chyba", "Musíte se přihlásit pro zobrazení oblíbených položek.", "OK");
            return;
        }

        var pamatky = await _db.GetFavoritesByTypeAsync(User.CurrentUser.Email, "pamatka");
        var restaurace = await _db.GetFavoritesByTypeAsync(User.CurrentUser.Email, "restaurace");
        var obchody = await _db.GetFavoritesByTypeAsync(User.CurrentUser.Email, "obchod");
        var bary = await _db.GetFavoritesByTypeAsync(User.CurrentUser.Email, "bar");

        foreach (var p in pamatky) p.Typ = "pamatka";
        foreach (var r in restaurace) r.Typ = "restaurace";
        foreach (var o in obchody) o.Typ = "obchod";
        foreach (var b in bary) b.Typ = "bar";

        _vsechnyOblibene = pamatky.Concat(restaurace).Concat(obchody).Concat(bary).ToList();

        foreach (var p in _vsechnyOblibene)
        {
            p.IsUserLoggedIn = true;
            p.IsFavorite = true;
            p.FavoriteIcon = "★";
        }

        _filtrovane = new List<Mapa.Pamatka>(_vsechnyOblibene);
        FavoritesList.ItemsSource = _filtrovane;
    }

    private async void OnFavoriteClicked(object sender, EventArgs e)
    {
        if (User.CurrentUser == null) return;

        var button = sender as Button;
        var item = button?.BindingContext as Mapa.Pamatka;

        if (item == null) return;

        bool confirm = await DisplayAlert("Odebrat", $"Opravdu chcete odebrat „{item.Nazev}“ z oblíbených?", "Ano", "Ne");
        if (!confirm) return;

        await _db.RemoveFavoriteAsync(User.CurrentUser.Email, item.Nazev, item.Typ, item.Lat, item.Lon);

        _vsechnyOblibene.Remove(item);
        _filtrovane.Remove(item);

        FavoritesList.ItemsSource = null;
        FavoritesList.ItemsSource = _filtrovane;
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var item = button?.BindingContext as Mapa.Pamatka;

        if (item == null) return;

        string lat = item.Lat.ToString(CultureInfo.InvariantCulture);
        string lon = item.Lon.ToString(CultureInfo.InvariantCulture);
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

    private void OnTypFilterChanged(object sender, EventArgs e)
    {
        var selected = TypFilter.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(selected) || selected == "Vše")
        {
            _filtrovane = new List<Mapa.Pamatka>(_vsechnyOblibene);
        }
        else
        {
            _filtrovane = _vsechnyOblibene.Where(p => p.Typ == selected).ToList();
        }

        FavoritesList.ItemsSource = null;
        FavoritesList.ItemsSource = _filtrovane;
    }
}
