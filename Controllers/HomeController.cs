// Import MVC classes and attributes used by the controller.
using Microsoft.AspNetCore.Mvc;

// Import memory-cache classes used to cache the doctor list.
using Microsoft.Extensions.Caching.Memory;

// Import JSON helper methods used to read the API response asynchronously.
using System.Net.Http.Json;

// Import the extension method used to calculate the total consultation charge.
using HospitalAppointmentManagement.Extensions;

// Import the Doctor and Post model classes.
using HospitalAppointmentManagement.Models;

// Define the namespace for the application's controllers.
namespace HospitalAppointmentManagement.Controllers;

// This controller handles doctor information, appointments and external posts.
public class HomeController : Controller
{
    // Store the IMemoryCache service received through dependency injection.
    private readonly IMemoryCache _memoryCache;

    // Store the HttpClientFactory used to create an HttpClient.
    private readonly IHttpClientFactory _httpClientFactory;

    // Define the cache key used to store the doctor list.
    private const string DoctorCacheKey = "doctor-list";

    // Constructor receives the required services from ASP.NET Core.
    public HomeController(
        IMemoryCache memoryCache,
        IHttpClientFactory httpClientFactory)
    {
        // Store the IMemoryCache service in the private field.
        _memoryCache = memoryCache;

        // Store the HttpClientFactory in the private field.
        _httpClientFactory = httpClientFactory;
    }

    // Display the list of doctors on the home page.
    public IActionResult Index()
    {
        // Get the doctor list from the memory cache.
        var doctors = GetCachedDoctors();

        // Send the doctor list to the Index Razor View.
        return View(doctors);
    }

    // Display details of a selected doctor.
    // Response caching is enabled for 60 seconds.
    // VaryByQueryKeys ensures different DoctorId values have separate cached responses.
    [ResponseCache(
        Duration = 60,
        Location = ResponseCacheLocation.Any,
        NoStore = false,
        VaryByQueryKeys = new[] { "DoctorId" })]
    public IActionResult Details(int DoctorId)
    {
        // Retrieve the doctor list from IMemoryCache.
        var doctors = GetCachedDoctors();

        // Find the doctor whose ID matches the DoctorId from the query string.
        var doctor = doctors.FirstOrDefault(
            d => d.DoctorId == DoctorId);

        // Check whether the requested doctor exists.
        if (doctor == null)
        {
            // Return HTTP 404 when the doctor is not found.
            return NotFound();
        }

        // Calculate the total consultation charge using the extension method.
        ViewBag.TotalCharge = doctor.CalculateTotalCharge();

        // Send the selected doctor to the Details Razor View.
        return View(doctor);
    }

    // Receive the appointment form submitted by the patient.
    [HttpPost]

    // Protect the form against Cross-Site Request Forgery attacks.
    [ValidateAntiForgeryToken]
    public IActionResult Appointment(
        int DoctorId,
        string PatientName)
    {
        // Check whether the patient entered a valid name.
        if (string.IsNullOrWhiteSpace(PatientName))
        {
            // Return to the selected doctor's details page if the name is empty.
            return RedirectToAction(
                nameof(Details),
                new { DoctorId });
        }

        // Retrieve the doctor list from IMemoryCache.
        var doctors = GetCachedDoctors();

        // Find the doctor selected through the hidden DoctorId field.
        var doctor = doctors.FirstOrDefault(
            d => d.DoctorId == DoctorId);

        // Check whether the selected doctor exists.
        if (doctor == null)
        {
            // Return HTTP 404 when the doctor does not exist.
            return NotFound();
        }

        // Store the patient's name in a browser cookie.
        Response.Cookies.Append(
            "PatientName",
            PatientName.Trim(),
            new CookieOptions
            {
                // Prevent JavaScript from directly accessing the cookie.
                HttpOnly = true,

                // Mark the cookie as essential for the application.
                IsEssential = true,

                // Allow the cookie on normal same-site requests.
                SameSite = SameSiteMode.Lax,

                // Make the cookie expire after one day.
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

        // Store the selected doctor's specialization in Session.
        HttpContext.Session.SetString(
            "SelectedSpecialization",
            doctor.Specialization);

        // Calculate the total consultation charge.
        ViewBag.TotalCharge =
            doctor.CalculateTotalCharge();

        // Create a confirmation message for the patient.
        ViewBag.AppointmentMessage =
            $"Appointment request submitted successfully for {doctor.DoctorName}.";

        // Read the specialization back from Session.
        ViewBag.SelectedSpecialization =
            HttpContext.Session.GetString(
                "SelectedSpecialization");

        // Display the doctor details page with the appointment confirmation.
        return View("Details", doctor);
    }

    // Retrieve posts asynchronously from JSONPlaceholder.
    public async Task<IActionResult> Posts()
    {
        // Create the named HttpClient registered in Program.cs.
        var client =
            _httpClientFactory.CreateClient("JsonPlaceholder");

        // Send an asynchronous GET request to the posts API.
        var posts =
            await client.GetFromJsonAsync<List<Post>>("posts")
            ?? new List<Post>();

        // Send the received posts to the Posts Razor View.
        return View(posts);
    }

    // Create the sample doctor list used by this CIE application.
    private static List<Doctor> GetDoctors()
    {
        // Return a list containing three sample doctors.
        return new List<Doctor>
        {
            // Create the first doctor.
            new Doctor
            {
                // Set the first doctor's ID.
                DoctorId = 1,

                // Set the first doctor's name.
                DoctorName = "Dr. Rajesh Patel",

                // Set the first doctor's specialization.
                Specialization = "Cardiology",

                // Set the first doctor's experience.
                Experience = 12,

                // Set the first doctor's consultation fee.
                ConsultationFee = 800m
            },

            // Create the second doctor.
            new Doctor
            {
                // Set the second doctor's ID.
                DoctorId = 2,

                // Set the second doctor's name.
                DoctorName = "Dr. Neha Shah",

                // Set the second doctor's specialization.
                Specialization = "Dermatology",

                // Set the second doctor's experience.
                Experience = 8,

                // Set the second doctor's consultation fee.
                ConsultationFee = 600m
            },

            // Create the third doctor.
            new Doctor
            {
                // Set the third doctor's ID.
                DoctorId = 3,

                // Set the third doctor's name.
                DoctorName = "Dr. Amit Mehta",

                // Set the third doctor's specialization.
                Specialization = "Orthopedics",

                // Set the third doctor's experience.
                Experience = 15,

                // Set the third doctor's consultation fee.
                ConsultationFee = 1000m
            }
        };
    }

    // Get the doctor list from IMemoryCache.
    private List<Doctor> GetCachedDoctors()
    {
        // Try to retrieve the doctor list using the cache key.
        if (!_memoryCache.TryGetValue(
            DoctorCacheKey,
            out List<Doctor>? doctors))
        {
            // Create the doctor list when it is not already cached.
            doctors = GetDoctors();

            // Configure the cache expiration settings.
            var cacheOptions =
                new MemoryCacheEntryOptions
                {
                    // Expire the doctor list after three minutes.
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(3)
                };

            // Store the doctor list in IMemoryCache.
            _memoryCache.Set(
                DoctorCacheKey,
                doctors,
                cacheOptions);
        }

        // Return the cached doctor list.
        return doctors!;
    }
}