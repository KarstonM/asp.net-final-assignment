using asp.net_final_assignment.Models;

namespace asp.net_final_assignment.ViewModels
{
    public class ReservationIndexVM
    {
        public List<Reservation> Reservations { get; set; }
        public List<Car> Cars { get; set; }
        public List<Customer> Renters { get; set; }
        public Reservation NewReservation { get; set; } = new Reservation();
    }
}
