# Guía para Añadir Funcionalidades API REST

## ????? Desarrollador

* [Angel Céspedes Quiroz](https://bo.linkedin.com/in/acq1305)
* Correo: <angel@nubeando.com>

## ?? Introducción

Esta guía proporciona instrucciones paso a paso sobre cómo añadir nuevas funcionalidades a la API REST del proyecto **eCommerce Products Service**. Las adiciones se realizarán siguiendo los patrones **CQRS (Command Query Responsibility Segregation)** y **Clean Architecture**, garantizando coherencia y mantenibilidad del código.

> **Nota importante:** Este proyecto **NO utiliza MediatR**. Implementa un **sistema CQRS personalizado** a través de la interfaz `IDispatcher` y handlers propios.

## ?? Pre-requisitos

Antes de comenzar, asegúrate de tener instalados los siguientes componentes:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) - Framework de desarrollo
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/) - IDE
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) - Base de datos
- Conocimientos básicos de:
  - RESTful APIs
  - C# y .NET
  - Entity Framework Core
  - Patrones CQRS y Clean Architecture

## ??? Estructura del Proyecto

El proyecto está organizado en 4 capas:

### **Domain** (`src/eCommerce.ProductsService.Domain`)
- **Entidades del dominio** (sin lógica de negocio ni dependencias)
- Ejemplo: `Entities/Product.cs`

### **Application** (`src/eCommerce.ProductsService.Application`)
- **UseCases**: Queries y Commands organizados por entidad
- **Abstractions/Messaging**: Sistema CQRS personalizado
- **Dtos**: Objetos de transferencia de datos
- **Interfaces**: Contratos para repositorios y servicios
- **Commons/Bases**: Clases base reutilizables
- **Behaviors**: Servicios transversales (validaciones)

### **Infrastructure** (`src/eCommerce.ProductsService.Infrastructure`)
- **Persistence**: DbContext, Repositorios, Migraciones
- **Services**: Implementaciones de servicios (UnitOfWork, OrderingQuery)

### **Api** (`src/eCommerce.ProductsService.Api`)
- **Controllers**: Controladores REST
- **Program.cs**: Configuración de la aplicación

---

## ?? Añadiendo una Nueva Query (Consulta)

Las **Queries** se utilizan para **operaciones de lectura** (GET). Sigamos el ejemplo de obtener un producto por ID.

### **Paso 1: Crear el DTO de Respuesta**

Ubicación: `src/eCommerce.ProductsService.Application/Dtos/Products/`

Crea el archivo `GetProductByIdResponseDto.cs`:

```csharp
namespace eCommerce.ProductsService.Application.Dtos.Products;

public class GetProductByIdResponseDto
{
    public Guid ProductID { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

### **Paso 2: Crear la Query**

Ubicación: `src/eCommerce.ProductsService.Application/UseCases/Products/Queries/GetProductById/`

Crea el archivo `GetProductByIdQuery.cs`:

```csharp
using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Dtos.Products;

namespace eCommerce.ProductsService.Application.UseCases.Products.Queries.GetProductById;

public sealed class GetProductByIdQuery : IQuery<GetProductByIdResponseDto>
{
    public Guid ProductId { get; set; }
}
```

> **Importante:** La Query debe implementar `IQuery<TResponse>` donde `TResponse` es el tipo de dato que retornará.

### **Paso 3: Crear el Handler de la Query**

En la misma carpeta, crea `GetProductByIdHandler.cs`:

```csharp
using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Dtos.Products;
using eCommerce.ProductsService.Application.Interfaces.Services;
using Mapster;

namespace eCommerce.ProductsService.Application.UseCases.Products.Queries.GetProductById;

