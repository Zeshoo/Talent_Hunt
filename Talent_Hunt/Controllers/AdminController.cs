using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


using Talent_Hunt.Models;
//using System.Net.Http.Formatting;

namespace Talent_Hunt.Controllers
{
    public class AdminController : Controller
    {
        private Talent_HuntEntities3 db = new Talent_HuntEntities3();
        private readonly string SignUpApi = "http://localhost/TalentHunt1/api/Main/Signup";
        private readonly string getUserApi = "http://localhost/TalentHunt1/api/Main/getUser";
        private static readonly HttpClient apiClient = new HttpClient();
        private readonly string apiUrl = "http://localhost/TalentHunt1/api/Main/ShowAllEvents";
        //private readonly string ShowTask = "http://localhost/TalentHunt1/api/Main/ShowTask";
        private readonly string createEventApiUrl = "http://localhost/TalentHunt1/api/Main/CreateEvent";
        //private readonly string addRulesApiUrl = "http://localhost/TalentHunt1/api/Main/AddRules";
        private readonly string assignMemberApiUrl = "http://localhost/TalentHunt1/api/Main/AssignedMembersToEvent";
        private readonly string addCommitteeMemberApiUrl = "http://localhost/TalentHunt1/api/Main/AddCommitteeMember";
        private readonly string assignApiUrl = "http://localhost/TalentHunt1/api/Main/AssignedMemberToEvent";
        private readonly string ApplyApi = "http://localhost/TalentHunt1/api/Main/Apply";
        private readonly string ApplicationView = "http://localhost/TalentHunt1/api/Main/ViewApplications";
        private readonly HttpClient client = new HttpClient();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Signup(Users user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid input.";
                return View(user);
            }

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(SignUpApi, content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Signup successful!";
                ModelState.Clear();

                // After successful signup, redirect to Login page
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ViewBag.Message = "Error: " + errorMessage;
                return View(user);
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string email, string password)
        {
            List<Users> users = new List<Users>();  // Use the Users model for deserialization

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(getUserApi);  // Use your actual API URL

                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<Users>>(jsonData);  // Deserialize to Users model
                }
            }

            // Check if user exists in the list
            Users loggedInUser = users.Find(u => u.Email == email && u.Password == password);  // Use Users model

