using System;
using System.Collections.Generic;
using System.Text;

namespace Ilmas6ber.Domain
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int ProfilePictureID { get; set; }
        public bool TeamColor { get; set; }
        public double xpPoints { get; set; }
        public int xpLevel { get; set; }
    }
}
