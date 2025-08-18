namespace Bakalarka_Jelinek
{
    public partial class LoginPage : ContentPage
    {
        private Db _db;

        public LoginPage()
        {
            InitializeComponent();
            _db = new Db();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Chyba", "Vyplòte všechny údaje!", "OK");
                return;
            }

            
            User user = await _db.AuthenticateUserAsync(email, password);

            if (user != null)
            {
                User.CurrentUser = user; 
                await DisplayAlert("Úspìch", $"Vítej zpìt, {user.Name}!", "OK");

                
                await Shell.Current.GoToAsync("//AfterRegLogPage");
            }
            else
            {
                await DisplayAlert("Chyba", "Neplatné pøihlašovací údaje.", "OK");
            }
        }
    }
}
