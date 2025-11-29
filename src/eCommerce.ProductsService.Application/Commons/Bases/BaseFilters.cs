namespace eCommerce.ProductsService.Application.Commons.Bases;

public class BaseFilters : BasePagination
{
    public int? NumFilter { get; set; }
    public string? TextFilter { get; set; }
}
