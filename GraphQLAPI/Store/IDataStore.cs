using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQLAPI.Store
{
    public interface IDataStore
    {
        IEnumerable<Item> GetItems();
        Item GetItemByBarcode(string barcode);
        Task<Item> AddItem(Item item);
    }
}