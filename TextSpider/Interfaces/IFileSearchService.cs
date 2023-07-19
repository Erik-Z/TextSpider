using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextSpider.Models;
using Windows.Storage;

namespace TextSpider.Interfaces
{
    public interface IFileSearchService
    {
        Task<IEnumerable<FileInformation>> SearchFilesInFolder(StorageFolder folder, string value);

        Task<FileInformation> SearchFile(StorageFile file, string value);
    }
}
