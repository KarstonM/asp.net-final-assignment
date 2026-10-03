using asp.net_final_assignment.Data;
using asp.net_final_assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using asp.net_final_assignment.ViewModels;

namespace asp.net_final_assignment.Controllers
{
    public class ReservationController : Controller
    {
        // Database Context
        private readonly VehicleDbContext vehicleDbContext;
        public ReservationController(VehicleDbContext vehicleDbContext)
        {
            this.vehicleDbContext = vehicleDbContext;
        }

        public bool AuthCheck()
        {
            return User.Identity.IsAuthenticated;
        }

        // Reservations Page
        [HttpGet]
        [Route("reservations")]
        public async Task<IActionResult> Index(string search)
        {
            if (!AuthCheck()) return RedirectToAction("Login", "Account");

            // Todays date
            var today = DateTime.Today;

            // Load all reservations
            var reservations = vehicleDbContext.Reservations
                .Include(r => r.Car)
                .Include(r => r.Renter)
                .AsQueryable(); // stays in DB context

            // Automatically update reservation statuses
            foreach (var res in reservations)
            {
                var car = res.Car;
                var renter = res.Renter;

                // Set car and renter status back to available/active if curr date is past end date
                if (res.End.Date <= today && res.Status != "Completed")
                {
                    res.Status = "Completed";
                    if (car != null) car.Status = "Available";
                    if (renter != null) renter.Status = "Active";
                }

                // Set car and renter status to rented/renting if curr date is within start and end date range
                else if (res.Start.Date <= today && res.End.Date > today && res.Status == "Upcoming")
                {
                    res.Status = "Confirmed";
                    if (car != null) car.Status = "Rented";
                    if (renter != null) renter.Status = "Renting";
                }
            }
            await vehicleDbContext.SaveChangesAsync();

            // If search then filter
            if (!string.IsNullOrEmpty(search))
            {
                reservations = reservations.Where(r =>
                    r.Renter.Name.Contains(search) ||
                    r.Car.Model.Contains(search) ||
                    r.Car.Make.Contains(search) ||
                    r.Car.Colour.Contains(search) ||
                    r.Car.Year.ToString().Contains(search) ||
                    r.Status.Contains(search));
            }

            // Filtered reservation list
            var allReservations = await reservations.ToListAsync();

            // Cars not under maintenance
            var availableCars = await vehicleDbContext.Cars
                .Where(c => c.Status != "Maintenance")
                .ToListAsync();

            // Find cars already reserved in future
            var futureReservations = await vehicleDbContext.Reservations
                .Where(r => r.End >= today && r.Status != "Completed")
                .ToListAsync();

            var reservedCarIds = futureReservations.Select(r => r.CarId).Distinct().ToList();

            // Cars available for new reservations (not reserved)
            var reservableCars = availableCars
                .Where(c => !reservedCarIds.Contains(c.Id))
                .ToList();

            // Add back only the car assigned to the reservation being edited
            var editingReservationCarIds = allReservations
                .Where(r => r.Car != null && (r.Status != "Completed" && r.End >= today))
                .Select(r => r.Car.Id)
                .Distinct();

            foreach (var carId in editingReservationCarIds)
            {
                var car = await vehicleDbContext.Cars.FindAsync(carId);
                if (car != null && !reservableCars.Any(c => c.Id == car.Id))
                {
                    reservableCars.Add(car);
                }
            }

            // Send view model
            return View(new ReservationIndexVM
            {
                Reservations = reservations.ToList(),
                Cars = reservableCars,
                Renters = await vehicleDbContext.Customers
                    .Where(c => c.Status != "Inactive")
                    .ToListAsync()
            });
        }

        // Add Reservation
        [HttpPost]
        [Route("reservations/add")]
        public async Task<IActionResult> Add(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                // Retrieve Car and Renter objs
                var car = await vehicleDbContext.Cars.FindAsync(reservation.CarId);
                var renter = await vehicleDbContext.Customers.FindAsync(reservation.RenterId);

                if (car != null && renter != null)
                {
                    var newReservation = new Reservation()
                    {
                        Status = reservation.Status,
                        Car = car,
                        Renter = renter,
                        Start = reservation.Start,
                        End = reservation.End
                    };

                    // Set related statuses
                    if (reservation.Status != "Completed")
                    {
                        car.Status = "Rented";
                        renter.Status = "Renting";
                    }

                    await vehicleDbContext.Reservations.AddAsync(newReservation);
                    await vehicleDbContext.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        // Edit Reservation
        [HttpPost]
        [Route("reservations/edit/{id}")]
        public async Task<IActionResult> Edit(Reservation newReservation)
        {
            if (ModelState.IsValid)
            {
                var reservation = await vehicleDbContext.Reservations
                    .Include(r => r.Car)
                    .Include(r => r.Renter)
                    .FirstOrDefaultAsync(r => r.Id == newReservation.Id);

                var newCar = await vehicleDbContext.Cars.FindAsync(newReservation.CarId);
                var newRenter = await vehicleDbContext.Customers.FindAsync(newReservation.RenterId);

                if (reservation != null && newCar != null && newRenter != null)
                {
                    // Reset old car/renter status
                    if (reservation.Status != "Completed")
                    {
                        reservation.Car.Status = "Available";
                        reservation.Renter.Status = "Active";
                    }

                    // Update core fields
                    reservation.Car = newCar;
                    reservation.CarId = newReservation.CarId;
                    reservation.Renter = newRenter;
                    reservation.RenterId = newReservation.RenterId;
                    reservation.Start = newReservation.Start;
                    reservation.End = newReservation.End;

                    var today = DateTime.Today;

                    // Determine new status based on dates
                    if (reservation.End.Date <= today)
                    {
                        reservation.Status = "Completed";
                        newCar.Status = "Available";
                        newRenter.Status = "Active";
                    }
                    else if (reservation.Start.Date > today)
                    {
                        reservation.Status = "Upcoming";
                        newCar.Status = "Available";
                        newRenter.Status = "Active";
                    }
                    else // within range
                    {
                        reservation.Status = "Confirmed";
                        newCar.Status = "Rented";
                        newRenter.Status = "Renting";
                    }

                    await vehicleDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }

            return View(newReservation);
        }


        // Delete Reservation
        [HttpGet]
        [Route("reservations/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve reservation obj
            var reservation = await vehicleDbContext.Reservations
                .Include(r => r.Car)
                .Include(r => r.Renter)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation != null)
            {
                var car = reservation.Car;
                var renter = reservation.Renter;

                vehicleDbContext.Reservations.Remove(reservation);
                await vehicleDbContext.SaveChangesAsync();

                // Check if car has other active reservations
                bool carStillReserved = await vehicleDbContext.Reservations.AnyAsync(r =>
                    r.CarId == car.Id && r.Status != "Completed");

                // If reservation is still active, reset the associated cars status
                if (!carStillReserved)
                {
                    car.Status = "Available";
                }

                // Check if renter has other active reservations
                bool renterStillRenting = await vehicleDbContext.Reservations.AnyAsync(r =>
                    r.RenterId == renter.Id && r.Status != "Completed");

                // If reservation still active, reset the associated renters status
                if (!renterStillRenting)
                {
                    renter.Status = "Active";
                }
                await vehicleDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}