internal sealed class GetProductByIdHandler(IUnitOfWork unitOfWork) 
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<GetProductByIdResponseDto>> Handle(
        GetProductByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var response = new BaseResponse<GetProductByIdResponseDto>();

        try
        {
            var product = await _unitOfWork.ProductRepository
                .GetProductByIdAsync(query.ProductId, cancellationToken);

            if (product is null)
            {
                response.IsSuccess = false;
                response.Message = "Producto no encontrado.";
                return response;
            }

            response.IsSuccess = true;
            response.Data = product.Adapt<GetProductByIdResponseDto>();
            response.Message = "Consulta exitosa.";
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Ocurrió un error inesperado. {ex.Message}";
        }

        return response;
    }
}
```

> **Importante:** 
> - El Handler debe implementar `IQueryHandler<TQuery, TResponse>`
> - Debe ser `internal sealed` por convención
> - Utiliza **Primary Constructor** de C# 12
> - Retorna `BaseResponse<TResponse>`

### **Paso 4: Añadir el Endpoint en el Controlador**

Ubicación: `src/eCommerce.ProductsService.Api/Controllers/ProductController.cs`

Añade el nuevo endpoint:

```csharp
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
```

### **Paso 5: Probar el Endpoint**

Ejecuta la aplicación y prueba con:
```http
GET https://localhost:5001/api/Product/{guid-del-producto}
```

---

## ?? Añadiendo un Nuevo Command (Comando)

Los **Commands** se utilizan para **operaciones de escritura** (POST, PUT, DELETE). Sigamos el ejemplo de crear un producto.

### **Paso 1: Crear el DTO de Request**

Ubicación: `src/eCommerce.ProductsService.Application/Dtos/Products/`

Crea el archivo `CreateProductRequestDto.cs`:

```csharp
namespace eCommerce.ProductsService.Application.Dtos.Products;

public class CreateProductRequestDto
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

### **Paso 2: Crear el Command**

Ubicación: `src/eCommerce.ProductsService.Application/UseCases/Products/Commands/CreateProduct/`

Crea el archivo `CreateProductCommand.cs`:

```csharp
using eCommerce.ProductsService.Application.Abstractions.Messaging;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.CreateProduct;

public sealed class CreateProductCommand : ICommand<Guid>
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

> **Importante:** El Command debe implementar `ICommand<TResponse>` donde `TResponse` es el tipo de retorno (en este caso `Guid` del producto creado).

### **Paso 3: Crear el Validador (Opcional pero Recomendado)**

En la misma carpeta, crea `CreateProductValidator.cs`:

```csharp
using FluentValidation;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es requerida.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");
    }
}
```

### **Paso 4: Crear el Handler del Command**

En la misma carpeta, crea `CreateProductHandler.cs`:

```csharp
using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Commons.Bases;
using eCommerce.ProductsService.Application.Interfaces.Services;
using eCommerce.ProductsService.Domain.Entities;

namespace eCommerce.ProductsService.Application.UseCases.Products.Commands.CreateProduct;

internal sealed class CreateProductHandler(IUnitOfWork unitOfWork) 
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<BaseResponse<Guid>> Handle(
        CreateProductCommand command, 
        CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Guid>();

        try
        {
            var product = new Product
            {
                ProductID = Guid.NewGuid(),
                Name = command.Name,
                Category = command.Category,
                UnitPrice = command.UnitPrice,
                StockQuantity = command.StockQuantity
            };

            await _unitOfWork.ProductRepository.AddProductAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.IsSuccess = true;
            response.Data = product.ProductID;
            response.Message = "Producto creado exitosamente.";
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Error al crear el producto. {ex.Message}";
        }

        return response;
    }
}
```

> **Importante:** 
> - El Handler debe implementar `ICommandHandler<TCommand, TResponse>`
> - Debe llamar a `SaveChangesAsync()` del UnitOfWork para persistir los cambios

### **Paso 5: Añadir el Endpoint en el Controlador**

En `ProductController.cs`, añade:

```csharp
[HttpPost]
public async Task<IActionResult> CreateProduct(
    [FromBody] CreateProductCommand command,
    CancellationToken cancellationToken)
{
    var response = await _dispatcher.Dispatch<CreateProductCommand, Guid>(
        command, cancellationToken);

    return response.IsSuccess 
        ? CreatedAtAction(nameof(GetProductById), new { id = response.Data }, response)
        : BadRequest(response);
}
```

### **Paso 6: Probar el Endpoint**

```http
POST https://localhost:5001/api/Product
Content-Type: application/json

