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
            int userId = 28;

            RestRequest userRequest = new RestRequest("api/users/" + userId.ToString());

            try
            {
                RestResponse userResponse = await userClient.ExecuteGetAsync(userRequest);

                if (!userResponse.IsSuccessful ||
                    string.IsNullOrEmpty(userResponse.Content))
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
            int userId = 28; // replace this with login

            RestRequest getRequest = new RestRequest("api/users/" + userId);
            RestResponse getResponse = await userClient.ExecuteGetAsync(getRequest);

            if (!getResponse.IsSuccessful || string.IsNullOrEmpty(getResponse.Content))
                return StatusCode(502, new { message = "Could not retrieve user." });

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
                return StatusCode(502, new { message = "Update failed." });

            return Json(new { message = "Profile updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest passwordRequest)
        {
            int userId = 28; // implement login here

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
                return BadRequest(new { message = "unable to change password" });

            return Json(new { message = "password changed successfully." });
        }
    }
}
