using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextSpider.Interfaces
{
    public interface IDialogService
    {
        Task ShowDialogAsync(string title, string message, string closeButtonText);
    }
}
