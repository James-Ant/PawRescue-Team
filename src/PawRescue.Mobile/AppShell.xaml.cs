using PawRescue.Mobile.Views;

namespace PawRescue.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages pushed on top of the tabs (have a back button)
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("forgot", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("reportdetails", typeof(ReportDetailsPage));
        Routing.RegisterRoute("volunteer", typeof(VolunteerDashboardPage));
        Routing.RegisterRoute("admin", typeof(AdminDashboardPage));
    }
}
