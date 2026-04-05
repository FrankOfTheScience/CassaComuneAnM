using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace CassaComuneAnM.MauiAppUi.Services;

public interface IAppDialogService
{
    Task ShowAlertAsync(string title, string message, string actionText = "CHIUDI");
    Task<bool> ShowConfirmAsync(string title, string message, string acceptText = "CONFERMA", string cancelText = "ANNULLA");
    Task<T?> ShowSelectionAsync<T>(string title, string message, IReadOnlyList<T> items, Func<T, string> labelSelector, T? selected = default);
    Task<DateTime?> ShowDatePickerAsync(string title, string message, DateTime selectedDate);
    Task<string?> ShowDetailActionsAsync(string title, IReadOnlyList<DialogDetailRow> rows, IReadOnlyList<string> actions);
}

public readonly record struct DialogDetailRow(string Label, string Value, string? Secondary = null);

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
                Style = (Style?)Application.Current?.Resources["DialogSecondaryButton"],
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

        var scrollableList = new ScrollView
        {
            Content = listLayout,
            MaximumHeightRequest = 420
        };

        var page = CreateHostPage(title, message, scrollableList, null, async () =>
        {
            completion.TrySetResult(default);
            await hostPage.Navigation.PopModalAsync(false);
        });
        await hostPage.Navigation.PushModalAsync(page, false);
        return await completion.Task;
    }

    public async Task<string?> ShowDetailActionsAsync(string title, IReadOnlyList<DialogDetailRow> rows, IReadOnlyList<string> actions)
    {
        var hostPage = GetActivePage();
        if (hostPage is null)
        {
            return null;
        }

        var completion = new TaskCompletionSource<string?>();
        var content = new VerticalStackLayout { Spacing = 10 };

        foreach (var row in rows)
        {
            var details = new VerticalStackLayout { Spacing = 2 };
            details.Add(new Label
            {
                FormattedText = new FormattedString
                {
                    Spans =
                    {
                        new Span { Text = $"{row.Label}: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = row.Value }
                    }
                }
            });

            if (!string.IsNullOrWhiteSpace(row.Secondary))
            {
                details.Add(new Label
                {
                    Text = row.Secondary,
                    Style = (Style?)Application.Current?.Resources["SecondaryValueLabel"],
                    FontAttributes = FontAttributes.Italic
                });
            }

            content.Add(new Frame
            {
                Style = (Style?)Application.Current?.Resources["ListCardFrame"],
                Padding = new Thickness(12),
                Content = details
            });
        }

        var buttons = new VerticalStackLayout { Spacing = 8 };
        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = action,
                Style = (Style?)Application.Current?.Resources[action == "ELIMINA" ? "DialogGhostButton" : "DialogButton"]
            };

            button.Clicked += async (_, _) =>
            {
                completion.TrySetResult(action);
                await hostPage.Navigation.PopModalAsync(false);
            };

            buttons.Add(button);
        }

        var page = CreateHostPage(title, string.Empty, content, buttons, async () =>
        {
            completion.TrySetResult(null);
            await hostPage.Navigation.PopModalAsync(false);
        });

        await hostPage.Navigation.PushModalAsync(page, false);
        return await completion.Task;
    }

    public async Task<DateTime?> ShowDatePickerAsync(string title, string message, DateTime selectedDate)
    {
        var hostPage = GetActivePage();
        if (hostPage is null)
        {
            return null;
        }

        var completion = new TaskCompletionSource<DateTime?>();
        var currentDate = selectedDate.Date;
        var visibleMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        var previewLabel = new Label
        {
            Text = currentDate.ToString("dd/MM/yyyy"),
            Style = (Style?)Application.Current?.Resources["CardTitleLabel"],
            HorizontalTextAlignment = TextAlignment.Center
        };
        var monthLabel = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            Style = (Style?)Application.Current?.Resources["SectionLabel"]
        };
        var calendarGrid = new Grid
        {
            ColumnSpacing = 6,
            RowSpacing = 6
        };

        for (var column = 0; column < 7; column++)
        {
            calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (var row = 0; row < 7; row++)
        {
            calendarGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        void RefreshCalendar()
        {
            previewLabel.Text = currentDate.ToString("dd/MM/yyyy");
            monthLabel.Text = visibleMonth.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("it-IT")).ToUpperInvariant();
            calendarGrid.Children.Clear();

            var weekDays = new[] { "L", "M", "M", "G", "V", "S", "D" };
            for (var index = 0; index < weekDays.Length; index++)
            {
                calendarGrid.Add(new Label
                {
                    Text = weekDays[index],
                    HorizontalTextAlignment = TextAlignment.Center,
                    Style = (Style?)Application.Current?.Resources["FieldLabel"]
                }, index, 0);
            }

            var firstDay = visibleMonth;
            var offset = ((int)firstDay.DayOfWeek + 6) % 7;
            var start = firstDay.AddDays(-offset);

            for (var slot = 0; slot < 42; slot++)
            {
                var day = start.AddDays(slot);
                var isCurrentMonth = day.Month == visibleMonth.Month && day.Year == visibleMonth.Year;
                var isSelected = day.Date == currentDate;
                var button = new Button
                {
                    Text = day.Day.ToString("00"),
                    Style = (Style?)Application.Current?.Resources[isSelected ? "DateCellButton" : "DateCellGhostButton"],
                    Opacity = isCurrentMonth ? 1 : 0.45,
                    Padding = new Thickness(0),
                    Margin = 0,
                    MinimumHeightRequest = 28,
                    MinimumWidthRequest = 28
                };

                var capturedDay = day;
                button.Clicked += (_, _) =>
                {
                    currentDate = capturedDay.Date;
                    visibleMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                    RefreshCalendar();
                };

                calendarGrid.Add(button, slot % 7, (slot / 7) + 1);
            }
        }

        var previousMonthButton = new Button
        {
            Text = "‹",
            WidthRequest = 40,
            Style = (Style?)Application.Current?.Resources["ChipGhostButton"]
        };
        previousMonthButton.Clicked += (_, _) =>
        {
            visibleMonth = visibleMonth.AddMonths(-1);
            RefreshCalendar();
        };

        var nextMonthButton = new Button
        {
            Text = "›",
            WidthRequest = 40,
            Style = (Style?)Application.Current?.Resources["ChipGhostButton"]
        };
        nextMonthButton.Clicked += (_, _) =>
        {
            visibleMonth = visibleMonth.AddMonths(1);
            RefreshCalendar();
        };

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8
        };
        header.Add(previousMonthButton, 0, 0);
        header.Add(monthLabel, 1, 0);
        header.Add(nextMonthButton, 2, 0);

        var content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Frame
                {
                    Style = (Style?)Application.Current?.Resources["ListCardFrame"],
                    Content = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children =
                        {
                            new Label
                            {
                                Text = "DATA SELEZIONATA",
                                Style = (Style?)Application.Current?.Resources["FieldLabel"],
                                HorizontalTextAlignment = TextAlignment.Center
                            },
                            previewLabel
                        }
                    }
                },
                header,
                calendarGrid
            }
        };
        RefreshCalendar();

        var actions = new VerticalStackLayout { Spacing = 8 };

        var confirmButton = new Button
        {
            Text = "CONFERMA",
            Style = (Style?)Application.Current?.Resources["DialogButton"]
        };
        confirmButton.Clicked += async (_, _) =>
        {
            completion.TrySetResult(currentDate);
            await hostPage.Navigation.PopModalAsync(false);
        };

        var cancelButton = new Button
        {
            Text = "ANNULLA",
            Style = (Style?)Application.Current?.Resources["DialogGhostButton"]
        };
        cancelButton.Clicked += async (_, _) =>
        {
            completion.TrySetResult(null);
            await hostPage.Navigation.PopModalAsync(false);
        };

        actions.Add(confirmButton);
        actions.Add(cancelButton);

        var page = CreateHostPage(title, message, content, actions, async () =>
        {
            completion.TrySetResult(null);
            await hostPage.Navigation.PopModalAsync(false);
        });
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
        var buttonRow = new VerticalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Fill
        };

        foreach (var action in actions)
        {
            var button = new Button
            {
                Text = action.Text,
                Style = (Style?)Application.Current?.Resources[
                    action.StyleKey == "PrimaryButton" ? "DialogButton" :
                    action.StyleKey == "SecondaryButton" ? "DialogSecondaryButton" :
                    action.StyleKey == "TertiaryButton" ? "DialogGhostButton" :
                    action.StyleKey],
                HorizontalOptions = LayoutOptions.Fill
            };

            button.Clicked += async (_, _) =>
            {
                completion.TrySetResult(action.Result);
                await hostPage.Navigation.PopModalAsync(false);
            };

            buttonRow.Add(button);
        }

        var page = CreateHostPage(title, message, buttonRow, null, async () =>
        {
            completion.TrySetResult(actions.Last().Result);
            await hostPage.Navigation.PopModalAsync(false);
        });
        await hostPage.Navigation.PushModalAsync(page, false);
        return await completion.Task;
    }

    private static ContentPage CreateHostPage(string title, string message, View content, View? footer = null, Func<Task>? dismissAction = null)
    {
        var overlay = new Grid
        {
            BackgroundColor = Color.FromArgb("#88000000"),
            Padding = new Thickness(18)
        };

        var cardContent = new VerticalStackLayout
        {
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            MaximumWidthRequest = 520
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        headerGrid.Add(new Label
        {
            Text = title.ToUpperInvariant(),
            Style = (Style?)Application.Current?.Resources["SectionLabel"]
        });

        if (dismissAction is not null)
        {
            var closeButton = new Button
            {
                Text = "CHIUDI",
                Style = (Style?)Application.Current?.Resources["ChipGhostButton"],
                Padding = new Thickness(10, 6),
                Margin = 0
            };
            closeButton.Clicked += async (_, _) => await dismissAction();
            headerGrid.Add(closeButton, 1, 0);
        }

        cardContent.Add(headerGrid);
        if (!string.IsNullOrWhiteSpace(message))
        {
            cardContent.Add(new Label
            {
                Text = message,
                Style = (Style?)Application.Current?.Resources["DialogMessageLabel"]
            });
        }
        cardContent.Add(content);

        if (footer is not null)
        {
            cardContent.Add(footer);
        }

        var scroll = new ScrollView
        {
            Content = cardContent
        };

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
            Content = scroll
        });

        return new DialogHostPage(dismissAction)
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

    private sealed class DialogHostPage : ContentPage
    {
        private readonly Func<Task>? _dismissAction;

        public DialogHostPage(Func<Task>? dismissAction)
        {
            _dismissAction = dismissAction;
        }

        protected override bool OnBackButtonPressed()
        {
            if (_dismissAction is null)
            {
                return base.OnBackButtonPressed();
            }

            MainThread.BeginInvokeOnMainThread(async () => await _dismissAction());
            return true;
        }
    }
}
