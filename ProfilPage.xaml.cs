namespace Bakalarka_Jelinek;

public partial class ProfilPage : ContentPage
{
    private bool isPasswordVisible = false;

    public ProfilPage()
    {
        InitializeComponent();
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (User.CurrentUser != null)
        {
            NameEntry.Text = User.CurrentUser.Name;
            EmailEntry.Text = User.CurrentUser.Email;
            PasswordEntry.Text = "********"; 

            
            if (!string.IsNullOrEmpty(User.CurrentUser.ProfilePhotoPath))
            {
                ProfileImage.Source = ImageSource.FromFile(User.CurrentUser.ProfilePhotoPath);
            }
        }
        else
        {
            DisplayAlert("Chyba", "Nepodaøilo se naèíst údaje uživatele.", "OK");
        }
    }



    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        string newPassword = await DisplayPromptAsync("Zmìna hesla", "Zadejte nové heslo:", "OK", "Zrušit", "", -1, Keyboard.Text);

        if (string.IsNullOrEmpty(newPassword))
        {
            await DisplayAlert("Chyba", "Heslo nemùže být prázdné!", "OK");
            return;
        }

        string confirmPassword = await DisplayPromptAsync("Potvrzení hesla", "Zadejte nové heslo znovu:", "OK", "Zrušit", "", -1, Keyboard.Text);

        if (string.IsNullOrEmpty(confirmPassword))
        {
            await DisplayAlert("Chyba", "Potvrzení hesla nemùže být prázdné!", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Chyba", "Hesla se neshodují! Zkuste to znovu.", "OK");
            return;
        }

        
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

        
        User.CurrentUser.Password = hashedPassword;
        await new Db().UpdateUserPasswordAsync(User.CurrentUser.Email, hashedPassword);

        await DisplayAlert("Hotovo", "Heslo bylo úspìšnì zmìnìno!", "OK");
    }

    private async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Vyberte profilovou fotku"
            });

            if (photo != null)
            {
                
                User.CurrentUser.ProfilePhotoPath = photo.FullPath;
                await new Db().UpdateUserPhotoAsync(User.CurrentUser.Email, photo.FullPath);

                
                ProfileImage.Source = ImageSource.FromFile(photo.FullPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", "Nepodaøilo se naèíst fotku: " + ex.Message, "OK");
        }
    }

    private async void OnFavoritesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(FavoritesPage));
    }

    private async void OnReviewsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MojeRecenzePage));
    }
   
}