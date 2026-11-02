using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class VolunteerDashboardPage : BasePage
{
    public VolunteerDashboardPage(VolunteerDashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
