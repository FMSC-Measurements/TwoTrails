using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TwoTrails.DAL;

namespace TwoTrails.Core.Media
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
                return mal.GetImageData(image);
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
    }
}
