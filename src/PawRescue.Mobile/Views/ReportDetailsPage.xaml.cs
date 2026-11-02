using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class ReportDetailsPage : BasePage
{
    public ReportDetailsPage(ReportDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
