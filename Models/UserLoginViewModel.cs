using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using QSProject.Data.Models;

namespace QSProject.Models
{
    public class UserLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
