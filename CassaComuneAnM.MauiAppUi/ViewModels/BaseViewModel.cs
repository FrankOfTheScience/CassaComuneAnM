using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using CassaComuneAnM.MauiAppUi.Services;
using Microsoft.Extensions.DependencyInjection;

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

    private IAppDialogService? DialogService =>
        Application.Current?.Handler?.MauiContext?.Services.GetService<IAppDialogService>();

    protected Task ShowAlertAsync(string title, string message)
    {
        if (DialogService is null)
        {
            return Task.CompletedTask;
        }

        return DialogService.ShowAlertAsync(title, message);
    }

    protected Task<bool> ShowConfirmAsync(string title, string message)
    {
        if (DialogService is null)
        {
            return Task.FromResult(false);
        }

        return DialogService.ShowConfirmAsync(title, message);
    }

    protected Task<T?> ShowSelectionAsync<T>(string title, string message, IReadOnlyList<T> items, Func<T, string> labelSelector, T? selected = default)
    {
        if (DialogService is null)
        {
            return Task.FromResult<T?>(default);
        }

        return DialogService.ShowSelectionAsync(title, message, items, labelSelector, selected);
    }

    protected Task<DateTime?> ShowDatePickerAsync(string title, string message, DateTime selectedDate)
    {
        if (DialogService is null)
        {
            return Task.FromResult<DateTime?>(null);
        }

        return DialogService.ShowDatePickerAsync(title, message, selectedDate);
    }

    protected Task<string?> ShowDetailActionsAsync(string title, IReadOnlyList<DialogDetailRow> rows, IReadOnlyList<string> actions)
    {
        if (DialogService is null)
        {
            return Task.FromResult<string?>(null);
        }

        return DialogService.ShowDetailActionsAsync(title, rows, actions);
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

    protected string FormatLocalizedDecimalInput(decimal value, string format = "N2")
    {
        return value.ToString(format, CultureInfo.GetCultureInfo("it-IT"));
    }
}
