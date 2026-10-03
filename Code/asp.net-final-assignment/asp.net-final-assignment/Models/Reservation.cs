using System.ComponentModel.DataAnnotations.Schema;

namespace asp.net_final_assignment.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        [ForeignKey("Car")]
        public int CarId { get; set; }
        public Car? Car { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        [ForeignKey("Renter")]
        public int RenterId { get; set; }           
        public Customer? Renter { get; set; }
    }
}
