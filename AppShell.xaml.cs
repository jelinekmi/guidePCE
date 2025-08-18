namespace Bakalarka_Jelinek
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ProfilPage), typeof(ProfilPage));
            Routing.RegisterRoute(nameof(PamatkyPage), typeof(PamatkyPage));
            Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
            Routing.RegisterRoute(nameof(ObchodyPage), typeof(ObchodyPage));
            Routing.RegisterRoute(nameof(BaryPage), typeof(BaryPage));
            Routing.RegisterRoute(nameof(RestauracePage), typeof(RestauracePage));
            Routing.RegisterRoute(nameof(Recenze), typeof(Recenze));
            Routing.RegisterRoute(nameof(MojeRecenzePage), typeof(MojeRecenzePage));
        }
    }
    
}
