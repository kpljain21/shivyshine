using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Shivyshine.Areas.Identity.Data;
using System.Linq;

namespace Shivyshine.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Username/Mobile Number/Email Address")]
            [Required(ErrorMessage = "Please enter Username/Mobile Number/Email Address")]
            [RegularExpression(@"^\S*$", ErrorMessage = "Space not allowed, please enter valid Username/Mobile Number/Email Address")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByNameAsync(Input.Email) != null)
                {
                    Input.Email = (await _userManager.FindByNameAsync(Input.Email)).Email;
                }
                else if (await _userManager.FindByEmailAsync(Input.Email) != null)
                {
                    Input.Email = (await _userManager.FindByEmailAsync(Input.Email)).Email;
                }
                else if (_userManager.Users.Any(p => p.PhoneNumber == Input.Email))
                {
                    Input.Email = _userManager.Users.FirstOrDefault(p => p.PhoneNumber == Input.Email).Email;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Input.Email + " user not found in our database, please sign up.");
                    return Page();
                }

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
