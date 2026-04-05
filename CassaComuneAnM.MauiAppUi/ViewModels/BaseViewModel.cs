using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

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
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    protected async Task RunBusyAsync(Func<Task> action, string errorTitle = "Errore")
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(errorTitle, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected bool TryParseDecimalInput(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim();
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"), out value)
            || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(normalized.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(normalized.Replace('.', ','), NumberStyles.Number, CultureInfo.GetCultureInfo("it-IT"), out value);
    }

    protected string FormatDecimalInput(decimal value, string format = "0.####")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }
}
