using PawRescue.Mobile.ViewModels;

namespace PawRescue.Mobile.Views;

/// <summary>
/// Every screen inherits this: it forwards OnAppearing to the ViewModel,
/// so pages contain no logic of their own (MVVM).
/// </summary>
public class BasePage : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BaseViewModel vm)
            await vm.OnAppearingAsync();
    }
}
