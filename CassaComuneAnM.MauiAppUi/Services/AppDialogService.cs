using Microsoft.Maui.Controls.Shapes;

namespace CassaComuneAnM.MauiAppUi.Services;

public interface IAppDialogService
{
    Task ShowAlertAsync(string title, string message, string actionText = "CHIUDI");
    Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "CONFERMA", string cancelText = "ANNULLA");
    Task<T?> ShowSelectionAsync<T>(string title, string message, IReadOnlyList<T> items, Func<T, string> labelSelector, T? selected = default);
}

public sealed class AppDialogService : IAppDialogService
{
    public Task ShowAlertAsync(string title, string message, string actionText = "CHIUDI")
    {
        return ShowDialogAsync(title, message, new[]
        {
            new DialogAction<string>(actionText, "PrimaryButton", actionText)
        });
    }

    public async Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "CONFERMA", string cancelText = "ANNULLA")
    {
        var result = await ShowDialogAsync(title, message, new[]
        {
            new DialogAction<bool>(acceptText, "PrimaryButton", true),
            new DialogAction<bool>(cancelText, "SecondaryButton", false)
        });

        return result;
    }

    public async Task<T?> ShowSelectionAsync<T>(string title, string message, IReadOnlyList<T> items, Func<T, string> labelSelector, T? selected = default)
    {
        var hostPage = GetActivePage();
        if (hostPage is null)
        {
            return default;
        }

        var completion = new TaskCompletionSource<T?>();
        var listLayout = new VerticalStackLayout { Spacing = 10 };

        foreach (var item in items)
        {
            var button = new Button
            {
                Text = labelSelector(item),
                Style = (Style?)Application.Current?.Resources["SecondaryButton"],
                HorizontalOptions = LayoutOptions.Fill,
                BorderWidth = EqualityComparer<T>.Default.Equals(item, selected) ? 2 : 1
            };

            button.Clicked += async (_, _) =>
            {
                completion.TrySetResult(item);
                await hostPage.Navigation.PopModalAsync(false);
            };

            listLayout.Add(button);
        }

        var cancelButton = new Button
        {
            Text = "CHIUDI",
            Style = (Style?)Application.Current?.Resources["TertiaryButton"]
        };
        cancelButton.Clicked += async (_, _) =>
        {
            completion.TrySetResult(default);
            await hostPage.Navigation.PopModalAsync(false);
        };

        var scrollableList = new ScrollView
        {
            Content = listLayout,
            MaximumHeightRequest = 420
        };

        var page = CreateHostPage(title, message, scrollableList, cancelButton);
        await hostPage.Navigation.PushModalAsync(page, false);
        return await completion.Task;
    }

    private async Task<T> ShowDialogAsync<T>(string title, string message, IReadOnlyList<DialogAction<T>> actions)
    {
        var hostPage = GetActivePage();
        if (hostPage is null)
        {
            return default!;
        }

        var completion = new TaskCompletionSource<T>();
        var buttonRow = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Fill
        };

        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = action.Text,
                Style = (Style?)Application.Current?.Resources[action.StyleKey],
                HorizontalOptions = LayoutOptions.Fill
            };

            button.Clicked += async (_, _) =>
            {
                completion.TrySetResult(action.Result);
                await hostPage.Navigation.PopModalAsync(false);
            };

            buttonRow.Add(button);
        }

        var page = CreateHostPage(title, message, buttonRow);
        await hostPage.Navigation.PushModalAsync(page, false);
        return await completion.Task;
    }

    private static ContentPage CreateHostPage(string title, string message, View content, View? footer = null)
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#88000000"),
            Padding = new Thickness(16)
        };

        var cardContent = new VerticalStackLayout
        {
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            MaximumWidthRequest = 520
        };

        cardContent.Add(new Label
        {
            Text = title.ToUpperInvariant(),
            Style = (Style?)Application.Current?.Resources["SectionLabel"]
        });
        cardContent.Add(new Label
        {
            Text = message,
            Style = (Style?)Application.Current?.Resources["DialogMessageLabel"]
        });
        cardContent.Add(content);

        if (footer is not null)
        {
            cardContent.Add(footer);
        }

        overlay.Add(new Border
        {
            Background = GetBrush("SurfaceBrush"),
            Stroke = GetBrush("BorderStrongBrush"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24) },
            Padding = new Thickness(20),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            MaximumWidthRequest = 560,
            MaximumHeightRequest = 760,
            Content = cardContent
        });

        return new ContentPage
        {
            BackgroundColor = Colors.Transparent,
            Content = overlay
        };
    }

    private static Page? GetActivePage()
    {
        Page? current = Application.Current?.MainPage;

        while (current is NavigationPage navigationPage)
        {
            current = navigationPage.CurrentPage;
        }

        while (current is FlyoutPage flyoutPage)
        {
            current = flyoutPage.Detail;
        }

        while (current is TabbedPage tabbedPage)
        {
            current = tabbedPage.CurrentPage;
        }

        return current;
    }

    private static Brush GetBrush(string key) => (Brush)(Application.Current?.Resources[key] ?? Brush.Transparent);

    private sealed record DialogAction<T>(string Text, string StyleKey, T Result);
}
