namespace Bazar;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(MenuDatos), typeof(MenuDatos));
        Routing.RegisterRoute(nameof(MenuEmpeños), typeof(MenuEmpeños));
	}
}
