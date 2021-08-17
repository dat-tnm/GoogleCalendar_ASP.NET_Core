using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models
{
    public class Credential
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string JsonValue { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
