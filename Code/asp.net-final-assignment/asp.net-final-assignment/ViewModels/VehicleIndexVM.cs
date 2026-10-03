using asp.net_final_assignment.Models;

namespace asp.net_final_assignment.ViewModels
{
    public class VehicleIndexVM
    {
        public List<Car> Cars { get; set; }
        public Car NewCar { get; set; } = new Car();
    }
}
