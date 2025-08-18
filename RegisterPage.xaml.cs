using System;
using BCrypt.Net;

namespace Bakalarka_Jelinek
{
    public partial class RegisterPage : ContentPage
    {
        private Db _db;

        public RegisterPage()
        {
            InitializeComponent();
            _db = new Db(); 

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Chyba", "Vyplòte všechny údaje!", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Chyba", "Hesla se neshodují!", "OK");
                return;
            }

            
            if (await _db.UserExistsByEmailAsync(email))
            {
                await DisplayAlert("Chyba", "Uživatel s tímto e-mailem už existuje.", "OK");
                return;
            }

            
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            
            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                Role = "user" 
            };

            try
            {
                await _db.AddUserAsync(newUser);
                await DisplayAlert("Úspìch", "Registrace probìhla úspìšnì!", "OK");
                await Shell.Current.GoToAsync(".."); 
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Nastala chyba pøi registraci: {ex.Message}", "OK");
            }
        }
    }
}
