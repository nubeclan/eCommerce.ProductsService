# eCommerce Products Service API

Microservicio de gestión de productos desarrollado con **.NET 10** siguiendo los principios de **Clean Architecture** y **CQRS (Command Query Responsibility Segregation)**.

## ?? Descripción

API REST para la gestión de productos en un sistema de eCommerce. Implementa una arquitectura limpia con separación de responsabilidades en capas bien definidas.

## ??? Arquitectura del Proyecto

El proyecto está organizado en 4 capas siguiendo los principios de Clean Architecture:

### 1. **Domain** (`eCommerce.ProductsService.Domain`)
- Contiene las entidades del dominio
- Sin dependencias de otras capas
- **Entidades:**
  - `Product`: Entidad principal con ProductID (Guid), Name, Category, UnitPrice, StockQuantity

### 2. **Application** (`eCommerce.ProductsService.Application`)
- Casos de uso (Queries y Commands)
- Interfaces de servicios y repositorios
- DTOs (Data Transfer Objects)
- Implementación personalizada de CQRS
- Validaciones con FluentValidation
- Mapeo con Mapster

**Componentes principales:**
- `Abstractions/Messaging`: Implementación personalizada de CQRS
  - `IDispatcher`: Despachador de comandos y queries
  - `ICommand<TResponse>` / `ICommandHandler<TCommand, TResponse>`
  - `IQuery<TResponse>` / `IQueryHandler<TQuery, TResponse>`
- `UseCases`: Casos de uso organizados por entidad
- `Commons/Bases`: Clases base (BaseResponse, BasePagination, BaseFilters, BaseError)
- `Behaviors`: Servicios transversales (ValidationService)

### 3. **Infrastructure** (`eCommerce.ProductsService.Infrastructure`)
- Implementación de repositorios
- Configuración de Entity Framework Core
- Migraciones de base de datos
- Servicios de infraestructura (UnitOfWork, OrderingQuery)
- **Base de datos:** MySQL
- **ORM:** Entity Framework Core

### 4. **API** (`eCommerce.ProductsService.Api`)
- Controladores REST
- Configuración de la aplicación
- Punto de entrada de la aplicación

## ??? Tecnologías Utilizadas

- **.NET 10**: Framework principal
- **ASP.NET Core Web API**: Para crear la API REST
- **Entity Framework Core**: ORM para acceso a datos
- **MySQL**: Base de datos relacional
- **Mapster**: Mapeo de objetos
- **FluentValidation**: Validación de modelos
- **CQRS personalizado**: Patrón de diseño implementado sin MediatR

## ?? Prerequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [Visual Studio Code](https://code.visualstudio.com/)

## ?? Instalación y Ejecución

### 1. Clonar el repositorio
```bash
git clone https://github.com/nubeclan/eCommerce.ProductsService.git
cd eCommerce.ProductsService
```

### 2. Configurar la cadena de conexión
Edita `appsettings.json` en el proyecto `eCommerce.ProductsService.Api`:
```json
{
  "ConnectionStrings": {
    "ProductsServiceConnection": "server=localhost;database=ProductsServiceDB;user=tu_usuario;password=tu_password"
  }
}
```

### 3. Aplicar migraciones
```bash
dotnet ef database update --project src/eCommerce.ProductsService.Infrastructure --startup-project src/eCommerce.ProductsService.Api
```

### 4. Ejecutar la aplicación
```bash
cd src/eCommerce.ProductsService.Api
dotnet run
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

## ?? Endpoints Disponibles

### **GET** `/api/Product`
Obtiene todos los productos con filtros, paginación y ordenamiento.

**Parámetros Query (opcionales):**
- `NumPage`: Número de página (default: 1)
- `NumRecordsPage`: Registros por página (default: 10, máx: 50)
- `Sort`: Campo por el cual ordenar (default: "ProductID")
- `Order`: Orden ascendente o descendente ("ASC" o "DESC", default: "ASC")
- `NumFilter`: Tipo de filtro (1: por nombre, 2: por categoría)
- `TextFilter`: Texto a buscar según el NumFilter

**Ejemplo de solicitud:**
```http
GET /api/Product?NumPage=1&NumRecordsPage=10&Sort=Name&Order=ASC&NumFilter=1&TextFilter=Laptop
```

**Respuesta exitosa (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
      "productID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Laptop Gaming",
      "category": "Electronics",
      "unitPrice": 1299.99,
      "stockQuantity": 15
    }
  ],
  "message": "Consulta exitosa.",
  "totalRecords": 1,
  "errors": null
}
```

### **GET** `/api/Product/{id}`
Obtiene un producto específico por su ID.

**Parámetros de ruta:**
- `id`: GUID del producto

