// Import the Doctor model so that the extension method can work with Doctor objects.
using HospitalAppointmentManagement.Models;

// This namespace contains extension methods used by the application.
namespace HospitalAppointmentManagement.Extensions;

// This static class contains extension methods for the Doctor model.
public static class DoctorExtensions
{
    // This extension method calculates the total consultation charge.
    public static decimal CalculateTotalCharge(this Doctor doctor)
    {
        // Add the fixed ₹100 service charge to the doctor's consultation fee.
        return doctor.ConsultationFee + 100m;
    }
}