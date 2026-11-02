using CommunityToolkit.Mvvm.ComponentModel;
using PawRescue.Mobile.Services;

namespace PawRescue.Mobile.ViewModels;

/// <summary>
/// INHERITANCE + ABSTRACTION for the UI layer.
/// Every ViewModel gets IsBusy/Title and one safe way to run async work
/// (busy flag + friendly error dialog), so the pattern isn't repeated 11 times.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly IDialogService Dialogs;

    protected BaseViewModel(IDialogService dialogs) => Dialogs = dialogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    public bool IsNotBusy => !IsBusy;

    /// <summary>Runs an action with the busy indicator and shows API errors as a dialog.</summary>
    protected async Task RunAsync(Func<Task> action, bool showBusy = true)
    {
        if (IsBusy) return;
        try
        {
            if (showBusy) IsBusy = true;
            await action();
        }
        catch (ApiException ex)
        {
            await Dialogs.AlertAsync("Oops", ex.Message);
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                await Shell.Current.GoToAsync("//login");
        }
        catch (HttpRequestException)
        {
            await Dialogs.AlertAsync("No connection", "Can't reach the Paw Rescue server. Check your internet connection and try again.");
        }
        catch (TaskCanceledException)
        {
            await Dialogs.AlertAsync("Timed out", "The server took too long to respond. Please try again.");
        }
        finally
        {
            if (showBusy) IsBusy = false;
        }
    }

    /// <summary>Called by the page's OnAppearing. Override to load data.</summary>
    public virtual Task OnAppearingAsync() => Task.CompletedTask;
}
