

namespace Bakalarka_Jelinek;

public partial class AfterRegLogPage : ContentPage
{
    public AfterRegLogPage()
    {
        InitializeComponent();
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        
        await Shell.Current.GoToAsync(nameof(ProfilPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        User.IsLoggedIn = false;
        await DisplayAlert("Úspìch", $"Byli jste odhlášeni, ", "OK");
        await Shell.Current.GoToAsync("//MainPage");
        
    }

    private async void OnPamatkyClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PamatkyPage));

    }
    private async void OnObchodyClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ObchodyPage));

    }
    private async void OnBaryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(BaryPage));

    }
    private async void OnRestauraceClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RestauracePage));

    }
}


