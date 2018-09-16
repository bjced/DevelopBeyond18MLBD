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


        //Select the labels to use
        string alt1Name = "Alt1";
        string alt2Name = "Alt2";

        //DB18Model _model = new DB18Model();
        //DB18ModelInput _inputData = new DB18ModelInput();

        public MainPage()
        {
            this.InitializeComponent();
            //LoadModelAsync();
            Setup();
        }

        /*
        private async Task LoadModelAsync()
        {
            // Load the .onnx file
            Uri uri = new Uri($"ms-appx:///Assets/DB18.onnx");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            _model = await DB18Model.CreateDB18Model(file);
        }
        
        
        async void EvalSnappedImage()
        {
            //Get a frame for input
            VideoFrame frame = await CreateVideoFrameFromImage(imageBox_Snap);
            _inputData.data = frame;
            //Inference using model and input data
            var evalOutput = await _model.EvaluateAsync(_inputData);
            // Do something with the model output
            var Alt1Probability = evalOutput.loss.First(x => x.Key == alt1Name);
            var Alt2Probability = evalOutput.loss.First(x => x.Key == alt2Name);
            _alt1TextBox.Text = Alt1Probability.Key + ": " + Math.Round(Alt1Probability.Value, 2);
            _alt2TextBox.Text = Alt2Probability.Key + ": " + Math.Round(Alt2Probability.Value, 2);
            _alt1Staple.Value = Alt1Probability.Value;
            _alt2Staple.Value = Alt2Probability.Value;

            if (Alt1Probability.Value > 0.8)
            {
                AddAlt1();
            }
            else if (Alt2Probability.Value > 0.8)
            {
                AddAlt2();
            }
        }
        */

        private void TakeSnapshot(object sender, RoutedEventArgs e)
        {
            imageBox_Snap.Source = imageBox_Play.Source;
            //EvalSnappedImage();
        }

        #region helper functions

        private void AddAlt2()
        {
            Int32 val2 = (Int32.Parse(_alt2ResultBox.Text)) + 1;
            _alt2ResultBox.Text = (val2).ToString();
        }

        private void AddAlt1()
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

        private void Setup()
        {
            // Setup background worker
            _bw.DoWork += BackgroundWorkerDoWork;
            _bw.ProgressChanged += BackgroundWorkerProgressChanged;
            _bw.WorkerReportsProgress = true;
            _bw.WorkerSupportsCancellation = true;
            _appleButton.Content = alt1Name;
            _bananaButton.Content = alt2Name;
            _alt1TextBox.Text = alt1Name;
            _alt2TextBox.Text = alt2Name;
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

        private void Alt1ButtonClick(object sender, RoutedEventArgs e)
        {
            _alt1Staple.Value = 1;
            _alt2Staple.Value = 0;
            AddAlt1();
        }

        private void Alt2ButtonClick(object sender, RoutedEventArgs e)
        {
            _alt1Staple.Value = 0;
            _alt2Staple.Value = 1;
            AddAlt2();
        }

        #endregion
    }
}
