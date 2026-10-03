using System.ComponentModel.DataAnnotations;

namespace asp.net_final_assignment.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
        [Phone]
        [Required(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
        public string PaymentType { get; set; }
        public ICollection<Reservation>? Reservations { get; set; } = new List<Reservation>();
        public string? Status { get; set; }
    }
}
