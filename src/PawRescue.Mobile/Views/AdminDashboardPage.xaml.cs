using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class AdminDashboardPage : BasePage
{
    public AdminDashboardPage(AdminDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
