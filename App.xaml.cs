namespace Bakalarka_Jelinek
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            FirebaseService.Init();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}