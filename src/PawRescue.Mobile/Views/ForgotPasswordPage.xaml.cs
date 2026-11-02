using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class ForgotPasswordPage : BasePage
{
    public ForgotPasswordPage(ForgotPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
