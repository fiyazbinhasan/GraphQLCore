using System.Collections.Generic;

namespace GraphQLAPI.Store
{
    public interface IDataStore
    {
        IEnumerable<Item> GetItems();
        Item GetItemByBarcode(string barcode);
    }
}