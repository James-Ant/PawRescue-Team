using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PawRescue.Mobile.Models;
using PawRescue.Mobile.Services;

namespace PawRescue.Mobile.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private readonly IReportApi _reports;
    private readonly ILocationService _location;
    private List<ReportSummaryDto> _loaded = new();
    private CancellationTokenSource? _debounce;

    public MapViewModel(IReportApi reports, ILocationService location, IDialogService dialogs) : base(dialogs)
    {
        _reports = reports;
        _location = location;
        Title = "Rescue Map";
    }

    /// <summary>Raised so the page can move the map camera (a pure View concern).</summary>
    public event Action<GeoPoint, double>? CenterRequested;

    public ObservableCollection<MapPinItem> Pins { get; } = new();

    public string[] SeverityFilters { get; } = new[] { "All", "Medium+", "High+", "Critical" };
    public string[] StatusFilters { get; } = new[] { "Open", "Reported", "In Progress", "Rescued", "All" };

    [ObservableProperty] private string selectedSeverityFilter = "All";
    [ObservableProperty] private string selectedStatusFilter = "Open";
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string resultText = string.Empty;

    private GeoPoint _center = new(14.8136, 121.0453); // default center until GPS answers
    private double _radiusKm = 5;

    public override async Task OnAppearingAsync()
    {
        if (_loaded.Count > 0) return;
        var here = await _location.GetCurrentAsync();
        if (here is not null) _center = here;
        CenterRequested?.Invoke(_center, _radiusKm);
        await LoadAsync();
    }

    /// <summary>Called by the page when the user pans/zooms; debounced so we don't spam the API.</summary>
    public void OnVisibleRegionChanged(double lat, double lng, double radiusKm)
    {
        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;

        Task.Delay(600, token).ContinueWith(async t =>
        {
            if (t.IsCanceled) return;
            var movedFar = Math.Abs(lat - _center.Latitude) > radiusKm / 222 || Math.Abs(lng - _center.Longitude) > radiusKm / 222;
            var zoomChanged = Math.Abs(radiusKm - _radiusKm) / Math.Max(_radiusKm, 0.1) > 0.3;
            _center = new GeoPoint(lat, lng);
            _radiusKm = Math.Clamp(radiusKm, 0.5, 100);

            if (movedFar || zoomChanged)
                await MainThread.InvokeOnMainThreadAsync(LoadAsync);
        }, TaskScheduler.Default);
    }

    [RelayCommand]
    private Task LoadAsync() => RunAsync(async () =>
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var result = await _reports.SearchAsync(SearchText, StatusFilter(), null);
            _loaded = result.Items.Where(MatchesSeverity).ToList();
        }
        else
        {
            var status = StatusFilter();
            var openOnly = SelectedStatusFilter == "Open";
            var all = await _reports.GetNearbyAsync(_center.Latitude, _center.Longitude, _radiusKm,
                                                    MinSeverity(), status, openOnly);
            _loaded = all;
        }
        RebuildPins();
    }, showBusy: false);

    [RelayCommand]
    private async Task CenterOnMeAsync()
    {
        var here = await _location.GetCurrentAsync();
        if (here is null) { await Dialogs.AlertAsync("Location off", "Turn on location services to center the map on you."); return; }
        _center = here;
        CenterRequested?.Invoke(here, 2);
    }

    partial void OnSelectedSeverityFilterChanged(string value) => LoadCommand.Execute(null);
    partial void OnSelectedStatusFilterChanged(string value) => LoadCommand.Execute(null);

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    /// <summary>Called when the user taps a pin's info window.</summary>
    public async Task OpenPinAsync(MapPinItem pin)
    {
        if (pin.ReportId is { } id)
        {
            await Shell.Current.GoToAsync("reportdetails", new Dictionary<string, object> { ["id"] = id });
        }
        else
        {
            // Cluster: zoom in on it.
            CenterRequested?.Invoke(new GeoPoint(pin.Location.Latitude, pin.Location.Longitude), Math.Max(_radiusKm / 4, 0.5));
        }
    }

    private void RebuildPins()
    {
        Pins.Clear();
        foreach (var pin in MarkerClusterer.Cluster(_loaded, _radiusKm)) Pins.Add(pin);
        ResultText = _loaded.Count == 0 ? "No reports in this area" : $"{_loaded.Count} report(s) in view";
    }

    // TODO(MEMBER 3): Turn the picker text into a severity:
    //   "Medium+" → SeverityLevel.Medium, "High+" → High, "Critical" → Critical, anything else → null.
    private SeverityLevel? MinSeverity() => null;

    private bool MatchesSeverity(ReportSummaryDto r) => MinSeverity() is not { } min || r.Severity >= min;

    // TODO(MEMBER 3): Turn the picker text into a status:
    //   "Reported" → ReportStatus.Reported, "In Progress" → InProgress, "Rescued" → Rescued, else → null.
    //   ("Open" and "All" stay null — LoadAsync sends openOnly = true for "Open".)
    private ReportStatus? StatusFilter() => null;
}
