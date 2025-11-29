using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Dtos.Products;
using eCommerce.ProductsService.Application.UseCases.Products.Queries.GetAllProducts;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.ProductsService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IDispatcher dispatcher) : ControllerBase
{
    private readonly IDispatcher _dispatcher = dispatcher;

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch<GetAllProductsQuery, 
            IEnumerable<GetAllProductsResponseDto>>(query, cancellationToken);

        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}
