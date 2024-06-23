using System.ComponentModel.DataAnnotations;

namespace RunGroupWebApp.ViewModels
{
    public class RegisterViewModel
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage ="Email address is required")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage ="Confirm Password is required")]

        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Password did not match!")]
        public string ConfirmPassword { get; set; }

    }
}
