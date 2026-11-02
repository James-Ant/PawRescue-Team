using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class RegisterPage : BasePage
{
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
