using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Rect = OpenCvSharp.Rect;

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
            var vm = this.DataContext as MainViewModel;
            vm.Load();
        }
    }

    public partial class MainViewModel :  ObservableObject
    {
        [ObservableProperty]
        BitmapImage imgSouce;

        public void Load()
        {
            Task.Run(() =>
            {
                var capture = new VideoCapture(0);
                capture.Set(VideoCaptureProperties.FrameWidth, 1920);
                capture.Set(VideoCaptureProperties.FrameHeight, 1080);
                //加载人眼、人脸模型数据
                OpenCvSharp.CascadeClassifier faceFinder = new CascadeClassifier(@".\haarcascade_frontalface_default.xml");
                OpenCvSharp.CascadeClassifier eyeFinder = new CascadeClassifier(@".\haarcascade_eye_tree_eyeglasses.xml");

                var frame = new Mat();
                while (true)
                {
                    capture.Read(frame);
                    var width = frame.Width;
                    var height = frame.Height;


                    //进行检测识别
                    var faceRects = faceFinder.DetectMultiScale(frame);
                    var eyeRects = eyeFinder.DetectMultiScale(frame);
                    var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

                    if (eyeRects.Length > 1 && faceRects.Length > 0)
                    {
                        Cv2.Rectangle(frame, faceRects[0], new Scalar(0, 0, 255), 1);
                        VerifyFace(bitmap, faceRects[0]);
                    }

                    ImgSouce = To(bitmap);
                    //this.img.Dispatcher.Invoke(() =>
                    //{
                    //    this.img.Source = To(frame);
                    //});
                }
            });
        }

        private void VerifyFace(Bitmap img, OpenCvSharp.Rect rect)
        {
            var faceImg = img.Clone(new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height), img.PixelFormat);
            faceImg = ResizeImage(faceImg, 150, 150);
            //faceImg.Save("./face.jpg", ImageFormat.Jpeg);
            var base64 = BitmapToBase64(faceImg);
            File.WriteAllText("./base64.txt", base64);
        }

        public static Bitmap ResizeImage(Bitmap bmp, int width, int height)
        {
            if (bmp.Width < width || bmp.Height < height)
            {
                return bmp;
            }
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(bmp, 0, 0, width, height);
            }
            return bitmap;
        }

        public static string BitmapToBase64(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage);
            }
        }

        private BitmapImage To(Bitmap image)
        {
            using MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Bmp);
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