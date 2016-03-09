using System;
using System.Threading.Tasks;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Test3DPrintApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Printing3D3MFPackage _package;

        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///zoetrope.3mf"));

            using (var stream = await file.OpenReadAsync())
            {
                await ThreadPool.RunAsync(async delegate
                {
                    _package = await Printing3D3MFPackage.LoadAsync(stream);

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        async delegate
                        {
                            var image = new BitmapImage();

                            await image.SetSourceAsync(_package.Thumbnail.TextureData);

                            Thumbnail.Source = image;
                        });
                });
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void MainPage_TaskRequested(Print3DManager sender, Print3DTaskRequestedEventArgs args)
        {
            args.Request.CreateTask("Zoetrope", string.Empty, OnSourceRequested);
        }

        private void OnSourceRequested(Print3DTaskSourceRequestedArgs args)
        {
            args.SetSource(_package);
        }

        private async void OnPrint(object sender, RoutedEventArgs e)
        {
            Print3DManager.GetForCurrentView().TaskRequested += MainPage_TaskRequested;

            await Print3DManager.ShowPrintUIAsync();

            Print3DManager.GetForCurrentView().TaskRequested -= MainPage_TaskRequested;
        }

        private async Task PrintFileAsync(StorageFile file)
        {
            if (file == null)
            {
                return;
            }

            using (var stream = await file.OpenReadAsync())
            {
                await ThreadPool.RunAsync(async delegate 
                {
                    _package = await Printing3D3MFPackage.LoadAsync(stream);
                });
                    
                Print3DManager.GetForCurrentView().TaskRequested += MainPage_TaskRequested;

                await Print3DManager.ShowPrintUIAsync();

                Print3DManager.GetForCurrentView().TaskRequested -= MainPage_TaskRequested;
            }
        }

        private async void OnSelectFile(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            picker.FileTypeFilter.Add(".3mf");

            var file = await picker.PickSingleFileAsync();

            await PrintFileAsync(file);
        }

        private async void OnLaunch(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///NeonHitch.jpg"));

            var options = new Windows.System.LauncherOptions();

            // Set 3d Builder as the target app
            options.TargetApplicationPackageFamilyName = "Microsoft.3DBuilder_8wekyb3d8bbwe";

            // Launch 3d Builder with any 2D image
            await Windows.System.Launcher.LaunchFileAsync(file, options);
        }
    }
}
