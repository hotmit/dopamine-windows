﻿using Digimezzo.Foundation.Core.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Dopamine.Converters
{
    public class PathToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = values[0] as string;
                int width = Int32.Parse(values[1].ToString());
                if (!string.IsNullOrEmpty(path) & System.IO.File.Exists(path))
                {
                    var info = new FileInfo(path);

                    if (info.Exists && info.Length > 0)
                    {
                        return ImageUtils.PathToBitmapImage(info.FullName, width, 0);
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
