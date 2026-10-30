using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly RestClient userClient;

        public UserProfileController(IConfiguration configuration)
        {
            string url = configuration["BankApi:BaseUrl"]!;
            userClient = new RestClient(url);
        }

        [HttpGet("/UserProfile")]
        public async Task<IActionResult> GetUserProfile()
        { 
            int userId = 25;

            RestRequest userRequest = new RestRequest("api/users/" + userId.ToString());

            try
            {
                RestResponse userResponse = await userClient.ExecuteGetAsync(userRequest);

                if (!userResponse.IsSuccessful || userResponse.Content == null)
                {
                    ViewBag.Error = "could not retrieve user";
                    return View("UserProfile", new User());
                }

                User? user = JsonConvert.DeserializeObject<User>(userResponse.Content);
                return View("UserProfile", user ?? new User());
            }
            catch (Exception)
            {
                ViewBag.Error = "unable to connect to server";
                return View("UserProfile", new User());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] User updatedUser)
        {
            int userId = 25; // replace this with login

            RestRequest getRequest = new RestRequest("api/users/" + userId);
            RestResponse getResponse = await userClient.ExecuteGetAsync(getRequest);

            if (!getResponse.IsSuccessful || string.IsNullOrEmpty(getResponse.Content))
            {
                return StatusCode(502, new { message = "Could not retrieve user." });
            }
            User? user = JsonConvert.DeserializeObject<User>(getResponse.Content);

            if (user == null)
                return StatusCode(502, new { message = "Invalid user data." });

            RestRequest request = new RestRequest("api/users/" + Uri.EscapeDataString(user.UserName));

            request.AddJsonBody(new
            {
                Name = user.Name,
                UserEmail = updatedUser.UserEmail,
                UserAddress = updatedUser.UserAddress,
                UserPhone = updatedUser.UserPhone
            });

            RestResponse response = await userClient.ExecutePutAsync(request);

            if (!response.IsSuccessful)
            {
                return StatusCode(502, new { message = "Update failed." });
            }
            return Json(new { message = "Profile updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordRequest)
        {
            int userId = 25; // implement login here

            RestRequest getRequest = new RestRequest("api/users/" + userId);
            RestResponse getResponse = await userClient.ExecuteGetAsync(getRequest);

            if (!getResponse.IsSuccessful || string.IsNullOrEmpty(getResponse.Content))
                return StatusCode(502, new { message = "could not retrieve user" });

            User? user = JsonConvert.DeserializeObject<User>(getResponse.Content);

            if (user == null)
                return StatusCode(502, new { message = "invalid user data" });

            RestRequest request = new RestRequest("api/users/" + Uri.EscapeDataString(user.UserName) + "/password");

            request.AddJsonBody(passwordRequest);

            RestResponse response = await userClient.ExecutePutAsync(request);

            if (!response.IsSuccessful)
            {
                return BadRequest(new { message = "unable to change password" });
            }
            return Json(new { message = "password changed successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetProfilePicture()
        {
            int userId = 25;

            var userResponse = await userClient.ExecuteGetAsync(new RestRequest($"api/users/{userId}"));

            var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

            if (user == null)
            {
                return NotFound();
            }
            var pfpResponse = await userClient.ExecuteGetAsync(new RestRequest($"api/users/{user.UserName}/pfp"));

            if (!pfpResponse.IsSuccessful || pfpResponse.RawBytes == null || pfpResponse.ContentType == null)
            {
                return NotFound();
            }
            string contentType = pfpResponse.ContentType;

            return File(pfpResponse.RawBytes, contentType);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile pfp)
        {
            int userId = 25; // Login implement here

            var userResponse = await userClient.ExecuteGetAsync(new RestRequest($"api/users/{userId}"));

            var user = JsonConvert.DeserializeObject<User>(userResponse.Content!);

            var pfpRequest = new RestRequest($"api/users/{user!.UserName}/pfp", Method.Put);

            var pfpFile = new MemoryStream();

            try
            {
                await pfp.CopyToAsync(pfpFile);

                pfpRequest.AddFile("picture", pfpFile.ToArray(), pfp.FileName, pfp.ContentType);
            }
            finally
            {
                pfpFile.Close();
            }

            var response = await userClient.ExecuteAsync(pfpRequest);

            if (!response.IsSuccessful)
            {
                return BadRequest("Upload failed.");
            }
            return Ok("pfp uploaded");
        }
    }
}