{
  "name": "Laptop Gaming",
  "category": "Electronics",
  "unitPrice": 1299.99,
  "stockQuantity": 10
}
```

---

## ?? Registro Automático de Handlers

**¡Buenas noticias!** No necesitas registrar manualmente cada handler. El método `AddHandlersFromAssembly()` en `DependencyInjection.cs` del proyecto Application registra automáticamente todos los handlers que implementan `ICommandHandler<,>` o `IQueryHandler<,>`.

```csharp
// Esto ya está configurado en Application/DependencyInjection.cs
services.AddHandlersFromAssembly(typeof(DependencyInjection).Assembly);
```

---

## ?? Estructura de BaseResponse

Todas las respuestas de la API utilizan `BaseResponse<T>`:

```csharp
public class BaseResponse<T>
{
    public bool IsSuccess { get; set; }           // Indica si la operación fue exitosa
    public T? Data { get; set; }                  // Datos de respuesta
    public string? Message { get; set; }          // Mensaje descriptivo
    public int TotalRecords { get; set; }         // Total de registros (para paginación)
    public IEnumerable<BaseError>? Errors { get; set; }  // Errores de validación
}
```

---

## ? Checklist para Añadir Funcionalidades

### Para Queries (GET):
- [ ] Crear DTO de respuesta
- [ ] Crear Query que implemente `IQuery<TResponse>`
- [ ] Crear QueryHandler que implemente `IQueryHandler<TQuery, TResponse>`
- [ ] Añadir endpoint en el Controller
- [ ] Probar el endpoint

### Para Commands (POST/PUT/DELETE):
- [ ] Crear Command que implemente `ICommand<TResponse>`
- [ ] Crear Validador con FluentValidation (opcional)
- [ ] Crear CommandHandler que implemente `ICommandHandler<TCommand, TResponse>`
- [ ] Llamar a `SaveChangesAsync()` en el handler
- [ ] Añadir endpoint en el Controller
- [ ] Probar el endpoint

---

## ?? Testing

Aunque este proyecto aún no tiene pruebas implementadas, es recomendable crear pruebas unitarias para:

- **Handlers**: Lógica de negocio
- **Validadores**: Reglas de validación
- **Repositorios**: Acceso a datos

Ejemplo de estructura:
```
tests/
??? eCommerce.ProductsService.Application.Tests/
??? eCommerce.ProductsService.Domain.Tests/
??? eCommerce.ProductsService.Api.Tests/
```

---

## ?? Recursos Adicionales

- [Clean Architecture por Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Mapster](https://github.com/MapsterMapper/Mapster)

---

## ?? Mejores Prácticas

1. **Mantén las capas separadas**: Domain no debe depender de nada, Application no debe conocer Infrastructure
2. **Usa DTOs**: No expongas entidades del dominio directamente en la API
3. **Valida siempre**: Utiliza FluentValidation para validar comandos
4. **Maneja errores**: Siempre usa try-catch y retorna mensajes descriptivos
5. **Usa async/await**: Todas las operaciones de I/O deben ser asíncronas
6. **Primary Constructors**: Utiliza la sintaxis moderna de C# 12
7. **Sealed classes**: Marca los handlers como `internal sealed`
8. **CancellationToken**: Siempre propágalo en operaciones asíncronas

---

## ? Preguntas Frecuentes

### ¿Por qué no usar MediatR?
Este proyecto implementa un sistema CQRS personalizado para tener control total sobre el flujo y evitar dependencias externas innecesarias.

### ¿Cómo se registran los handlers?
Automáticamente mediante reflexión en `DependencyInjection.cs` de la capa Application.

### ¿Dónde coloco la lógica de negocio?
En los handlers de la capa Application, nunca en los controladores.

### ¿Puedo usar otros mappers además de Mapster?
Sí, pero el proyecto está configurado con Mapster. Tendrías que modificar `DependencyInjection.cs`.

---

## ?? Contacto

Si tienes dudas o sugerencias sobre esta guía:

* [Angel Céspedes Quiroz](https://bo.linkedin.com/in/acq1305)
* Correo: <angel@nubeando.com>