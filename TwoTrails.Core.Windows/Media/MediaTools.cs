using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TwoTrails.Core.Media;
using TwoTrails.DAL;

namespace TwoTrails.Core
{
    public static class MediaTools
    {
        public static BitmapImage LoadImage(this ITtMediaLayer mal, TtImage image)
        {
            BitmapImage bitmap = new BitmapImage();

            if (image.IsExternal)
            {
                if (File.Exists(image.FilePath))
                {
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(image.FilePath);
                    bitmap.EndInit();
                    return bitmap;
                }
                else
                {
                    throw new FileNotFoundException(image.FilePath);
                }
            }
            else
            {
                return GetImageData(mal, image);
            }
        }

        public static Task<BitmapImage> LoadImageAsync(this ITtMediaLayer mal, TtImage image, AsyncCallback callback)
        {
            return Task.Run(() => {
                BitmapImage bi = LoadImage(mal, image);

                callback(new ImageAsyncResult(false, true, Tuple.Create(image, bi)));

                return bi;
            });
        }


        public static void SaveImageToFile(this BitmapImage image, string filePath)
        {
            if (image != null)
            {
                BitmapEncoder encoder;

                switch (Path.GetExtension(filePath).ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }

                encoder.Frames.Add(BitmapFrame.Create(image));

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                } 
            }
        }


        public class ImageAsyncResult : IAsyncResult
        {
            public bool IsCompleted { get; private set; }

            public WaitHandle AsyncWaitHandle { get; } = null;

            public object AsyncState { get; private set; }

            public TtImage ImageInfo { get; }

            public BitmapImage Image { get; }

            public bool CompletedSynchronously { get; private set; }

            public ImageAsyncResult(bool competedSynchronously, bool isCompleted, object obj)
            {
                IsCompleted = isCompleted;
                CompletedSynchronously = competedSynchronously;
                AsyncState = obj;

                Tuple<TtImage, BitmapImage> tup = (Tuple<TtImage, BitmapImage>)AsyncState;

                ImageInfo = tup.Item1;
                Image = tup.Item2;
            }
        }


        public static Color GetColor(int argb)
        {
            return Color.FromArgb(GetAlpha(argb), GetRed(argb), GetGreen(argb), GetBlue(argb));
        }

        public static BitmapImage GetImageData(ITtMediaLayer mal, TtImage image)
        {
            BitmapImage bitmap = new BitmapImage();

            byte[] data = mal.GetRawImageData(image);

            if (data != null)
            {
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(data);
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }



        public static byte GetAlpha(int color)
        {
            return (byte)((color >> 24) & 255);
        }

        public static byte GetRed(int color)
        {
            return (byte)((color >> 16) & 255);
        }

        public static byte GetGreen(int color)
        {
            return (byte)((color >> 8) & 255);
        }

        public static byte GetBlue(int color)
        {
            return (byte)(color & 255);
        }
    }
}
