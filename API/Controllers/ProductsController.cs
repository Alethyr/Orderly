using API.RequestHelpers;
using Core.Entities;
using Core.Intrefaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repo) : ControllerBase
{
    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        var spec = new BrandListSpec(); 
        return Ok(await repo.ListAsync(spec));
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeListSpec();
        return Ok(await repo.ListAsync(spec)); 
    }

  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
    [FromQuery]ProductSpecParams specParams)
    {
        var spec = new ProductSpecification(specParams);
        var products = await repo.ListAsync(spec);
        var count = await repo.CountAsync(spec);
        var pagination = new Pagination<Product>(specParams.PageIndex,
                                                 specParams.PageSize,
                                                 count,
                                                 products);
        return Ok(pagination);
    }
   
   [HttpGet("{id:int}")]
   public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await repo.GetByIdAsync(id);
        if(product is null) return NotFound();
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        repo.Add(product);
        if(await repo.SaveChangesAsync())
        {
            return CreatedAtAction("GetProduct", new {id = product.Id}, product);
        }
        return BadRequest("Problem creating Product");
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateProduct(int id, Product product)
    {
        if(product.Id != id || !ProductExists(id))
        {
            return BadRequest("Cannot update this product");
        }
        repo.Update(product);
        if(await repo.SaveChangesAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem updating Product");
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await repo.GetByIdAsync(id);
        if(product is null) return NotFound();
        repo.Remove(product);
        if(await repo.SaveChangesAsync())
        {
            return NoContent();
        }
        return BadRequest("Problem deleting problem");
    }

    private bool ProductExists(int id)
    {
        return repo.Exists(id);
    }
}
