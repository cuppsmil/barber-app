using BarberApp.ViewModels;

namespace BarberApp.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage()
    {
        InitializeComponent();
        _viewModel = (HomeViewModel)BindingContext;

        // Подписка на изменения для управления UI
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.IsLoading))
        {
            LoadingIndicator.IsRunning = _viewModel.IsLoading;
            LoadingIndicator.IsVisible = _viewModel.IsLoading;
            MainContent.IsVisible = !_viewModel.IsLoading;
        }

        if (e.PropertyName == nameof(HomeViewModel.ErrorMessage))
        {
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                ErrorLabel.Text = _viewModel.ErrorMessage;
                ErrorContainer.IsVisible = true;
                MainContent.IsVisible = false;
            }
            else
            {
                ErrorContainer.IsVisible = false;
                MainContent.IsVisible = !_viewModel.IsLoading;
            }
        }
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        ErrorContainer.IsVisible = false;
        await _viewModel.ReloadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
}