using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ilmas6ber.Models.Accounts
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Sisesta oma parool uuesti: ")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Paroolid ei kattu. Proovi uuesti.")]
        public string ConfirmPassword { get; set; }
        public string DisplayName { get; set; }


    }
}
