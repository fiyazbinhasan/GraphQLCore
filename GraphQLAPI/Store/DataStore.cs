using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLAPI.Data;

namespace GraphQLAPI.Store
{
    public class DataStore : IDataStore
    {
		private readonly ApplicationDbContext _applicationDbContext;

		public DataStore(ApplicationDbContext applicationDbContext)
        {
			_applicationDbContext = applicationDbContext;
        }

        public async Task<Item> AddItem(Item item)
        {
            var addedItem = await _applicationDbContext.Items.AddAsync(item);
            return addedItem.Entity;
        }

        public Item GetItemByBarcode(string barcode)
        {
			return _applicationDbContext.Items.First(i => i.Barcode.Equals(barcode));
        }

        public IEnumerable<Item> GetItems()
        {
			return _applicationDbContext.Items;
        }
    }
}
