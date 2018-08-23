using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQLAPI.Models;

namespace GraphQLAPI.Store
{
    public interface IDataStore
    {
		Task<IEnumerable<Item>> GetItemsAsync();
		Task<Item> GetItemByBarcodeAsync(string barcode);
		Task<Item> GetItemByIdAsync(int itemId);
		Task<IDictionary<int, Item>> GetItemsByIdAsync(IEnumerable<int> itemIds, CancellationToken token);
        Task<Item> CreateItemAsync(Item item);

        Task<IEnumerable<Customer>> GetCustomersAsync();
		Task<Customer> GetCustomerByIdAsync(int customerId);
		Task<IDictionary<int, Customer>> GetCustomersByIdAsync(IEnumerable<int> customerIds, CancellationToken token);
        Task<Customer> CreateCustomerAsync(Customer customer);

		Task<IEnumerable<Order>> GetOrdersAsync();
		Task<Order> GetOrderByIdAsync(int orderId);
        Task<IDictionary<int, Order>> GetOrdersByIdAsync(IEnumerable<int> orderIds, CancellationToken token);
		Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
		Task<ILookup<int, Order>> GetOrdersByCustomerIdAsync(IEnumerable<int> customerIds);
		Task<Order> CreateOrderAsync(Order order);      

		Task<IEnumerable<OrderItem>> GetOrderItemAsync();
		Task<OrderItem> AddOrderItemAsync(OrderItem orderItem);
	}
}