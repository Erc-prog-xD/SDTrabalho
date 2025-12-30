namespace NotificationService.Models
{
    
public class AppointmentEvent
{
    public long appointment_id { get; set; }
    public long patient_id { get; set; }
    public long doctor_id { get; set; }
    public string status { get; set; } = "";
    public string message { get; set; } = "";
    public string created_at { get; set; } = ""; // ISO string
}

}