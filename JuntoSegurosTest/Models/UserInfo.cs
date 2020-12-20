using System.ComponentModel.DataAnnotations;

namespace JuntoSegurosTest.Models
{
    public class UserInfo
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
