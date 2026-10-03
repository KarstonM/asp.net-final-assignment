using asp.net_final_assignment.Data;
using asp.net_final_assignment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asp.net_final_assignment.Controllers
{
    public class HomeController : Controller
    {
        // Database Context
        private readonly VehicleDbContext vehicleDbContext;
        public HomeController(VehicleDbContext vehicleDbContext)
        {
            this.vehicleDbContext = vehicleDbContext;    
        }

        public bool AuthCheck()
        {
            return User.Identity.IsAuthenticated;
        }

        // Dashboard
        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            if (!AuthCheck()) return RedirectToAction("Login", "Account");

            // Retrieve data for dashboard
            var totalVehicles = await vehicleDbContext.Cars.CountAsync();
            var rentedVehicles = await vehicleDbContext.Cars.CountAsync(c => c.Status == "Rented");
            var availableVehicles = await vehicleDbContext.Cars.CountAsync(c => c.Status == "Available");
            var maintenanceVehicles = await vehicleDbContext.Cars.CountAsync(c => c.Status == "Maintenance");
            var totalCustomers = await vehicleDbContext.Customers.CountAsync();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var newCustomersThisMonth = await vehicleDbContext.Customers
                .Where(c => c.Reservations.Any())
                .Where(c => c.Reservations
                    .OrderBy(r => r.Start)
                    .FirstOrDefault().Start.Month == currentMonth &&
                            c.Reservations.OrderBy(r => r.Start)
                    .FirstOrDefault().Start.Year == currentYear)
                .CountAsync();

            // Upcoming Reservations
            var now = DateTime.Now;
            var upcomingReservations = await vehicleDbContext.Reservations
                .CountAsync(r => r.Start > now);

            // Compare total reservations this month vs last month
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var totalThisMonth = await vehicleDbContext.Reservations
                .CountAsync(r => r.Start >= thisMonthStart && r.Start < nextMonthStart);
                                            
            var totalLastMonth = await vehicleDbContext.Reservations
                .CountAsync(r => r.Start >= lastMonthStart && r.Start < thisMonthStart);

            var diff = totalThisMonth - totalLastMonth;
            var diffText = (diff >= 0 ? "+" : "") + diff.ToString();

            var recentCustomers = vehicleDbContext.Customers
                .Include(c => c.Reservations)
                .ThenInclude(r => r.Car)
                .Where(c => c.Reservations.Any())
                .OrderByDescending(c => c.Reservations.Max(r => r.Start))
                .Take(5)
                .ToList();

            var topMakes = vehicleDbContext.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car != null)
                .GroupBy(r => r.Car.Make)
                .Select(g => new
                {
                    Make = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(4)
                .ToList();

            // Send data to the view
            ViewBag.TotalVehicles = totalVehicles;
            ViewBag.RentedVehicles = rentedVehicles;
            ViewBag.AvailableVehicles = availableVehicles;
            ViewBag.MaintenanceVehicles = maintenanceVehicles;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.NewCustomers = newCustomersThisMonth;
            ViewBag.UpcomingReservations = upcomingReservations;
            ViewBag.ReservationDiffText = diffText;
            ViewBag.RecurringCustomers = recentCustomers;
            ViewBag.TopRentedMakes = topMakes;
            return View();
        }

        // Reports Page
        [HttpGet]
        [Route("reports")]
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
        {
            if (!AuthCheck()) return RedirectToAction("Login", "Account");
                    
            // Retireve reservations
            var reservations = await vehicleDbContext.Reservations
                .Include(r => r.Car)
                .Include(r => r.Renter)
                .ToListAsync();

            // If start date filter has val, filter
            if (startDate.HasValue)
                reservations = reservations.Where(r => r.Start >= startDate.Value).ToList();

            // if end date filter has val, filter
            if (endDate.HasValue)
                reservations = reservations.Where(r => r.End <= endDate.Value).ToList();

            // Retrieve the most rented vehicle model
            var mostRentedGroup = reservations
                .GroupBy(r => r.Car?.Model)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            // Get model name and percentage
            var mostRentedModel = mostRentedGroup?.Key ?? "N/A";
            var mostRentedModelPercentage = mostRentedGroup != null
                ? (int)((double)mostRentedGroup.Count() / reservations.Count * 100)
                : 0;

            // Calc avg duration of rentals in days
            var averageDuration = reservations.Count > 0
                ? (int)reservations.Average(r => (r.End - r.Start).TotalDays)
                : 0;

            // Determine month with most reservations
            var peakMonthGroup = reservations
                .GroupBy(r => r.Start.Month)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            //Get name of rental month and percentage of month with most reservations
            string peakMonth = peakMonthGroup != null
                ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(peakMonthGroup.Key)
                : "N/A";

            double peakMonthPercentage = peakMonthGroup != null
                ? (int)((double)peakMonthGroup.Count() / reservations.Count * 100)
                : 0;

            // Find renter with most reservations
            var mostActiveRenterGroup = reservations
                .GroupBy(r => r.Renter?.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            // Count active reservations or upcoming
            var activeReservations = reservations.Count(r => r.End >= DateTime.Today);

            // Count total customers and divide by status
            var customerCount = await vehicleDbContext.Customers.CountAsync();
            var activeCustomers = await vehicleDbContext.Customers.CountAsync(c => c.Status == "Active" || c.Status == "Renting");
            var inactiveCustomers = customerCount - activeCustomers;

            // Send view model
            var viewModel = new ReportsVM
            {
                StartDate = startDate,
                EndDate = endDate,
                MostRentedModel = mostRentedModel,
                MostRentedModelPercentage = mostRentedModelPercentage,
                AverageRentalDays = averageDuration,
                PeakRentalMonth = peakMonth,
                PeakRentalMonthPercentage = peakMonthPercentage,
                MostActiveRenter = mostActiveRenterGroup?.Key ?? "N/A",
                ActiveReservationsCount = activeReservations,
                ActiveCustomerRate = customerCount > 0 ? (int)((double)activeCustomers / customerCount * 100) : 0,
                InactiveCustomerRate = customerCount > 0 ? (int)((double)inactiveCustomers / customerCount * 100) : 0
            };

            return View(viewModel);
        }
    }
}
