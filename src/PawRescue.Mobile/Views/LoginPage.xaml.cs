using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class LoginPage : BasePage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
