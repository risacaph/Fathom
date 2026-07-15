using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Fathom.Viewer;

public partial class MainWindow : Window
{
    private readonly FathomClient _client = new();
    private int _chapterId;
    private int _page;
    private int _pages;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
    }

    /// <summary>Apply screen-capture protection as soon as the native window handle exists.</summary>
    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        NativeMethods.ProtectFromCapture(hwnd);
    }

    /// <summary>Swallow copy / save / print / screenshot shortcuts while the app has focus.</summary>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
            e.Key is Key.C or Key.S or Key.P or Key.Insert)
        {
            e.Handled = true;
        }
        if (e.Key is Key.PrintScreen or Key.Snapshot)
        {
            e.Handled = true;
        }
    }

    // ---------------- Login ----------------

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        LoginStatus.Text = "";
        LoginButton.IsEnabled = false;
        try
        {
            await _client.LoginAsync(ServerUrlBox.Text, UsernameBox.Text, PasswordBox.Password);
            Show(BrowsePanel);
            SearchBox.Focus();
        }
        catch (Exception ex)
        {
            LoginStatus.Text = "Sign-in failed: " + ex.Message;
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        SeriesList.ItemsSource = null;
        ChapterList.ItemsSource = null;
        SearchBox.Text = "";
        PasswordBox.Password = "";
        Show(LoginPanel);
    }

    // ---------------- Browse ----------------

    private async void Search_Click(object sender, RoutedEventArgs e) => await RunSearchAsync();

    private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) await RunSearchAsync();
    }

    private async Task RunSearchAsync()
    {
        var query = SearchBox.Text?.Trim();
        if (string.IsNullOrEmpty(query)) return;
        try
        {
            ChapterList.ItemsSource = null;
            SeriesList.ItemsSource = await _client.SearchAsync(query);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Search failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void SeriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SeriesList.SelectedItem is not SearchSeries series) return;
        try
        {
            ChapterList.ItemsSource = await _client.GetChaptersAsync(series.SeriesId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not load documents", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Open_Click(object sender, RoutedEventArgs e) => await OpenSelectedAsync();

    private async void ChapterList_DoubleClick(object sender, MouseButtonEventArgs e) => await OpenSelectedAsync();

    private async Task OpenSelectedAsync()
    {
        if (ChapterList.SelectedItem is not ChapterItem chapter) return;
        try
        {
            var info = await _client.GetInfoAsync(chapter.Id);
            _chapterId = chapter.Id;
            _pages = Math.Max(1, info.Pages);
            _page = 0;
            ReaderTitle.Text = string.IsNullOrWhiteSpace(info.Title) ? chapter.Display : info.Title;
            Show(ReaderPanel);
            await LoadPageAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not open document", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ---------------- Reader ----------------

    private async Task LoadPageAsync()
    {
        try
        {
            var bytes = await _client.GetPageAsync(_chapterId, _page);
            var bmp = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;   // decode now so the stream can be disposed
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            bmp.Freeze();
            PageImage.Source = bmp;

            PageIndicator.Text = $"Page {_page + 1} / {_pages}";
            PrevButton_UpdateState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not load page", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void PrevButton_UpdateState()
    {
        PrevButton.IsEnabled = _page > 0;
        NextButton.IsEnabled = _page < _pages - 1;
    }

    private async void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (_page <= 0) return;
        _page--;
        await LoadPageAsync();
    }

    private async void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_page >= _pages - 1) return;
        _page++;
        await LoadPageAsync();
    }

    private void CloseReader_Click(object sender, RoutedEventArgs e)
    {
        PageImage.Source = null;   // drop the in-memory page
        Show(BrowsePanel);
    }

    // ---------------- Helpers ----------------

    private void Show(UIElement panel)
    {
        LoginPanel.Visibility = ReferenceEquals(panel, LoginPanel) ? Visibility.Visible : Visibility.Collapsed;
        BrowsePanel.Visibility = ReferenceEquals(panel, BrowsePanel) ? Visibility.Visible : Visibility.Collapsed;
        ReaderPanel.Visibility = ReferenceEquals(panel, ReaderPanel) ? Visibility.Visible : Visibility.Collapsed;
    }
}
