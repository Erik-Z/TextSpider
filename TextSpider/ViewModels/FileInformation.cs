using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSpider.ViewModels
{
    public class FileInformation
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }
        public int Matches { get; set; }
        public string FileSize {get; set;}
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public string Encoding { get; set; }
        public string Attributes { get; set; }
        public string Results { get; set; }
    }
}
