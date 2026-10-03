using asp.net_final_assignment.Data;
using asp.net_final_assignment.Models;
using asp.net_final_assignment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace asp.net_final_assignment.Controllers
{
    public class VehicleController : Controller
    {
        // Database Context
        private readonly VehicleDbContext vehicleDbContext;
        public VehicleController(VehicleDbContext vehicleDbContext)
        {
            this.vehicleDbContext = vehicleDbContext;
        }
        
        public bool AuthCheck()
        {
            return User.Identity.IsAuthenticated;
        }

        // Vehicles Page
        [HttpGet]
        [Route("vehicles")]
        public async Task<IActionResult> Index(string search)
        {
            if (!AuthCheck()) return RedirectToAction("Login", "Account");

            // If search, then filter based on search
            var vehicles = vehicleDbContext.Cars.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                vehicles = vehicles.Where(c =>
                    c.Colour.Contains(search) ||
                    c.Model.Contains(search) ||
                    c.Make.Contains(search) ||
                    c.Year.ToString().Contains(search) ||
                    c.Status.Contains(search));
            }

            var viewModel = new VehicleIndexVM
            {
                Cars = await vehicles.ToListAsync()
            };
            return View(viewModel);
        }

        // Add Car
        [HttpPost]
        [Route("vehicles/add")]
        public async Task<IActionResult> Add(Car car)
        {
            if (ModelState.IsValid)
            {
                var newCar = new Car()
                {
                    Model = car.Model,
                    Make = car.Make,
                    Colour = car.Colour,
                    Year = car.Year,
                    Status = car.Status
                };
                await vehicleDbContext.Cars.AddAsync(newCar);
                await vehicleDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Edit Car
        [HttpPost]
        [Route("vehicles/edit/{id}")]
        public async Task<IActionResult> Edit(Car newCar)
        {
            if (ModelState.IsValid)
            {
                var car = await vehicleDbContext.Cars.FindAsync(newCar.Id);
                if (car != null)
                {
                    car.Model = newCar.Model;
                    car.Make = newCar.Make;
                    car.Colour = newCar.Colour;
                    car.Year = newCar.Year;
                    await vehicleDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(newCar);
        }

        // Delete Car
        [HttpGet]
        [Route("vehicles/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await vehicleDbContext.Cars.FindAsync(id);
            if (car != null)
            {
                vehicleDbContext.Cars.Remove(car);
                await vehicleDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
