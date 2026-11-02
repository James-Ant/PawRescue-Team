using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

public partial class ReportAnimalPage : BasePage
{
    public ReportAnimalPage(ReportAnimalViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
