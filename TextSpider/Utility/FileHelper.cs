using System;
using System.IO;

namespace TextSpider.Utility
{
    internal static class FileHelper
    {
        public static string FormatFileSize(ulong fileSize)
        {
            string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (fileSize == 0)
                return "0 " + sizeSuffixes[0];

            int suffixIndex = (int)(Math.Log(fileSize, 1024));
            double normalizedSize = fileSize / Math.Pow(1024, suffixIndex);
            string formattedSize = $"{normalizedSize:0.##} {sizeSuffixes[suffixIndex]}";

            return formattedSize;
        }

        public static bool IsFolderPath(string path)
        {
            System.IO.FileAttributes attributes = File.GetAttributes(path);
            return (attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
        }
    }
}
