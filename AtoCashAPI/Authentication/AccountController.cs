using AtoCashAPI.Data;
using AtoCashAPI.Models;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AtoCashAPI.Authentication
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailSender _emailSender;
        private readonly AtoCashDbContext context;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _config;


        public AccountController(IEmailSender emailSender, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
            AtoCashDbContext context,
             IConfiguration config)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
            _emailSender = emailSender;
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        [ActionName("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userid, string token)
        {
            if (userid == null || token == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "userid or token is invalid" });
            }

            var user = await userManager.FindByIdAsync(userid);

            if (user == null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "User not found!" });
            }

            token = token.Replace("^^^","+");
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return Ok(new RespStatus { Status = "Success", Message = "Thank you for confirming Email!" });
            }

            return Conflict(new RespStatus { Status = "Failure", Message = "Email not confirmed!" });

        }


        // GET: api/<AccountController>
        [HttpPost]
        [ActionName("Register")]
        [Authorize(Roles = "AtominosAdmin, Admin, Manager, Finmgr")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            //check if employee-id is already registered

            var empid = model.EmployeeId;

            bool empIdExists = context.Users.Where(x => x.EmployeeId == model.EmployeeId).Any();

            if (empIdExists)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Employee Id is already taken" });
            }


            //check if email is already in use if yes.. throw error

            var useremail = await userManager.FindByEmailAsync(model.Email);

            if (useremail != null)
            {
                return Conflict(new RespStatus { Status = "Failure", Message = "Email is already taken" });
            }

            MailAddress mailAddress = new(model.Email);

            //MODIFY HOST DOMAIN NAME HERE => CURRENTLY only GMAIL and MAILINATOR
            //if ( mailAddress.Host.ToUpper() != Constants.EmailDomain.ToUpper())
            //{
            //    return Conflict(new RespStatus { Status = "Failure", Message = "Use company mail address!" });
            //}
            //Creating a IdentityUser object
            var user = new ApplicationUser
            {
                EmployeeId = model.EmployeeId,
                UserName = model.Username,
                Email = model.Email,
                PasswordHash = model.Password
            };

            RespStatus respStatus = new();

            try
            {
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                

                if (result.Succeeded)
                {
                    try
                    {


                        // Send Mail ID confirmation email

                        string[] paths = { Directory.GetCurrentDirectory(), "ConfirmEmail.html" };
                        string FilePath = Path.Combine(paths);
                        _logger.LogInformation("Email template path " + FilePath);
                        StreamReader str = new StreamReader(FilePath);
                        string MailText = str.ReadToEnd();
                        str.Close();

                        var domain = _config.GetSection("FrontendDomain").Value;
                        MailText = MailText.Replace("{Domain}", domain);

                        var builder = new MimeKit.BodyBuilder();
                        var receiverEmail = model.Email;
                        string subject = "AtoTax: Confirm your Email Id";

                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        token = token.Replace("+", "^^^");
                        string txtdata = "http://" + domain + "/confirm-email?token=" + token + "&email=" + model.Email;

                        MailText = MailText.Replace("{FrontendDomain}", domain);
                        MailText = MailText.Replace("{ConfirmEmailUrl}", txtdata);


                        builder.HtmlBody = MailText;

                        EmailDto emailDto = new EmailDto();
                        emailDto.To = receiverEmail;
                        emailDto.Subject = subject;
                        emailDto.Body = builder.HtmlBody;

                        await _emailSender.SendEmailAsync(emailDto);
                        _logger.LogInformation("Confirm Email: " + receiverEmail + " Mail ID Confirmation Email Sent for the user!");




                    }
                    catch (Exception ex)
                    {

                        respStatus.Message = "Email confirmation mail not sent";
                        respStatus.Status = "Failure";
                    }




                    //Assigning a User Role to the Registered user
                    result = await userManager.AddToRoleAsync(user, "User");

                    if (result.Succeeded)
                    {
                        respStatus.Message = "User Registered and User Access Role added to Employee!";
                        respStatus.Status = "Success";
                    }
                    else
                    {
                        respStatus.Message = "User Registered but User Access Role not added to Employee!";
                        respStatus.Status = "Failure";
                    }
                }
                else
                {
                    respStatus.Message = "User Name should be Unique!";
                    respStatus.Status = "Failure";
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            

            return Ok(respStatus);
        }


        [HttpPost]
        [ActionName("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            //Creating a IdentityUser object

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Unauthorized(new RespStatus { Status = "Failure", Message = "Username or Password Incorrect" });
            }

            if (user.EmployeeId != 0)
            {
                var emp = context.Employees.Find(user.EmployeeId);

                if (emp == null)
                {
                    return Unauthorized(new RespStatus { Status = "Failure", Message = "Employee Id is Invalid" });
                }
                bool isEmpActive = emp.StatusTypeId == (int)EStatusType.Active;
                if (!isEmpActive)
                {
                    return Unauthorized(new RespStatus { Status = "Failure", Message = "Employee is Inactive" });
                }

            }

            //check if the employee is 1. active or 2.inactive to deny login


            var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);


            //if signin successful send message
            if (result.Succeeded)
            {
                var secretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySecretKey12323232"));

                var signingcredentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

                var modeluser = await userManager.FindByEmailAsync(model.Email);
                var userroles = await userManager.GetRolesAsync(modeluser);
                var empid = user.EmployeeId;
                int currencyId = 0;
                string currencyCode = "";
                string empFirstName = "";
                string empLastName = "";
                string empEmail = "";

                var employee = await context.Employees.FindAsync(empid);

                if (employee != null)
                {
                    empFirstName = employee.FirstName ?? "";
                    empLastName = employee.LastName ?? "";
                    empEmail = employee.Email ?? "";
                    currencyId = employee.CurrencyTypeId ?? 0;
                    currencyCode = context.CurrencyTypes.Find(currencyId).CurrencyCode;
                }


                //add claims
                var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, modeluser.UserName),
                 new Claim(ClaimTypes.Email, model.Email),
                 new Claim("EmployeeId", empid.ToString())

                };
                //add all roles belonging to the user
                foreach (var role in userroles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var tokenOptions = new JwtSecurityToken(

                    issuer: "https://localhost:5001",
                    audience: "https://localhost:5001",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(5),
                     signingCredentials: signingcredentials
                    );

                _logger.LogInformation("Employee " + empid + " loggedin at " + DateTime.UtcNow);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);


                return Ok(new { Token = tokenString, Role = userroles, FirstName = empFirstName, LastName = empLastName, EmpId = empid.ToString(), Email = empEmail, currencyCode, currencyId });
            }

            return Unauthorized(new RespStatus { Status = "Failure", Message = "Username or Password Incorrect" });
        }



        [HttpPost]
        [ActionName("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            //check if employee-id is already registered



            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.email);


                if (user == null)
                {
                    return Ok(new RespStatus { Status = "Failure", Message = "User Not found!" });
                }
                //bool isUserConfirmed = await userManager.IsEmailConfirmedAsync(user);
                //if (user != null && isUserConfirmed)

                if (user != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    //var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var passwordResetlink= Url.Action("ResetPassword", "Account", new { email = model.email, token = token, Request.Scheme });

                    //return Ok(passwordResetLink);



                    token = token.Replace("+", "^^^");




                    ////get password ResetURL from environment variable to send password reset email
                    //string? PasswordResetRedirectDomain = Environment.GetEnvironmentVariable("PasswordResetDomain");

                    //if(! String.IsNullOrEmpty(PasswordResetRedirectDomain))
                    //{
                    //    redirectDomain = PasswordResetRedirectDomain;

                    //}
                    //else
                    //{

                    //    if (domain == "foodunitco.com" || domain == "signsa.com" || domain == "foodunitco.onmicrosoft.com"
                    //        || domain == "2eat.com.sa" || domain == "2eat.sa" || domain == "alzadalyawmi.com"
                    //        || domain == "estilo.sa" || domain == "foodunit.uk" || domain == "dhyoof.com"
                    //        || domain == "janburger.com" || domain == "luluatnajd.com" || domain == "shawarma-plus.com"
                    //        || domain == "shawarmaplus.sa" || domain == "signsa.com" || domain == "tameesa.com"
                    //        || domain == "tameesa.com.sa" || domain == "thouq.sa" || domain == "tameesa.com" || domain == "gmail.com"
                    //        )
                    //    {
                    //        redirectDomain = "fw";
                    //    }
                    //    else
                    //    {
                    //        redirectDomain = domain.Split('.')[0];
                    //    }


                    //    //console.log('redirectDomain=='+redirectDomain);

                    //    domain = "https://" + redirectDomain + ".atocash.com/change-password?token=";
                    //}









                    //  string[] paths = { Directory.GetCurrentDirectory(),
                    string[] paths = { Directory.GetCurrentDirectory(), "PasswordReset.html" };
                    string FilePath = Path.Combine(paths);
                    _logger.LogInformation("Email template path " + FilePath);
                    StreamReader str = new StreamReader(FilePath);
                    string MailText = str.ReadToEnd();
                    str.Close();

                    var domain = _config.GetSection("FrontendDomain").Value;
                    MailText = MailText.Replace("{FrontendDomain}", domain);

                    var builder = new MimeKit.BodyBuilder();
                    var receiverEmail = model.email;
                    string subject = "Password Reset Link";
                    string txtdata = "https://" + domain + "/change-password?token=" + token + "&email=" + model.email;

                    MailText = MailText.Replace("{PasswordResetUrl}", txtdata);

                    builder.HtmlBody = MailText;

                    EmailDto emailDto = new EmailDto();
                    emailDto.To = receiverEmail;
                    emailDto.Subject = subject;
                    emailDto.Body = builder.HtmlBody;


                    await _emailSender.SendEmailAsync(emailDto);
                    _logger.LogInformation("ForgotPassword: " + receiverEmail + "Password Reset Email Sent with token");

                }

                return Ok(new RespStatus { Status = "Success", Message = "Password Reset Email Sent!" });
            }
            return Conflict(new RespStatus { Status = "Failure", Message = "Input params invalid" });
        }




        [HttpPost]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.email);

                if (user != null)
                {

                    model.Token = model.Token.Replace("^^^", "+");
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        var receiverEmail = model.email;
                        string subject = "Password Changed";
                        string body = "Your new Password is:" + model.Password;

                        EmailDto emailDto = new EmailDto();
                        emailDto.To = receiverEmail;
                        emailDto.Subject = subject;
                        emailDto.Body = body;

                        await _emailSender.SendEmailAsync(emailDto);
                        _logger.LogInformation("Password Reset: " + receiverEmail + "Password has been Reset");
                        return Ok(new RespStatus { Status = "Success", Message = "Your Password has been reset!" });
                    }

                    List<object> errResp = new();
                    foreach (var error in result.Errors)
                    {
                        errResp.Add(error.Description);
                    }
                    return Ok(errResp);
                }

                return Conflict(new RespStatus { Status = "Failure", Message = "User is invalid" });

            }

            return Conflict(new RespStatus { Status = "Failure", Message = "Model state is invalid" });

        }




        ////


    }
}