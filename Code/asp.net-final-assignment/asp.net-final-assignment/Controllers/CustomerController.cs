using asp.net_final_assignment.Data;
using asp.net_final_assignment.Models;
using asp.net_final_assignment.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace asp.net_final_assignment.Controllers
{
    public class CustomerController : Controller
    {
        private readonly VehicleDbContext vehicleDbContext;
        public CustomerController(VehicleDbContext vehicleDbContext)
        {
            this.vehicleDbContext = vehicleDbContext;
        }
        public bool AuthCheck()
        {
            return User.Identity.IsAuthenticated;
        }
        [HttpGet]
        [Route("customers")]
        public async Task<IActionResult> Index(string search)
        {
            if (!AuthCheck()) return RedirectToAction("Login", "Account");

            var customers = from c in vehicleDbContext.Customers
                           select c;
            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c => c.Name.Contains(search) ||
                                               c.Status.Contains(search) ||
                                               c.Email.Contains(search) ||
                                               c.PhoneNumber.Contains(search) ||
                                               c.PaymentType.Contains(search));
            }
            return View(new CustomerIndexVM
            {
                Customers = await customers.ToListAsync()
            });

        }
        [HttpPost]
        [Route("customers/add")]
        public async Task<IActionResult> Add(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var newCustomer = new Customer()
                {
                    Name = customer.Name,
                    Email = customer.Email,
                    PhoneNumber = customer.PhoneNumber,
                    PaymentType = customer.PaymentType,
                    Status = customer.Status
                };
                await vehicleDbContext.Customers.AddAsync(newCustomer);
                await vehicleDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("customers/edit/{id}")]
        public async Task<IActionResult> Edit(Customer newCustomer)
        {
            if (ModelState.IsValid)
            {
                var customer = await vehicleDbContext.Customers.FindAsync(newCustomer.Id);
                if (customer != null)
                {
                    customer.Name = newCustomer.Name;
                    customer.Email = newCustomer.Email;
                    customer.PhoneNumber = newCustomer.PhoneNumber;
                    customer.PaymentType = newCustomer.PaymentType;
                    customer.Status = newCustomer.Status;
                    await vehicleDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(newCustomer);
        }
        [HttpGet]
        [Route("customers/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await vehicleDbContext.Customers.FindAsync(id);
            if (customer != null)
            {
                vehicleDbContext.Customers.Remove(customer);
                await vehicleDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