**Respuesta exitosa (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "productID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Laptop Gaming",
    "category": "Electronics",
    "unitPrice": 1299.99,
    "stockQuantity": 15
  },
  "message": "Consulta exitosa.",
  "totalRecords": 0,
  "errors": null
}
```

### **POST** `/api/Product`
Crea un nuevo producto.

**Cuerpo de la solicitud:**
```json
{
  "name": "Laptop Gaming MSI",
  "category": "Electronics",
  "unitPrice": 1299.99,
  "stockQuantity": 15
}
```

**Respuesta exitosa (201 Created):**
```json
{
  "isSuccess": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Producto creado exitosamente.",
  "totalRecords": 0,
  "errors": null
}
```

### **PUT** `/api/Product/{id}`
Actualiza un producto existente.

**Parámetros de ruta:**
- `id`: GUID del producto

**Cuerpo de la solicitud:**
```json
{
  "name": "Laptop Gaming MSI - Actualizado",
  "category": "Electronics",
  "unitPrice": 1399.99,
  "stockQuantity": 20
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "isSuccess": true,
  "data": true,
  "message": "Producto actualizado exitosamente.",
  "totalRecords": 0,
  "errors": null
}
```

### **DELETE** `/api/Product/{id}`
Elimina un producto por su ID.

**Parámetros de ruta:**
- `id`: GUID del producto

**Respuesta exitosa (204 No Content)**

### **Respuestas de Error**
**Respuesta con error (400 Bad Request / 404 Not Found):**
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Producto no encontrado.",
  "totalRecords": 0,
  "errors": [
    {
      "propertyName": "Name",
      "errorMessage": "El nombre del producto es requerido."
    }
  ]
}
```

## ?? Estructura de Directorios

```
eCommerce.ProductsService/
??? src/
?   ??? eCommerce.ProductsService.Api/
?   ?   ??? Controllers/
?   ?   ?   ??? ProductController.cs
?   ?   ??? Program.cs
?   ?   ??? appsettings.json
?   ?
?   ??? eCommerce.ProductsService.Application/
?   ?   ??? Abstractions/
?   ?   ?   ??? Messaging/
?   ?   ?       ??? IDispatcher.cs
?   ?   ?       ??? Dispatcher.cs
?   ?   ?       ??? ICommand.cs
?   ?   ?       ??? ICommandHandler.cs
?   ?   ?       ??? IQuery.cs
?   ?   ?       ??? IQueryHandler.cs
?   ?   ??? Commons/
?   ?   ?   ??? Bases/
?   ?   ?       ??? BaseResponse.cs
?   ?   ?       ??? BasePagination.cs
?   ?   ?       ??? BaseFilters.cs
?   ?   ?       ??? BaseError.cs
?   ?   ??? Dtos/
?   ?   ?   ??? Products/
?   ?   ?       ??? GetAllProductsResponseDto.cs
?   ?   ?       ??? GetProductByIdResponseDto.cs
?   ?   ??? Interfaces/
?   ?   ?   ??? Persistence/
?   ?   ?   ?   ??? IProductRepository.cs
?   ?   ?   ??? Services/
?   ?   ?       ??? IUnitOfWork.cs
?   ?   ?       ??? IOrderingQuery.cs
?   ?   ??? UseCases/
?   ?   ?   ??? Products/
?   ?   ?       ??? Queries/
?   ?   ?       ?   ??? GetAllProducts/
?   ?   ?       ?   ?   ??? GetAllProductsQuery.cs
?   ?   ?       ?   ?   ??? GetAllProductsHandler.cs
?   ?   ?       ?   ??? GetProductById/
?   ?   ?       ?       ??? GetProductByIdQuery.cs
?   ?   ?       ?       ??? GetProductByIdHandler.cs
?   ?   ?       ??? Commands/
?   ?   ?           ??? CreateProduct/
?   ?   ?           ?   ??? CreateProductCommand.cs
?   ?   ?           ?   ??? CreateProductValidator.cs
?   ?   ?           ?   ??? CreateProductHandler.cs
?   ?   ?           ??? UpdateProduct/
?   ?   ?           ?   ??? UpdateProductCommand.cs
?   ?   ?           ?   ??? UpdateProductValidator.cs
?   ?   ?           ?   ??? UpdateProductHandler.cs
?   ?   ?           ??? DeleteProduct/
?   ?   ?               ??? DeleteProductCommand.cs
?   ?   ?               ??? DeleteProductValidator.cs
?   ?   ?               ??? DeleteProductHandler.cs
?   ?   ??? Behaviors/
?   ?   ?   ??? IValidationService.cs
?   ?   ?   ??? ValidationService.cs
?   ?   ??? DependencyInjection.cs
?   ?
?   ??? eCommerce.ProductsService.Domain/
?   ?   ??? Entities/
?   ?       ??? Product.cs
?   ?
?   ??? eCommerce.ProductsService.Infrastructure/
?       ??? Persistence/
?       ?   ??? Context/
?       ?   ?   ??? ApplicationDbContext.cs
?       ?   ??? Repositories/
?       ?   ?   ??? ProductRepository.cs
?       ?   ??? Migrations/
?       ??? Services/
?       ?   ??? UnitOfWork.cs
?       ?   ??? OrderingQuery.cs
?       ??? DependencyInjection.cs
?
??? README.md
??? TUTORIAL.md
```

## ?? Patrón CQRS Personalizado

Este proyecto implementa CQRS sin utilizar MediatR, con una implementación personalizada:

- **Queries**: Para operaciones de lectura (GET)
- **Commands**: Para operaciones de escritura (POST, PUT, DELETE)
- **Dispatcher**: Enrutador central que despacha requests a sus handlers correspondientes
- **Handlers**: Lógica de negocio para cada query o command

## ?? Próximas Funcionalidades

- [x] Crear producto (POST)
- [x] Obtener producto por ID (GET)
- [x] Actualizar producto (PUT)
- [x] Eliminar producto (DELETE)
- [x] Validaciones con FluentValidation
- [ ] Pruebas unitarias
- [ ] Documentación con Swagger/OpenAPI

## ????? Desarrollador

* [Angel Céspedes Quiroz](https://bo.linkedin.com/in/acq1305)
* Correo: <angel@nubeando.com>

## ?? Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.