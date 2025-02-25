using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,

        ITokenService tokenService, IMapper mapper, IEmailSender emailSender,
        IUnitOfWork unitOfWork)
        {
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            if (await UserExists(registerDto.KnownAs)) return BadRequest("KnownAs is taken");
            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var param = new Dictionary<string, string>
        {
            {"token", token },
            {"email", user.Email }
        };
            var callback = QueryHelpers.AddQueryString(registerDto.ClientURI, param);
            var userName = user.UserName;
            var html = new StringBuilder();
            html.Append(string.Format("<table width='60%' align='center' cellspacing='0' cellpadding='0' bgcolor='#FFFFFF' height='120px'>" +
                            "<tbody><tr><td align='center' bgcolor='#30b8e9' width='100%' valign='middle'>" +
                            "<img src='https://localhost:4200/assets/images/headermail.jpg'></td></tr></tbody></table>" +
                            "<table align='center' width='60%' valign='top'><tbody><tr><td style='padding-top:2rem'><p>Dear {0},</p>" +
                            "</td></tr><tr><td><p>Thank you for signing up with Rainobu!" +
                            "Cheers to the start of our new adventure!! To verify your email please click on the button down below.</p>" +
                            "</td></tr><tr><td> <p style='text-align:center;'>" + "<br/><br/>" +
                            "<a href='{1}' style='border:none;background-color:#30b8e9;padding:10px 15px;border-radius:10px;text-decoration: none;color:white;'>Verify Now</a>" +
                            "</p><br/><br/></td></tr>" +
                            "<tr><td>Best Wishes,<br/>Rainobu Team </td></tr> </tbody></table>", userName, callback));

            var message = new MailMessage(new string[] { user.Email }, "Rainobu Email Verification", html.ToString(), null);


            await _emailSender.SendEmailAsync(message);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);
            var ActiveType = await _unitOfWork.TitleRepository.GetTitleName(ActivitiesType.FirstRegister);
            if (ActiveType != null)
            {
                var getTitle = new TitleActive
                {
                    AppUserId = user.Id,
                    AppUser = user,
                    TitleNameId = ActiveType.Id,
                    TitleName = ActiveType,
                    Name = ActiveType.Name,
                    IsMain = true,
                    Type = ActivitiesType.FirstRegister
                };
                user.titleAcitive.Add(getTitle);
                if (!await _unitOfWork.Complete()) return BadRequest("Problem Add Title");
            }
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = new AppUser();
            if (loginDto.Username.Contains("@"))
            {
                user = await _userManager.Users
               .Include(p => p.Photos)
               .Include(l => l.LikedStoryByUsers)
               .Include(v => v.VipUsers)
               .SingleOrDefaultAsync(x => x.Email == loginDto.Username.ToLower());
            }
            else
            {
                user = await _userManager.Users
               .Include(p => p.Photos)
               .Include(l => l.LikedStoryByUsers)
               .Include(v => v.VipUsers)
               .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            }

            // var user = await _userManager.FindByNameAsync(userForAuthentication.Email);
            if (user == null) return Unauthorized("Invalid username");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Email is not confirmed");

            // if (!await _userManager.IsEmailConfirmedAsync(user))
            //     return Unauthorized(new AuthResponseDto { ErrorMessage = "Email is not confirmed" });

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid Password");
            var isVIP = await _userManager.IsInRoleAsync(user, "VIP");
            if (isVIP)
            {
                // DateTime expired = user.VipUsers.Max(x => x.ExpiredDate);
                DateTime expired = user.VipUsers
                        .Any() ? user.VipUsers.Max(x => x.ExpiredDate) : DateTime.UtcNow.AddDays(1);
                DateTime current = DateTime.UtcNow;
                int resualtDate = DateTime.Compare(expired, current);
                if (resualtDate < 0)
                {
                    var resultRemoveRole = await _userManager.RemoveFromRoleAsync(user, "VIP");
                    if (!resultRemoveRole.Succeeded) return BadRequest("Failed to remove from roles");
                }
            }
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;
            await _unitOfWork.Complete();
                        //save refreshtoken to db

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Point = user.Point,
                Title = user.Title,
                RefreshToken = refreshToken.Token
            };
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
                return BadRequest("Invalid Request");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string>
        {
            {"token", token },
            {"email", forgotPasswordDto.Email }
        };

            var callback = QueryHelpers.AddQueryString(forgotPasswordDto.ClientURI, param);

            var message = new MailMessage
            (new string[] { user.Email }, "Rainobu Password Reset",
            $"Please reset your password by clicking this: <a href='{callback}'>link</a>",
             null);
            await _emailSender.SendEmailAsync(message);

            return Ok();
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return BadRequest("Invalid Request");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (!resetPassResult.Succeeded)
            {
                var errors = resetPassResult.Errors.Select(e => e.Description);

                return BadRequest(new { Errors = errors });
            }

            await _userManager.SetLockoutEndDateAsync(user, new DateTime(2000, 1, 1));

            return Ok();
        }
        [HttpGet("EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromQuery] string email, [FromQuery] string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest("Invalid Email Confirmation Request");

            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
                return BadRequest("Invalid Email Confirmation Request");

            return Ok();
        }
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("ExternalLogin")]
        public async Task<ActionResult<UserDto>> ExternalLogin([FromBody] ExternalAuthDto externalAuth)
        {
            var payload = await _tokenService.VerifyGoogleToken(externalAuth);
            if (payload == null)
                return BadRequest("Invalid External Authentication.");
            var info = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    var photo = new Photo
                    {
                        Url = payload.Picture,
                        IsMain = true
                    };
                    user = new AppUser
                    {
                        Email = payload.Email,
                        UserName = payload.GivenName,
                        //DateOfBirth = DateTime.MinValue,
                        KnownAs = payload.GivenName,
                        // Photos = new Photo {
                        //     Url= payload.Picture,
                        //     IsMain = true
                        // }
                        Photos = { photo }
                    };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "Member");
                    var ActiveType = await _unitOfWork.TitleRepository.GetTitleName(ActivitiesType.FirstRegister);
                    var getTitle = new TitleActive
                    {
                        AppUserId = user.Id,
                        AppUser = user,
                        TitleNameId = ActiveType.Id,
                        TitleName = ActiveType,
                        Name = ActiveType.Name,
                        IsMain = true,
                        Type = ActivitiesType.FirstRegister
                    };
                    user.titleAcitive.Add(getTitle);
                    if (!await _unitOfWork.Complete()) return BadRequest("Problem Add Title");
                    //prepare and send an email for the email confirmation
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }
            }
            if (user == null)
                return BadRequest("Invalid External Authentication.");
            //check for the Locked out account
            //var token = await _tokenService.CreateToken(user);
            var main = await _userManager.Users
                                .Include(p => p.Photos)
                                .Include(l => l.LikedStoryByUsers)
                                // .Include(r => r.recievePoints)
                                // .Include(t => t.titleAcitive)
                                //     .ThenInclude(t => t.TitleName)
                                .Include(v => v.VipUsers)
                                .SingleOrDefaultAsync(x => x.Id == user.Id);
            //get role
            // var userRoles = await _userManager.GetRolesAsync(user);
            var isVIP = await _userManager.IsInRoleAsync(user, "VIP");
            if (isVIP)
            {
                DateTime expired = user.VipUsers.Max(x => x.ExpiredDate);
                DateTime current = DateTime.Now;
                int resualtDate = DateTime.Compare(expired, current);
                if (resualtDate < 0)
                {
                    var resultRemoveRole = await _userManager.RemoveFromRoleAsync(user, "VIP");
                    if (!resultRemoveRole.Succeeded) return BadRequest("Failed to remove from roles");
                }
            }

            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiryTime = refreshToken.Expires;
            await _unitOfWork.Complete();
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = main.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Point = user.Point,
                Title = user.Title,
                RefreshToken = refreshToken.Token
            };
        }

        // [Route("Savesresponse")]    
        // [HttpPost]    
        // public object Savesresponse(Users user)    
        // {    
        //     try    
        //     {    
        //         SocialLoginEntities DB = new SocialLoginEntities();    
        //         Socaillogin Social= new Socaillogin();    
        //         if (Social.TId == 0)    
        //         {    
        //             Social.name = user.name;    
        //             Social.email = user.email;    
        //             Social.provideid = user.provideid;    
        //             Social.provider = user.provider;    
        //             Social.image = user.image;    
        //             Social.token = user.token;    
        //             Social.idToken = user.idToken;    
        //             var a=  DB.Socaillogins.Add(Social);    
        //             DB.SaveChanges();    
        //             return a;    
        //         }    
        //     }    
        //     catch (Exception)    
        //     {    

        //         throw;    
        //     }    
        //     return new Response    
        //     { Status = "Error", Message = "Invalid Data." };    
        // }    

    }
}