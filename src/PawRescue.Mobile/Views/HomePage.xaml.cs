using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class HomePage : BasePage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
