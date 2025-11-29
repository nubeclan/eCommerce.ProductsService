using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Dtos.Products;
using eCommerce.ProductsService.Application.UseCases.Products.Commands.CreateProduct;
using eCommerce.ProductsService.Application.UseCases.Products.Commands.DeleteProduct;
using eCommerce.ProductsService.Application.UseCases.Products.Commands.UpdateProduct;
using eCommerce.ProductsService.Application.UseCases.Products.Queries.GetAllProducts;
using eCommerce.ProductsService.Application.UseCases.Products.Queries.GetProductById;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.ProductsService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IDispatcher dispatcher) : ControllerBase
{
    private readonly IDispatcher _dispatcher = dispatcher;

    /// <summary>
    /// Obtiene todos los productos con filtros, paginación y ordenamiento
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch<GetAllProductsQuery, 
            IEnumerable<GetAllProductsResponseDto>>(query, cancellationToken);

        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery { ProductId = id };
        
        var response = await _dispatcher.Dispatch<GetProductByIdQuery, 
            GetProductByIdResponseDto>(query, cancellationToken);

        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch<CreateProductCommand, Guid>(
            command, cancellationToken);

        if (!response.IsSuccess)
            return BadRequest(response);

        return CreatedAtAction(
            nameof(GetProductById), 
            new { id = response.Data }, 
            response);
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        command.ProductId = id;
        
        var response = await _dispatcher.Dispatch<UpdateProductCommand, bool>(
            command, cancellationToken);

        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Elimina un producto por su ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand { ProductId = id };
        
        var response = await _dispatcher.Dispatch<DeleteProductCommand, bool>(
            command, cancellationToken);

        return response.IsSuccess ? NoContent() : NotFound(response);
    }
}
