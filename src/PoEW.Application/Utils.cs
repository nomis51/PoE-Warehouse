using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PoEW.Application {
    public static class Utils {
        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap) {
            using (var memory = new System.IO.MemoryStream()) {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static bool ValidSessionId(string value) {
            return new System.Text.RegularExpressions.Regex("[a-z0-9]{32}").IsMatch(value);
        }

    }
}
