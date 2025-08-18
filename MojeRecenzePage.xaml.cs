using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bakalarka_Jelinek
{
    public partial class MojeRecenzePage : ContentPage
    {
        private List<Review> mojeRecenze;

        public MojeRecenzePage()
        {
            InitializeComponent();
            FirebaseService.Init();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var email = User.CurrentUser?.Email;
            if (string.IsNullOrEmpty(email))
                return;

            mojeRecenze = await FirebaseService.GetReviewsByUserAsync(email);
            ReviewListView.ItemsSource = mojeRecenze;
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var review = (Review)button.BindingContext;

            bool confirm = await DisplayAlert("Smazat recenzi", "Opravdu chcete tuto recenzi odstranit?", "Ano", "Ne");
            if (!confirm) return;

            await FirebaseService.DeleteReviewAsync(review.Id);
            mojeRecenze.Remove(review);

            // Refresh CollectionView
            ReviewListView.ItemsSource = null;
            ReviewListView.ItemsSource = mojeRecenze;
        }
    }
}
