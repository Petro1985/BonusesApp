using BonusesApp.Core.Models.Shop;

namespace BonusesApp.Core.Services.Shop.Interfaces;

public interface ICustomerService
{
    IEnumerable<Customer> GetTopActiveCustomers(int count);
    IEnumerable<Customer> GetAllCustomersData();
}