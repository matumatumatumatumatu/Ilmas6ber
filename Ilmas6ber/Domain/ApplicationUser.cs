using System;
using System.Collections.Generic;
using System.Text;

namespace Ilmas6ber.Domain
{
    public class ApplicationUser
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int ProfilePictureID { get; set; }
        public bool TeamColor { get; set; }
        public double xpPoints { get; set; }
    }
}
