using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class NotificationsPage : BasePage
{
    public NotificationsPage(NotificationsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
