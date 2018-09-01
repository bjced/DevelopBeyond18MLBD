using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DevelopBeyond18MLBD
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BackgroundWorker _bw = new BackgroundWorker();
        string _uripath = @"http://192.168.0.130:8080/shot.jpg";
        int _framerate = 10;//Hz

        //BananaramaModel _model = new BananaramaModel();
        //Bananarama _inputData = new Bananarama();


        public MainPage()
        {
            this.InitializeComponent();
            //LoadModelAsync();
            SetupBackgroundWorker();
        }


        /*
        private async Task LoadModelAsync()
        {
            // Load the .onnx file
            Uri uri = new Uri($"ms-appx:///Assets/Bananarama.onnx");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            _model = await BananaramaModel.CreateBananaramaModel(file);
        }*/
        
        /*
        async void EvalSnappedImage()
        {
            //Get a frame for input
            VideoFrame frame = await CreateVideoFrameFromImage(imageBox_Snap);
            _inputData.data = frame;
            //Inference using model and input data
            var evalOutput = await _model.EvaluateAsync(_inputData);
            // Do something with the model output
            var appleProbability = evalOutput.loss.First(x => x.Key == "apple");
            var bananaProbability = evalOutput.loss.First(x => x.Key == "banana");
            _appleTextBox.Text = appleProbability.Key + ": " + Math.Round(appleProbability.Value, 2);
            _bananaTextBox.Text = bananaProbability.Key + ": " + Math.Round(bananaProbability.Value, 2);
            _appleStaple.Value = appleProbability.Value;
            _bananaStaple.Value = bananaProbability.Value;

            if (appleProbability.Value > 0.8)
            {
                AddApple();
            }
            else if (bananaProbability.Value > 0.8)
            {
                AddBanana();
            }
        }
        */

        private void TakeSnapshot(object sender, RoutedEventArgs e)
        {
            imageBox_Snap.Source = imageBox_Play.Source;
            //EvalSnappedImage();
        }

        #region helper functions

        private void AddBanana()
        {
            Int32 val2 = (Int32.Parse(_alt2ResultBox.Text)) + 1;
            _alt2ResultBox.Text = (val2).ToString();
        }

        private void AddApple()
        {
            Int32 val1 = (Int32.Parse(_alt1ResultBox.Text)) + 1;
            _alt1ResultBox.Text = (val1).ToString();
        }

        private async Task RefreshImageFromUri()
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                InMemoryRandomAccessStream randomAccess = new InMemoryRandomAccessStream();
                System.Net.Http.HttpResponseMessage imageResponse = await client.GetAsync(_uripath);
                DataWriter writer = new DataWriter(randomAccess.GetOutputStreamAt(0));
                writer.WriteBytes(await imageResponse.Content.ReadAsByteArrayAsync());
                await writer.StoreAsync();
                BitmapImage bm = new BitmapImage();
                await bm.SetSourceAsync(randomAccess);
                imageBox_Play.Source = bm;
            }
            catch (Exception)
            {
                //Ignore
            }
        }


        private static async Task<VideoFrame> CreateVideoFrameFromImage(Image image)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(image);

            IBuffer pixels = await renderTargetBitmap.GetPixelsAsync();
            var bitmap = SoftwareBitmap.CreateCopyFromBuffer(pixels,
                                                             BitmapPixelFormat.Bgra8,
                                                             renderTargetBitmap.PixelWidth,
                                                             renderTargetBitmap.PixelHeight,
                                                             BitmapAlphaMode.Premultiplied);

            var frame = VideoFrame.CreateWithSoftwareBitmap(bitmap);
            return frame;
        }

        async Task<SoftwareBitmap> CreateSoftwareBitmapAsync()
        {
            SoftwareBitmap softwareBitmap;
            FileOpenPicker fileOpenPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            return softwareBitmap;
        }

        private void SetupBackgroundWorker()
        {
            // Setup background worker
            _bw.DoWork += BackgroundWorkerDoWork;
            _bw.ProgressChanged += BackgroundWorkerProgressChanged;
            _bw.WorkerReportsProgress = true;
            _bw.WorkerSupportsCancellation = true;
        }
        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            while (_bw.CancellationPending == false)
            {
                Task.Delay(1000 / _framerate).Wait();
                _bw.ReportProgress(i); //Use for UI updates
                i++;
            }
        }

        private void BackgroundWorkerProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var tsk = RefreshImageFromUri();
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (_bw.IsBusy)
            {
                _bw.CancelAsync();
            }
            else
            {
                _bw.RunWorkerAsync();
            }
        }

        private void AppleButtonClick(object sender, RoutedEventArgs e)
        {
            _appleStaple.Value = 1;
            _bananaStaple.Value = 0;
            AddApple();
        }

        private void BananaButtonClick(object sender, RoutedEventArgs e)
        {
            _appleStaple.Value = 0;
            _bananaStaple.Value = 1;
            AddBanana();
        }

        #endregion
    }
}
