namespace PawRescue.Mobile;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;
        UserAppTheme = AppTheme.Light; // the Figma design is light-only
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(_shell);
}
