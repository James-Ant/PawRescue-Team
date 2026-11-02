using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class ProfilePage : BasePage
{
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
