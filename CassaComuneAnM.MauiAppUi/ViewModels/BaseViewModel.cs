using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CassaComuneAnM.MauiAppUi.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    protected INavigation? Navigation => Application.Current?.MainPage?.Navigation;

    protected Task ShowAlertAsync(string title, string message)
    {
        if (Application.Current?.MainPage is null)
        {
            return Task.CompletedTask;
        }

        return Application.Current.MainPage.DisplayAlert(title, message, "OK");
    }

    protected Task<bool> ShowConfirmAsync(string title, string message)
    {
        if (Application.Current?.MainPage is null)
        {
            return Task.FromResult(false);
        }

        return Application.Current.MainPage.DisplayAlert(title, message, "Si", "No");
    }
}
