// This namespace contains the model classes used in the application.
namespace HospitalAppointmentManagement.Models;

// This class represents a post received from the JSONPlaceholder API.
public class Post
{
    // Stores the unique ID of the post received from the API.
    public int Id { get; set; }

    // Stores the title of the post received from the API.
    public string Title { get; set; } = string.Empty;
}