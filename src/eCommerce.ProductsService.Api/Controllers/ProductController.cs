using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
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
    /// <param name="query">Parámetros de consulta para filtrado, paginación y ordenamiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista paginada de productos</returns>
    /// <response code="200">Lista de productos obtenida exitosamente</response>
    /// <response code="400">Error en los parámetros de consulta</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     GET /api/Product?PageNumber=1&amp;PageSize=10&amp;OrderBy=Name&amp;OrderDirection=asc
    /// 
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<GetAllProductsResponseDto>>), 200)]
    [ProducesResponseType(400)]
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
    /// <param name="id">ID único del producto (GUID)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalles del producto solicitado</returns>
    /// <response code="200">Producto encontrado</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<GetProductByIdResponseDto>), 200)]
    [ProducesResponseType(404)]
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
    /// <param name="command">Datos del producto a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ID del producto creado</returns>
    /// <response code="201">Producto creado exitosamente</response>
    /// <response code="400">Datos inválidos o errores de validación</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     POST /api/Product
    ///     {
    ///         "name": "Laptop Gaming MSI",
    ///         "category": "Electronics",
    ///         "unitPrice": 1299.99,
    ///         "stockQuantity": 15
    ///     }
    ///     
    /// Otros ejemplos:
    /// 
    ///     Mouse Gaming:
    ///     {
    ///         "name": "Mouse Logitech G502",
    ///         "category": "Accessories",
    ///         "unitPrice": 79.99,
    ///         "stockQuantity": 50
    ///     }
    ///     
    ///     Teclado Mecánico:
    ///     {
    ///         "name": "Teclado Mecánico Corsair K95",
    ///         "category": "Accessories",
    ///         "unitPrice": 199.99,
    ///         "stockQuantity": 30
    ///     }
    /// 
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<Guid>), 201)]
    [ProducesResponseType(400)]
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
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="command">Datos actualizados del producto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de actualización</returns>
    /// <response code="200">Producto actualizado exitosamente</response>
    /// <response code="400">Datos inválidos o producto no encontrado</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     PUT /api/Product/{id}
    ///     {
    ///         "name": "Laptop Gaming MSI - ACTUALIZADO",
    ///         "category": "Electronics",
    ///         "unitPrice": 1399.99,
    ///         "stockQuantity": 20
    ///     }
    /// 
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    [ProducesResponseType(400)]
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
    /// <param name="id">ID del producto a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Producto eliminado exitosamente</response>
    /// <response code="404">Producto no encontrado</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
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
