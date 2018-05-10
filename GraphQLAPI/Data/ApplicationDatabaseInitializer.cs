using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQLAPI.Data
{
    public class ApplicationDatabaseInitializer
    {
        public async Task SeedAsync(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await applicationDbContext.Database.EnsureDeletedAsync();
                await applicationDbContext.Database.MigrateAsync();
                await applicationDbContext.Database.EnsureCreatedAsync();

                var items = new List<Item>
                {
                    new Item { Barcode= "123", Title="Headphone", SellingPrice=50},
                    new Item { Barcode= "456", Title="Keyboard", SellingPrice= 40},
                    new Item { Barcode= "789", Title="Monitor", SellingPrice= 100}
                };

                await applicationDbContext.Items.AddRangeAsync(items);

				await applicationDbContext.SaveChangesAsync();
            }
        }
    }
}