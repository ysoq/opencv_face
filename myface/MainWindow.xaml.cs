using OpenCvSharp;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace myface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var capture = new VideoCapture(0);
            capture.Set(VideoCaptureProperties.FrameWidth, 1920);
            capture.Set(VideoCaptureProperties.FrameHeight, 1080);

            Task.Run(() =>
            {
                var frame = new Mat();
                while (true)
                {
                    capture.Read(frame);
                    var width = frame.Width;
                    var height = frame.Height;

                    //加载人眼、人脸模型数据
                    OpenCvSharp.CascadeClassifier faceFinder = new CascadeClassifier(@"C:\code\2.face\haarcascade_frontalface_default.xml");
                    OpenCvSharp.CascadeClassifier eyeFinder = new CascadeClassifier(@"C:\code\2.face\haarcascade_eye_tree_eyeglasses.xml");
                    //进行检测识别
                    var faceRects = faceFinder.DetectMultiScale(frame);
                    var eyeRects = eyeFinder.DetectMultiScale(frame);
                    if (eyeRects.Length > 1 && faceRects.Length > 0)
                    {
                        Cv2.Rectangle(frame, faceRects[0], new Scalar(0, 0, 255), 1);
                    }

                    this.img.Dispatcher.Invoke(() =>
                    {
                        this.img.Source = To(frame);
                    });
                }
            });
        }

        private BitmapImage To(Mat image)
        {
            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);

            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
}