            if (loggedInUser != null)
            {
                // Store user session
                Session["UserId"] = loggedInUser.Id; // Store logged-in user's Id
                int userId = Convert.ToInt32(Session["UserId"]);

                Session["UserRole"] = loggedInUser.Role;
                Session["UserName"] = loggedInUser.Name;

                // Redirect based on role
                if (loggedInUser.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (loggedInUser.Role == "Student")
                {
                    return RedirectToAction("StudentDashBoard", "Admin");
                }
                else if (loggedInUser.Role == "Committee")
                {
                    return RedirectToAction("CommitteeDashBoard", "Admin");
                }
                else
                {
                    Console.WriteLine("User Not Found: ");
                }
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> DashBoard()
        {
            List<Event> events = new List<Event>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    events = JsonConvert.DeserializeObject<List<Event>>(data);
                }
            }
            return View(events);
        }
        [HttpGet]
        public async Task<ActionResult> EventDetails(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/ShowAllEvents");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var events = JsonConvert.DeserializeObject<List<Event>>(data);

                        // Find the event with the given id
                        var eventItem = events.FirstOrDefault(e => e.Id == id);

                        if (eventItem != null)
                        {
                            return View(eventItem); // Pass event details to view
                        }
                        else
                        {
                            return HttpNotFound("Event not found."); // Event not found
                        }
                    }
                    else
                    {
                        return HttpNotFound("Error retrieving events.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet]
        public async Task<ActionResult> CommitteeDashBoard()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                // Use userId here
            }

            List<Event> events = new List<Event>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    events = JsonConvert.DeserializeObject<List<Event>>(data);
                }
            }
            return View(events);
        }
        [HttpGet]
        public async Task<ActionResult> StudentDashBoard()
        {
            List<Event> events = new List<Event>();
            string userRole = Session["UserRole"] as string;
            string userName = Session["UserName"] as string;
            int userId = Convert.ToInt32(Session["UserId"]);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    events = JsonConvert.DeserializeObject<List<Event>>(data);
                }
            }

            // Return the view and pass the list of events to it
            return View(events);
        }
        [HttpGet]
        public async Task<ActionResult> StudentEventDetails(int id)
        {
            try
            {
                string userRole = Session["UserRole"] as string;
                string userName = Session["UserName"] as string;
                int userId = Convert.ToInt32(Session["UserId"]);
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/ShowAllEvents");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var events = JsonConvert.DeserializeObject<List<Event>>(data);

                        // Find the event with the given id
                        var eventItem = events.FirstOrDefault(e => e.Id == id);

                        if (eventItem != null)
                        {
                            return View(eventItem); // Pass event details to view
                        }
                        else
                        {
                            return HttpNotFound("Event not found."); // Event not found
                        }
                    }
                    else
                    {
                        return HttpNotFound("Error retrieving events.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> StudentEventDetailsafterstatusupdate(int id)
        {
            try
            {
                string userRole = Session["UserRole"] as string;
                string userName = Session["UserName"] as string;
                int userId = Convert.ToInt32(Session["UserId"]);
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/ShowAllEvents");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var events = JsonConvert.DeserializeObject<List<Event>>(data);

                        // Find the event with the given id
                        var eventItem = events.FirstOrDefault(e => e.Id == id);

                        if (eventItem != null)
                        {
                            return View(eventItem); // Pass event details to view
                        }
                        else
                        {
                            return HttpNotFound("Event not found."); // Event not found
                        }
                    }
                    else
                    {
                        return HttpNotFound("Error retrieving events.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Apply(int UserId, int EventId)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
        {
            { "UserId", UserId.ToString() },
            { "EventId", EventId.ToString() }
        };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(ApplyApi, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = result;
                }
                else
                {
                    TempData["Message"] = await response.Content.ReadAsStringAsync();
                }

                return RedirectToAction("StudentDashBoard");
            }
        }

        [HttpGet]
        public async Task<ActionResult> ViewApplications(int eventId)
        {
            List<Apply> applications = new List<Apply>();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{ApplicationView}?eventId={eventId}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    applications = JsonConvert.DeserializeObject<List<Apply>>(data);
                }
                else
                {
                    TempData["Error"] = await response.Content.ReadAsStringAsync();
                }
            }

            ViewBag.EventId = eventId;
            return View(applications);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateApplicationStatus(int ApplyId, string newStatus)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
        {
            { "applyId", ApplyId.ToString() },
            { "newStatus", newStatus }
        };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("http://localhost/TalentHunt1/api/Main/UpdateApplicationStatus", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = $"Application updated successfully.";
                }
                else
                {
                    TempData["Error"] = await response.Content.ReadAsStringAsync();
                }
            }

            // Redirect back to applications list or dashboard
            return RedirectToAction("DashBoard");
        }

        [HttpGet]
        public async Task<ActionResult> ShowTasks(int eventid)
        {
            List<Talent_Hunt.Models.Task> tasks = new List<Talent_Hunt.Models.Task>();

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"http://localhost/TalentHunt1/api/Main/ShowTask?eventid={eventid}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Talent_Hunt.Models.Task>>(data);
                }
                else
                {
                    // Optionally handle error (e.g., ViewBag.Message = "Failed to load tasks")
                }
            }

            return View(tasks);
        }
        [HttpGet]
        public async Task<ActionResult> ShowTasksForStudents(int eventid)
        {
            List<Talent_Hunt.Models.Task> tasks = new List<Talent_Hunt.Models.Task>();

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"http://localhost/TalentHunt1/api/Main/ShowTask?eventid={eventid}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Talent_Hunt.Models.Task>>(data);
                }
                else
                {
                    // Optionally handle error (e.g., ViewBag.Message = "Failed to load tasks")
                }
            }

            return View(tasks);
        }
        [HttpPost]
        public async Task<ActionResult> SubmitTask(HttpPostedFileBase file, Submission submission)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost/TalentHunt1/"); // Replace with your actual API base URL

                    using (var content = new MultipartFormDataContent())
                    {
                        // Add file content
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileStreamContent = new StreamContent(file.InputStream);
                            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "\"file\"",
                                FileName = "\"" + file.FileName + "\""
                            };
                            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                            content.Add(fileStreamContent);
                        }

                        // Add form data as JSON
                        string submitJson = JsonConvert.SerializeObject(submission);
                        content.Add(new StringContent(submitJson), "submitInfo");

                        // Send POST request
                        var response = await client.PostAsync("api/Main/SubmitTask", content); // Replace with correct route
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            ViewBag.Message = "Task submitted successfully: " + result;
                        }
                        else
                        {
                            ViewBag.Message = "Error: " + response.ReasonPhrase;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Exception: " + ex.Message;
            }

            return View(); // Return to same view or redirect as needed
        }



        [HttpGet]
        public ActionResult CreateEvent()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateEvent(HttpPostedFileBase eventImage, string eventTitle, string description,
                 string regStartDate, string regEndDate, string eventDate, string startTime, string endTime)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var formData = new MultipartFormDataContent();

                    // Add JSON data
                    var eventData = new
                    {
                        Title = eventTitle,
                        Details = description,
                        RegStartDate = regStartDate,
                        RegEndDate = regEndDate,
                        EventDate = eventDate,
                        EventStartTime = startTime,
                        EventEndTime = endTime
                    };

                    string jsonContent = JsonConvert.SerializeObject(eventData);
                    formData.Add(new StringContent(jsonContent, Encoding.UTF8, "application/json"), "submitInfo");

                    // Add Image if present
                    if (eventImage != null)
                    {
                        var streamContent = new StreamContent(eventImage.InputStream);
                        formData.Add(streamContent, "eventImage", eventImage.FileName);
                    }

                    // Call API
                    HttpResponseMessage response = await client.PostAsync(createEventApiUrl, formData);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ✅ Log full API response
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["Error"] = $"Error creating event. Status: {response.StatusCode}, API Response: {responseBody}";
                        return RedirectToAction("CreateEvent");
                    }

                    // Deserialize event ID
                    var createdEvent = JsonConvert.DeserializeObject<Event>(responseBody);
                    int createdEventId = createdEvent.Id;  

                    TempData["Success"] = "Event created successfully ";
                    return RedirectToAction("CreateEvent");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Exception occurred: " + ex.Message;
                return RedirectToAction("CreateEvent");
            }
        }



        [HttpGet]
        public ActionResult Report()
        {
            return View();
        }

        private readonly string showEventsApiUrl = "http://localhost/TalentHunt1/api/Main/ShowAllEvents";
        private readonly string showTasksApiUrl = "http://localhost/TalentHunt1/api/Main/ShowTask"; // NEW LINE
        private readonly string saveTaskApiUrl = "http://localhost/TalentHunt1/api/Main/CreateTask"; // FIXED: corrected case

        // GET: CreateTask
        // Action to display the form
        //public ActionResult CreateTask()
        //{
        //    return View();
        //}

        // Action to handle the form submission




        // GET: Admin/CreateTask
        // GET: Admin/CreateTask
        public ActionResult CreateTask(int? eventId)
        {
            // Ensure eventId is passed in the URL
            if (eventId == null)
            {
                TempData["ErrorMessage"] = "Event ID is required.";
                return RedirectToAction("DashBoard"); // Redirect to the Index page if eventId is missing
            }

            // Initialize TaskDto with EventID from URL
            var model = new TaskDto
            {
                EventID = eventId.Value // Set EventID to the one passed in URL
            };

            return View(model); // Pass the model to the view
        }

        // POST: Admin/CreateTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTask(TaskDto model, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Send data to API using MultipartFormDataContent instead of JSON
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost/TalentHunt1/");

                        // Create a multipart form data content
                        using (var formData = new MultipartFormDataContent())
                        {
                            // Add form fields that match exactly what the API expects
                            formData.Add(new StringContent(model.EventID.ToString()), "EventID");
                            formData.Add(new StringContent(model.TaskStartTime), "TaskStartTime");
                            formData.Add(new StringContent(model.TaskEndTime), "TaskEndTime");
                            formData.Add(new StringContent(model.Description), "Description");

                            // Handle image upload
                            if (Image != null && Image.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(Image.FileName);
                                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);

                                // Create a memory stream to hold the image data
                                using (var memoryStream = new MemoryStream())
                                {
                                    Image.InputStream.CopyTo(memoryStream);
                                    memoryStream.Position = 0;

                                    // Add the image to the form data
                                    var imageContent = new ByteArrayContent(memoryStream.ToArray());
                                    imageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                    {
                                        Name = "\"file\"",
                                        FileName = "\"" + uniqueFileName + "\""
                                    };
                                    imageContent.Headers.ContentType = new MediaTypeHeaderValue(Image.ContentType);
                                    formData.Add(imageContent);
                                }
                            }

                            // Send the request
                            var response = await client.PostAsync("api/Main/CreateTask", formData);

                            if (response.IsSuccessStatusCode)
                            {
                                TempData["SuccessMessage"] = "Task created successfully.";
                                return RedirectToAction("DashBoard");
                            }
                            else
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                TempData["ErrorMessage"] = $"API error: {response.ReasonPhrase} - {errorContent}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Exception occurred: " + ex.Message;
                }
            }
            else
            {
                // Add error messages from ModelState
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        ModelState.AddModelError("", error.ErrorMessage);
                    }
                }
            }

            TempData["ErrorMessage"] = "Failed to create task.";
            return View(model);
        }


        

        [HttpGet]
        public async Task<ActionResult> CommitteeEventDetails(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/ShowAllEvents");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var events = JsonConvert.DeserializeObject<List<Event>>(data);

                        // Find the event with the given id
                        var eventItem = events.FirstOrDefault(e => e.Id == id);

                        if (eventItem != null)
                        {
                            return View(eventItem); // Pass event details to view
                        }
                        else
                        {
                            return HttpNotFound("Event not found."); // Event not found
                        }
                    }
                    else
                    {
                        return HttpNotFound("Error retrieving events.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }




        [HttpPost]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                var payload = new { EventId = id };
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("http://localhost/TalentHunt1/api/Main/DeleteEvent", content);
                    if (response.IsSuccessStatusCode)
                    {
                        // ✅ After successful deletion, get the updated list of events
                        HttpResponseMessage getEvents = await client.GetAsync("http://localhost/TalentHunt1/api/Main/ShowAllEvents");

                        if (getEvents.IsSuccessStatusCode)
                        {
                            string data = await getEvents.Content.ReadAsStringAsync();
                            var eventList = JsonConvert.DeserializeObject<List<Event>>(data);

                            // ✅ Show only event titles in view
                            return RedirectToAction("DashBoard"); // Or another existing view name that shows the list

                        }
                        else
                        {
                            TempData["Error"] = "Event deleted, but failed to fetch updated list.";
                            return RedirectToAction("DashBoard");
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Failed to delete event.";
                        return RedirectToAction("EventDetails", new { id });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Exception: " + ex.Message;
                return RedirectToAction("EventDetails", new { id });
            }
        }

        [HttpPost]
        public ActionResult EditEvent(int id)
        {
            var eventItem = db.Event.Find(id);
            if (eventItem == null)
            {
                return HttpNotFound();
            }
            db.SaveChanges();
            return View(eventItem);
        }
        [HttpGet]
        public ActionResult AddCommitteeMember()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddCommitteeMember(userAddCommittee model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            // Serialize form data to JSON
                            string json = JsonConvert.SerializeObject(model);
                            form.Add(new StringContent(json, Encoding.UTF8, "application/json"), "submitInfo");

                            // Attach image if provided
                            if (imageFile != null && imageFile.ContentLength > 0)
                            {
                                var streamContent = new StreamContent(imageFile.InputStream);
                                streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                                {
                                    Name = "\"imageFile\"",
                                    FileName = "\"" + imageFile.FileName + "\""
                                };
                                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                                form.Add(streamContent);
                            }

                            // API URL
                            string apiUrl = "http://localhost/TalentHunt1/api/Main/AddCommitteeMember"; // replace with actual

                            var response = await client.PostAsync(apiUrl, form);

                            if (response.IsSuccessStatusCode)
                            {
                                TempData["Success"] = "Committee member added successfully.";
                                return RedirectToAction("AddCommitteeMember");
                            }
                            else
                            {
                                TempData["Error"] = "Error: " + await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error: " + ex.Message;
                }
            }

            return View(model);
        }

        // GET: Assign committee members to an event
        // GET: Assign committee members to an event
        private readonly string showCommitteeMembersUrl = "http://localhost/TalentHunt1/api/Main/GetAllCommitteeMembers";
        [HttpGet]
        public async Task<ActionResult> AssignCommittee(int eventId)
        {
            ViewBag.EventId = eventId;
            List<CommitteeMember> allMembers = new List<CommitteeMember>();
            List<int> assignedMemberIds = new List<int>();

            using (HttpClient client = new HttpClient())
            {
                // Get all committee members
                var allResponse = await client.GetAsync("http://localhost/TalentHunt1/api/Main/GetAllCommitteeMember");
                if (allResponse.IsSuccessStatusCode)
                {
                    string allData = await allResponse.Content.ReadAsStringAsync();
                    allMembers = JsonConvert.DeserializeObject<List<CommitteeMember>>(allData);
                }

                // Get assigned committee members for the current event
                var assignedResponse = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/GetAssignedMembers?eventId={eventId}");
                if (assignedResponse.IsSuccessStatusCode)
                {
                    string assignedData = await assignedResponse.Content.ReadAsStringAsync();
                    var assigned = JsonConvert.DeserializeObject<List<AssignedMember>>(assignedData);  // Corrected class name
                    assignedMemberIds = assigned
                        .Where(a => a.CommitteeMemberID.HasValue)
                        .Select(a => a.CommitteeMemberID.Value)
                        .ToList();
                }
            }

            // Pass the assigned member IDs to pre-check the checkboxes
            ViewBag.AssignedMemberIds = assignedMemberIds;

            // Return the view with all committee members
            return View(allMembers);
        }

        // POST: Assign selected committee members to an event
        [HttpPost]
        public async Task<ActionResult> AssignCommittee(int eventId, List<int> selectedMembers, string status)
        {
            ViewBag.EventId = eventId;
            if (selectedMembers == null || !selectedMembers.Any())
            {
                TempData["Error"] = "⚠️ Please select at least one committee member.";
                return RedirectToAction("AssignCommittee", new { eventId });
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                status = "pending";
            }

            var payload = new
            {
                Id = 0,
                EventId = eventId,
                MemberIdList = selectedMembers,
                Status = status
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost/TalentHunt1/api/Main/AssignedMemberToEvent", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    string message = responseContent.Trim('"');
                    if (message.Contains("already assigned"))
                    {
                        TempData["Warning"] = message;
                    }
                    else
                    {
                        TempData["Success"] = message;
                    }
                    return RedirectToAction("DashBoard");
                }
                else
                {
                    TempData["Error"] = "❌ Error assigning members. " + responseContent;
                    return RedirectToAction("AssignCommittee", new { eventId });
                }
            }
        }



        [HttpGet]
        public async Task<ActionResult> ShowAssignedMembers(int eventId)
        {
            List<AssignedMemberViewModel> assignedMembers = new List<AssignedMemberViewModel>();

            using (HttpClient client = new HttpClient())
            {
                // Get assigned members from API
                HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/Main/ShowAssignedMembers?eventid={eventId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    assignedMembers = JsonConvert.DeserializeObject<List<AssignedMemberViewModel>>(result);
                }
            }

            return View(assignedMembers);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateEvent(Event model, HttpPostedFileBase file)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost/TalentHunt1/");

                    using (var content = new MultipartFormDataContent())
                    {
                        // Serialize event model to JSON
                        var json = JsonConvert.SerializeObject(model);
                        content.Add(new StringContent(json, Encoding.UTF8, "application/json"), "submitInfo");

                        // Check if the file is provided
                        if (file != null && file.ContentLength > 0)
                        {
                            var streamContent = new StreamContent(file.InputStream);
                            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "file",
                                FileName = Path.GetFileName(file.FileName)
                            };
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                            content.Add(streamContent, "file", Path.GetFileName(file.FileName));
                        }

                        // Call the API
                        var response = await client.PostAsync("api/main/UpdateEvent", content);

                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Message"] = "Event updated successfully.";
                        }
                        else
                        {
                            TempData["Message"] = "Failed to update event: " + response.ReasonPhrase;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Exception: " + ex.Message;
            }

            return RedirectToAction("DashBoard");
        }



        public async Task<ActionResult> ShowAllCommiteeMembers()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost/TalentHunt1/api/Main/GetAllCommitteeMember");

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var committeeMembers = JsonConvert.DeserializeObject<List<CommitteeMember>>(json);

                        return View("ShowAllCommiteeMembers", committeeMembers); // Pass data to view
                    }
                    else
                    {
                        TempData["Error"] = "Error fetching committee members.";
                        return View(new List<CommitteeMember>());
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return View(new List<CommitteeMember>());
            }
        }
        

        private readonly string deletecommitteememberapi = "http://localhost/TalentHunt1/api/Main/DeleteCommitteeMember";

        [HttpPost]
        public async Task<ActionResult> DeleteCommitteeMember(int id)
        {
            using (var client = new HttpClient())
            {
                // Prepare request content
                var requestData = new { id = id };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Send POST request to API
                var response = await client.PostAsync(deletecommitteememberapi, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Committee member deleted successfully.";
                }
                else
                {
                    TempData["Message"] = "Error deleting committee member.";
                }

                return RedirectToAction("ShowAllCommiteeMembers"); // Or wherever you want to go after deletion
            }
        }

        [HttpGet] // or HttpPost, depending on how you call it from the UI
        public async Task<ActionResult> ViewAssignments(int userId)
        {
            List<AssignedMemberDetails> assignments = new List<AssignedMemberDetails>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost/TalentHunt1/");

                var requestBody = new { UserId = userId };
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("api/Main/NotificationToAssignMember", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        assignments = JsonConvert.DeserializeObject<List<AssignedMemberDetails>>(content);
                    }
                    else
                    {
                        ViewBag.Error = "Failed to fetch data. Status Code: " + response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Exception occurred: " + ex.Message;
                }
            }

            return View(assignments);
        }





        private HttpClient client3;



      



        [HttpGet]
        public async Task<ActionResult> AcceptReject(int id, string status)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost/TalentHunt1/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    Id = id,
                    Status = status
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Main/RequestAcceptReject", content);

                // Save message regardless of success/failure
                TempData["Message"] = response.IsSuccessStatusCode
                    ? "Status updated successfully!"
                    : "Error updating status.";
            }

            // ✅ Always redirect somewhere (NEVER return View)
            return RedirectToAction("CommitteeDashBoard"); // replace with your actual target view
        }







        //[HttpPost]
        //public async Task<ActionResult> Apply(int userId, int eventId)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri("http://localhost/TalentHunt1/");

        //        var requestData = new
        //        {
        //            UserId = userId,
        //            EventId = eventId
        //        };

        //        var json = JsonConvert.SerializeObject(requestData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = await client.PostAsync("api/Main/Apply", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var resultString = await response.Content.ReadAsStringAsync();
        //            dynamic result = JsonConvert.DeserializeObject<dynamic>(resultString);

        //            // Use TempData to persist between redirects
        //            TempData["StudentName"] = result.StudentName;
        //            TempData["EventTitle"] = result.EventTitle;
        //            TempData["Status"] = result.Status;

        //            return RedirectToAction("Events");
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Application failed: " + response.ReasonPhrase;
        //            return RedirectToAction("Events");
        //        }
        //    }
        //}
        [HttpGet]
        public async Task<ActionResult> RegisteredEvents(int UserId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    //HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/main/RegisteredEvents?eventId={eventId}  UserId={ UserId}");
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/TalentHunt1/api/main/RegisteredEvents?UserId={UserId}");

                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var events = JsonConvert.DeserializeObject<List<RegisteredEventsDetails>>(data);

                        // Find the event with the given id
                        //var eventItem = events.FirstOrDefault(e => e.Id == eventId);

                        if (events != null)
                        {
                            return View(events); // Pass event details to view
                        }
                        else
                        {
                            return HttpNotFound("You are not register in any event"); // Event not found
                        }
                    }
                    else
                    {
                        return HttpNotFound("Error retrieving events.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

    }

}

