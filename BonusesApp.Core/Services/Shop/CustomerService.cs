using BonusesApp.Core.Infrastructure;
using BonusesApp.Core.Models.Shop;
using BonusesApp.Core.Services.Shop.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BonusesApp.Core.Services.Shop;

public class CustomerService(ApplicationDbContext dbContext) : ICustomerService
{
    public IEnumerable<Customer> GetTopActiveCustomers(int count) => throw new NotImplementedException();

    public IEnumerable<Customer> GetAllCustomersData() => dbContext.Customers
        .Include(c => c.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(d => d.Product)
        .Include(c => c.Orders).ThenInclude(o => o.Cashier)
        .AsSingleQuery()
        .OrderBy(c => c.Name)
        .ToList();
}