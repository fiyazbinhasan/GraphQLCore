using System.Collections.Generic;
using System.Linq;
using GraphQLAPI.Data;

namespace GraphQLAPI.Store
{
    public class DataStore : IDataStore
    {
		private ApplicationDbContext _applicationDbContext;

		public DataStore(ApplicationDbContext applicationDbContext)
        {
			_applicationDbContext = applicationDbContext;
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
