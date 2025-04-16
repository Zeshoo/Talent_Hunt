using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Talent_Hunt.Models;
namespace Talent_Hunt.Controllers
{
    public class AdminController : Controller
    {
        private TalentHunt2Entities1 db = new TalentHunt2Entities1();
        private readonly string apiUrl = "http://localhost/ApiDemo/api/Main/ShowAllEvents";
        private readonly string createEventApiUrl = "http://localhost/ApiDemo/api/Main/CreateEvent";
        private readonly string addRulesApiUrl = "http://localhost/apidemo/api/Main/AddRules";
        private readonly string assignMemberApiUrl = "http://localhost/apidemo/api/Main/AssignedMembersToEvent";
        private readonly string addCommitteeMemberApiUrl = "http://localhost/ApiDemo/api/Main/AddCommitteeMember";
        private readonly string assignApiUrl = "http://localhost/ApiDemo/api/Main/AssignedMemberToEvent";
        public ActionResult Index()
        {
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
        public ActionResult CreateEvent()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateEvent(HttpPostedFileBase eventImage, string eventTitle, string description,
 string regStartDate, string regEndDate, string eventDate, string startTime, string endTime, string[] rules)
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
                    int createdEventId = createdEvent.Id;  // Extract event ID

                    // ✅ Now, send rules to the API
                    if (rules != null && rules.Length > 0 && createdEventId > 0)
                    {
                        var rulesData = new
                        {
                            EventID = createdEventId,
                            Rules = rules.ToList()
                        };

                        string rulesJson = JsonConvert.SerializeObject(rulesData);
                        using (var rulesClient = new HttpClient())
                        {
                            rulesClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            var rulesContent = new StringContent(rulesJson, Encoding.UTF8, "application/json");

                            HttpResponseMessage rulesResponse = await rulesClient.PostAsync(addRulesApiUrl, rulesContent);
                            string rulesResponseBody = await rulesResponse.Content.ReadAsStringAsync();

                            if (!rulesResponse.IsSuccessStatusCode)
                            {
                                TempData["Error"] = $"Event created, but rules not saved. Status: {rulesResponse.StatusCode}, API Response: {rulesResponseBody}";
                                return RedirectToAction("CreateEvent");
                            }
                        }
                    }

                    TempData["Success"] = "Event created successfully with rules!";
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

        private readonly string showEventsApiUrl = "http://localhost/Apidemo/api/Main/ShowAllEvents";
        private readonly string saveTaskApiUrl = "http://localhost/apidemo/api/Main/Createtask";

        // 📌 Fetch events list for dropdown
        [HttpGet]
        // ✅ Function to Fetch All Events and Tasks

        public async Task<ActionResult> CreateTask()
        {
            try
            {
                List<Event> events = new List<Event>();
                List<TaskModel> tasks = new List<TaskModel>();

                using (HttpClient client = new HttpClient())
                {
                    // Fetch all events
                    HttpResponseMessage eventResponse = await client.GetAsync(showEventsApiUrl);
                    if (eventResponse.IsSuccessStatusCode)
                    {
                        string eventData = await eventResponse.Content.ReadAsStringAsync();
                        events = JsonConvert.DeserializeObject<List<Event>>(eventData);
                    }

                    // Fetch all tasks
                    HttpResponseMessage taskResponse = await client.GetAsync(showEventsApiUrl);
                    if (taskResponse.IsSuccessStatusCode)
                    {
                        string taskData = await taskResponse.Content.ReadAsStringAsync();
                        tasks = JsonConvert.DeserializeObject<List<TaskModel>>(taskData);
                    }
                }

                ViewBag.Events = events ?? new List<Event>(); // ✅ Prevents null reference error
                ViewBag.Tasks = tasks ?? new List<TaskModel>(); // ✅ Prevents null reference error
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error fetching data: " + ex.Message;
                return View();
            }
        }

        // ✅ Function to Save Task in Database
        [HttpPost]
        public async Task<ActionResult> CreateTask(TaskModel task)
        {
            if (task == null)
            {
                return Json(new { success = false, message = "Invalid task data!" });
            }

            try
            {
                string jsonContent = JsonConvert.SerializeObject(task);
                using (var client = new HttpClient())
                {
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("http://localhost/ApiDemo/api/Main/CreateTask", content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { success = false, message = $"Error saving task. API Response: {responseBody}" });
                    }
                }

                return Json(new { success = true, message = "Task saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Exception occurred: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult> EventDetails(int id)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Use the original API endpoint to get event details
                    HttpResponseMessage response = await client.GetAsync($"http://localhost/ApiDemo/api/Main/ShowAllEvents");

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

                    HttpResponseMessage response = await client.PostAsync("http://localhost/ApiDemo/api/Main/DeleteEvent", content);
                    if (response.IsSuccessStatusCode)
                    {
                        // ✅ After successful deletion, get the updated list of events
                        HttpResponseMessage getEvents = await client.GetAsync("http://localhost/ApiDemo/api/Main/ShowAllEvents");

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
            var eventItem = db.Events.Find(id);
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

        // POST: Admin/AddCommitteeMember
        [HttpPost]
        public async Task<ActionResult> AddCommitteeMember(Users user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Serialize the user data to JSON
                        var jsonContent = JsonConvert.SerializeObject(user);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        // Make the API call
                        HttpResponseMessage response = await client.PostAsync(addCommitteeMemberApiUrl, content);

                        // Handle the response
                        if (response.IsSuccessStatusCode)
                        {
                            TempData["Success"] = "Committee member added successfully.";
                            return RedirectToAction("AddCommitteeMember"); // Redirect back after success
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            TempData["Error"] = "Error: " + errorMessage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error: " + ex.Message;
                }
            }

            return View(user); // Return view with model in case of errors
        }
        // GET: Assign committee members to an event
        private readonly string showCommitteeMembersUrl = "http://localhost/ApiDemo/api/Main/GetAllCommitteeMembers";
        

        [HttpGet]
        public async Task<ActionResult> AssignCommittee(int eventId)
        {
            ViewBag.EventId = eventId;
            List<Users> committeeMembers = new List<Users>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(showCommitteeMembersUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    committeeMembers = JsonConvert.DeserializeObject<List<Users>>(data);
                }
            }

            return View(committeeMembers);
        }

        [HttpPost]
        public async Task<ActionResult> AssignCommittee(int eventId, List<int> selectedMembers, string status)
        {
            if (selectedMembers == null || !selectedMembers.Any())
            {
                TempData["Error"] = "Please select at least one committee member.";
                return RedirectToAction("AssignCommittee", new { eventId });
            }

            var payload = new
            {
                EventId = eventId,
                MemberIdList = selectedMembers,
                Status = status
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(assignApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Committee members assigned successfully!";
                    return RedirectToAction("EventDetails", new { id = eventId });
                }
                else
                {
                    TempData["Error"] = "Error assigning members.";
                    return RedirectToAction("AssignCommittee", new { eventId });
                }
            }


        }
       

        [HttpGet]
        public async Task<ActionResult> ShowAssignedMembers(int eventId)
        {
            List<AssignedMember> assignedMembers = new List<AssignedMember>();
            List<Users> allUsers = new List<Users>();

            using (HttpClient client = new HttpClient())
            {
                // Get assigned members
                HttpResponseMessage response = await client.GetAsync($"http://localhost/ApiDemo/api/Main/ShowAssignedMembers?eventid={eventId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    assignedMembers = JsonConvert.DeserializeObject<List<AssignedMember>>(result);
                }

                // Get all users
                HttpResponseMessage userResponse = await client.GetAsync("http://localhost/ApiDemo/api/Main/GetAllCommitteeMembers");
                if (userResponse.IsSuccessStatusCode)
                {
                    var result = await userResponse.Content.ReadAsStringAsync();
                    allUsers = JsonConvert.DeserializeObject<List<Users>>(result);
                }
            }

            // Build ViewModel: Match CommitteeMemberID with User Name
           var viewModel = assignedMembers.Select(m => new AssignedMemberViewModel {

                MemberName = allUsers.FirstOrDefault(u => u.Id == m.CommitteeMemberID)?.Name ?? "Unknown",
                Status = m.Status
            }).ToList();

            return View(viewModel);
        }



    }
}