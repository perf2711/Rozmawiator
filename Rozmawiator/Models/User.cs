using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Rozmawiator.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; }
        public ImageSource Avatar { get; set; }
        public DateTime RegistrationDateTime { get; set; }
    }
}
