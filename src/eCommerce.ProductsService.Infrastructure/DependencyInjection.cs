using eCommerce.ProductsService.Application.Interfaces.Persistence;
using eCommerce.ProductsService.Application.Interfaces.Services;
using eCommerce.ProductsService.Infrastructure.Persistence.Context;
using eCommerce.ProductsService.Infrastructure.Persistence.Repositories;
using eCommerce.ProductsService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var assembly = typeof(ApplicationDbContext).Assembly.FullName;

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseMySQL(
            configuration.GetConnectionString("ProductsServiceConnection")!,
            x => x.MigrationsAssembly(assembly)));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<IOrderingQuery, OrderingQuery>();

        return services;
    }
}
