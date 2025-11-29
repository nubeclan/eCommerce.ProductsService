using eCommerce.ProductsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;

namespace eCommerce.ProductsService.Infrastructure.Persistence.Context;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IConfiguration configuration) : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public IDbConnection CreateConnection()
        => new MySqlConnection(_configuration
            .GetConnectionString("ProductsServiceConnection"));
}
