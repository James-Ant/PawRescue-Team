using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PawRescue.Mobile.Models;
using PawRescue.Mobile.Services;

namespace PawRescue.Mobile.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private const double HomeRadiusKm = 10;

    private readonly IReportApi _reports;
    private readonly IUserApi _users;
    private readonly INotificationApi _notifications;
    private readonly ILocationService _location;
    private readonly TokenStore _tokens;

    public HomeViewModel(IReportApi reports, IUserApi users, INotificationApi notifications,
                         ILocationService location, TokenStore tokens, IDialogService dialogs) : base(dialogs)
    {
        _reports = reports;
        _users = users;
        _notifications = notifications;
        _location = location;
        _tokens = tokens;
    }

    public ObservableCollection<ReportSummaryDto> NearbyReports { get; } = new();

    [ObservableProperty] private string greeting = string.Empty;
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private bool isVolunteer;
    [ObservableProperty] private bool isAdmin;
    [ObservableProperty] private int unreadCount;
    [ObservableProperty] private int urgentCount;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool hasNoReports;
    [ObservableProperty] private string locationText = "Locating you…";
    [ObservableProperty] private ReportSummaryDto? selectedReport;

    public override async Task OnAppearingAsync()
    {
        UserName = _tokens.CurrentUser?.FullName.Split(' ').FirstOrDefault() ?? "friend";
        IsVolunteer = _tokens.IsVolunteer;
        IsAdmin = _tokens.IsAdmin;
        Greeting = DateTime.Now.Hour switch { < 12 => "Good morning", < 18 => "Good afternoon", _ => "Good evening" };
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await RunAsync(LoadCoreAsync, showBusy: !IsRefreshing);
        IsRefreshing = false;
    }

    private async Task LoadCoreAsync()
    {
        var here = await _location.GetCurrentAsync();
        if (here is null)
        {
            LocationText = "Location is off — showing latest reports";
            var latest = await _reports.SearchAsync(null);
            Replace(latest.Items.Where(r => r.Status is not (ReportStatus.Rescued or ReportStatus.Closed)).Take(20));
        }
        else
        {
            LocationText = await _location.GetAddressAsync(here.Latitude, here.Longitude) ?? "Near you";
            Replace(await _reports.GetNearbyAsync(here.Latitude, here.Longitude, HomeRadiusKm));

            // Lets the server send "new nearby report" alerts to volunteers.
            if (IsVolunteer) await _users.UpdateLocationAsync(here.Latitude, here.Longitude);
        }

        UnreadCount = await _notifications.GetUnreadCountAsync();
    }

    private void Replace(IEnumerable<ReportSummaryDto> items)
    {
        NearbyReports.Clear();
        foreach (var r in items) NearbyReports.Add(r);
        UrgentCount = NearbyReports.Count(r => r.Severity >= SeverityLevel.High);
        HasNoReports = NearbyReports.Count == 0;
    }

    // Selecting a card opens its details, then clears the selection so it can be tapped again.
    partial void OnSelectedReportChanged(ReportSummaryDto? value)
    {
        if (value is null) return;
        SelectedReport = null;
        _ = Shell.Current.GoToAsync("reportdetails", new Dictionary<string, object> { ["id"] = value.Id });
    }

    [RelayCommand] private Task ReportAnimalAsync() => Shell.Current.GoToAsync("//report");
    [RelayCommand] private Task OpenMapAsync() => Shell.Current.GoToAsync("//map");
    [RelayCommand] private Task OpenNotificationsAsync() => Shell.Current.GoToAsync("//notifications");
    [RelayCommand] private Task OpenVolunteerDashboardAsync() => Shell.Current.GoToAsync("volunteer");
    [RelayCommand] private Task OpenAdminDashboardAsync() => Shell.Current.GoToAsync("admin");
}
