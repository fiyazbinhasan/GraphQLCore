using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLAPI.Models;

namespace GraphQLAPI.Store
{
    public interface IDataStore
    {
		Task<IEnumerable<Item>> GetItemsAsync();
		Task<Item> GetItemByBarcodeAsync(string barcode);
        Task<Item> GetItemByIdAsync(int itemId);
        Task<Item> CreateItemAsync(Item item);

        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer> CreateCustomerAsync(Customer customer);

		Task<IEnumerable<Order>> GetOrdersAsync();
		Task<Order> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
		Task<Order> CreateOrderAsync(Order order);      

		Task<IEnumerable<OrderItem>> GetOrderItemAsync();
		Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
	}
}