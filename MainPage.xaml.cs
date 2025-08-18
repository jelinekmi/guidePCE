namespace Bakalarka_Jelinek
{
    public partial class MainPage : ContentPage
    {
        

        public MainPage()
        {

            FirebaseService.Init();
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(LoginPage)); 
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage)); 
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

}
