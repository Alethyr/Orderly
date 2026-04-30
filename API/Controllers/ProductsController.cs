using API.RequestHelpers;
using Core.Entities;
using Core.Intrefaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController(IUnitOfWork unit) : BaseApiController
{
    /// <summary>
    /// Получение списка всех брендов продуктов.
    /// </summary>
    /// <response code="200">Список брендов. Возвращает пустой массив если брендов нет.</response>
    [HttpGet("brands")]
    [Cache(10000)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    { 
        var spec = new BrandListSpec();
        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }

    /// <summary>
    /// Получение списка всех типов продуктов.
    /// </summary>
    /// <response code="200">Список типов. Возвращает пустой массив если типов нет.</response>
    [HttpGet("types")]
    [Cache(10000)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeListSpec();
        return Ok(await unit.Repository<Product>().ListAsync(spec));
    }

    /// <summary>
    /// Получение постраничного списка всех продуктов.
    /// </summary>
    /// <remarks>
    /// Возвращает постраничный список продуктов с возможностью фильтрации.
    /// Параметры запроса:
    /// - Brands: бренды 
    /// - Types: типы 
    /// - Sort: сортировка ("priceAsc", "priceDesc"), по умолчанию nameAsc
    /// - Search: поисковый запрос по названию
    /// - PageIndex: номер страницы (по умолчанию 1)
    /// - PageSize: размер страницы (максимум 50)
    /// </remarks>
    /// <response code="200">Постраничный список продуктов. Возвращает пустой массив если продуктов нет.</response>
    [HttpGet]
    [Cache(600)]
    [ProducesResponseType(typeof(Pagination<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams productParams)
    {
        var spec = new ProductSpecification(productParams);
        return await CreatePagedResult(unit.Repository<Product>(), spec, 
            productParams.PageIndex, productParams.PageSize);
    }

    /// <summary>
    /// Получение продукта по id.
    /// </summary>
    /// <response code="200">Продукт по id успешно найден.</response>
    /// <response code="404">Продукт с указанным id не найден.</response>
    /// <param name="id">Идентификатор товара</param>
    [HttpGet("{id:int}")]
    [Cache(300)]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product is null) return NotFound();
        return product;
    }

    /// <summary>
    /// Создание нового продукта.
    /// </summary>
    /// <response code="201">Продукт успешно создан.</response>
    /// <response code="400">Ошибка при создании продукта.</response>
    /// <param name="product">Продукт который необходимо создать</param>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [InvalidateCache("api/products|")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        unit.Repository<Product>().Add(product);
        if (await unit.Complete())
        {
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }
        return BadRequest("Problem creating Product");
    }

    /// <summary>
    /// Обновление существующего продукта.
    /// </summary>
    /// <response code="204">Продукт успешно обновлён.</response>
    /// <response code="400">Ошибка при обновлении продукта.</response>
    /// <response code="404">Продукт с указанным id не найден.</response>
    /// <response code="409">Id в URL не совпадает с Id в теле запроса.</response>
    /// <param name="id">Идентификатор продукта</param>
    /// <param name="product">Новые данные продукта</param>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [InvalidateCache("api/products|")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if (product.Id != id) return Conflict("Product id mismatch");
        if (await unit.Repository<Product>().ExistsAsync(id) is false) return NotFound();
            
        unit.Repository<Product>().Update(product);

        if (await unit.Complete())
        {
            return NoContent();
        }
         
        return BadRequest("Problem updating Product");
    }

    /// <summary>
    /// Удаление существующего продукта.
    /// </summary>
    /// <response code="204">Продукт успешно удалён.</response>
    /// <response code="400">Ошибка при удалении продукта.</response>
    /// <response code="404">Продукт с указанным id не найден.</response>
    /// <param name="id">Идентификатор продукта</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [InvalidateCache("api/products|")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await unit.Repository<Product>().GetByIdAsync(id);
        if (product is null) return NotFound();
        unit.Repository<Product>().Remove(product);
        if (await unit.Complete())
        {
            return NoContent();
        }
        return BadRequest("Problem with deleting product");
    }
}
