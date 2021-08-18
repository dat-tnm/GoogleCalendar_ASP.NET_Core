using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Attachment
    {
        public string FileId { get; set; }
        public string FileUrl { get; set; }
        public string IconLink { get; set; }
        public string MimeType { get; set; }
        public string Title { get; set; }
        public string ETag { get; set; }
    }
}
