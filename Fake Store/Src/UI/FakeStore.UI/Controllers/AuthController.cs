using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Entities;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;

namespace FakeStoreUI.Controllers
{
    [Route("[Controller]")]
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllUsers()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5035/api/Auth/getAllUsers")

            };

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            string serilizedUsers = await response.Content.ReadAsStringAsync();
            List<User>? users = JsonConvert.DeserializeObject<List<User>>(serilizedUsers);
            return View(users);
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> Signup()
        {
            return View(new UserRegistrationRequest());
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Signup(UserRegistrationRequest registrationRequest)
        {
            string fileName = "";
            if (!ModelState.IsValid)
            {
                ViewBag.Message = 0;
                return View(registrationRequest);
            }
            HttpClient client = _httpClientFactory.CreateClient();
            // For saving image ( if exists )
            if (registrationRequest.Image != null)
            {
                if (registrationRequest.Image.Length > 0)
                {
                    var formData = new MultipartFormDataContent();
                    formData.Add(new StreamContent(registrationRequest.Image.OpenReadStream()), "ImageFile", registrationRequest.Image.FileName.Trim().Replace(" ", ""));
                    HttpResponseMessage imageSaveResponse = await client.PostAsync("http://localhost:5035/api/Auth/SaveUserProfileImage", formData);

                    fileName = await imageSaveResponse.Content.ReadAsStringAsync();
                }
            }

            // Hasing the password
            registrationRequest.Password =  HelperClasses.HashHelper.HashPassword(registrationRequest.Password);
            User user = ConvertToUser.ToUser(registrationRequest, fileName);

            // For saving the use information
            string userJsonContent = System.Text.Json.JsonSerializer.Serialize(user);
            HttpContent content = new StringContent(userJsonContent, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("http://localhost:5035/api/Auth/UserAdd", content);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Message = 0;
                return View(registrationRequest);
            }
            ViewBag.Message = 1;
            return View(registrationRequest);
        }

        [HttpGet]
        [Route("[Action]")]

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> Signin(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View(new LoginDTO());
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Signin(LoginDTO loginCredentials, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = 0;
                return View(loginCredentials);
            }
            HttpClient client = _httpClientFactory.CreateClient();
            loginCredentials.Password = HelperClasses.HashHelper.HashPassword(loginCredentials.Password);
            string creadentialsSerialized = JsonConvert.SerializeObject(loginCredentials);

            HttpContent content = new StringContent(creadentialsSerialized, System.Text.Encoding.UTF8, "application/json");
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:5035/api/Auth/VerifyUser"),
                Content = content
            };

            HttpResponseMessage response = await client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode) return StatusCode(500);

            if (response.Content == null) return StatusCode(500);
            string responseContent = await response.Content.ReadAsStringAsync();

            //Tuple<int,int?, string, string, string> : isValidUser, Id, Role, Name, ImageName
            Tuple<int,int?, string,string,string>? responseTuple = JsonConvert.DeserializeObject<Tuple<int, int?,string,string, string>>(responseContent);

            if (responseTuple == null)
            {
                ViewBag.Message = 0;
                return View(loginCredentials);
            }
            else if (responseTuple.Item1 == 0 || responseTuple.Item2 == null)
            {
                ViewBag.Message = 0;
                return View(loginCredentials);
            }

            if (HttpContext.Request.Cookies.ContainsKey("AuthenticationCookie"))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,loginCredentials.EmailOrPhone),
                    new Claim(ClaimTypes.Role,responseTuple.Item3 ),

                    new Claim("UserID",responseTuple.Item2?.ToString()),
                    new Claim("UserName",responseTuple.Item4),  // For userName
                    new Claim("ImageName",responseTuple.Item5) // for UserImage
                };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principle = new ClaimsPrincipal(identity);

            AuthenticationProperties props;
            if (loginCredentials.RememberMe)
            {
                props = new AuthenticationProperties()
                {
                    //AllowRefresh = <bool>,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    IsPersistent = true
                    //IssuedUtc = <DateTimeOffset>,
                    //RedirectUri = <string>
                };
            }
            else
            {
                props = new AuthenticationProperties()
                {
                    //AllowRefresh = <bool>,
                    //ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    IsPersistent = false
                    //IssuedUtc = <DateTimeOffset>,
                    //RedirectUri = <string>

                };
            }
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle, props);
            return Redirect(ReturnUrl == null ? "/Products/AllProducts" : ReturnUrl);

        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("~/Products/AllProducts");
        }
    }
}
