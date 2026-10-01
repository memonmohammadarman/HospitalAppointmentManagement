// This namespace contains the model classes used in the application.
namespace HospitalAppointmentManagement.Models;

// This class represents a doctor in the hospital appointment system.
public class Doctor
{
    // Stores the unique ID of the doctor.
    public int DoctorId { get; set; }

    // Stores the full name of the doctor.
    public string DoctorName { get; set; } = string.Empty;

    // Stores the medical specialization of the doctor.
    public string Specialization { get; set; } = string.Empty;

    // Stores the number of years of professional experience.
    public int Experience { get; set; }

    // Stores the doctor's consultation fee before adding the service charge.
    public decimal ConsultationFee { get; set; }
}