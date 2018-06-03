using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLAPI.Data;
using GraphQLAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLAPI.Store
{
    public class DataStore : IDataStore
    {
		private readonly ApplicationDbContext _applicationDbContext;

		public DataStore(ApplicationDbContext applicationDbContext)
        {
			_applicationDbContext = applicationDbContext;
		}

		public async Task<IEnumerable<Item>> GetItemsAsync()
        {
			return await _applicationDbContext.Items.AsNoTracking().ToListAsync();
        }      

		public Task<Item> GetItemByBarcodeAsync(string barcode)
		{
			return _applicationDbContext.Items.FirstAsync(i => i.Barcode.Equals(barcode));
		}

        public async Task<Item> GetItemByIdAsync(int itemId)
        {
            return await _applicationDbContext.Items.FindAsync(itemId);
        }

        public async Task<Item> CreateItemAsync(Item item)
        {
            var addedItem = await _applicationDbContext.Items.AddAsync(item);
            await _applicationDbContext.SaveChangesAsync();
            return addedItem.Entity;
        }

		public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _applicationDbContext.Customers.AsNoTracking().ToListAsync();
        }

		public async Task<Customer> CreateCustomerAsync(Customer customer)
        {         
            var addedCustomer = await _applicationDbContext.Customers.AddAsync(customer);
            await _applicationDbContext.SaveChangesAsync();
            return addedCustomer.Entity;
        }      
      
		public async Task<IEnumerable<Order>> GetOrdersAsync()
		{
			return await _applicationDbContext.Orders.AsNoTracking().ToListAsync();
		}           

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _applicationDbContext.Orders.FindAsync(orderId);
		}       

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _applicationDbContext.Orders.Where(o => o.CustomerId == customerId).ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            var addedOrder = await _applicationDbContext.Orders.AddAsync(order);
            await _applicationDbContext.SaveChangesAsync();
            return addedOrder.Entity;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemAsync()
        {         
			return await _applicationDbContext.OrderItem.AsNoTracking().ToListAsync();
        }

		public async Task<OrderItem> AddOrderItemAsync(OrderItem orderItem)
		{         
			var addedOrderItem = await _applicationDbContext.OrderItem.AddAsync(orderItem);
            await _applicationDbContext.SaveChangesAsync();
			return addedOrderItem.Entity;
		}      
	}
}
