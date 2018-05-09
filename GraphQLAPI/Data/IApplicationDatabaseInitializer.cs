using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace GraphQLAPI.Data
{
    public interface IApplicationDatabaseInitializer
    {
        Task SeedAsync(IApplicationBuilder app);
    }
}
