namespace Practica8_NL15_47
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("SegundaPagina", typeof(SegundaPagina));
        }

    }
}
