using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLAPI.Models;

namespace GraphQLAPI.Store
{
    public interface IDataStore
    {
		Task<IEnumerable<Item>> GetItemsAsync();
        Task<Item> GetItemByBarcodeAsync(string barcode);
        Task<Item> AddItemAsync(Item item);
      
		Task<IEnumerable<Order>> GetOrdersAsync();
		Task<Order> AddOrderAsync(Order order);

		Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
		Task<Customer> AddCustomerAsync(Customer customer);
	}
}