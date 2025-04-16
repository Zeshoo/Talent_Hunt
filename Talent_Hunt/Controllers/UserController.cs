using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Talent_Hunt.Models;

namespace Talent_Hunt.Controllers
{
    public class UserController : Controller
    {
        private readonly string apiUrl = "http://localhost/Apidemo/api/Main/getUser";
       // private readonly string apiUrlImages = "http://localhost/apidemo/api/Images";

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string email, string password)
        {
            List<User> users = new List<User>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<User>>(jsonData);
                }
            }

            // Check if user exists in the list
            User loggedInUser = users.Find(u => u.Email == email && u.Password == password);

            if (loggedInUser != null)
            {
                // Store user session
                Session["UserID"] = loggedInUser.UserID;
                Session["UserRole"] = loggedInUser.Role;
                Session["UserName"] = loggedInUser.Name;

                // Redirect based on role
                if (loggedInUser.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (loggedInUser.Role == "Student")
                {
                    return RedirectToAction("Index", "Student");
                }
                else if (loggedInUser.Role == "Committee")
                {
                    return RedirectToAction("Index", "CommitteeMember");
                }
                else
                {
                    Console.WriteLine("User Not Found: " );
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }
    }

    // Model for API Response
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Ensure this is securely hashed in real-world applications
        public string Role { get; set; }
    }
}
