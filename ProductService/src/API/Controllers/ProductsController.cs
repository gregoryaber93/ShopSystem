using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Abstractions.CQRS;
using ProductService.Application.Features.Products.Commands.AddProduct;
using ProductService.Application.Features.Products.Commands.DeleteProduct;
using ProductService.Application.Features.Products.Commands.UpdateProduct;
using ProductService.Application.Features.Products.Queries.GetProductById;
using ProductService.Application.Features.Products.Queries.GetProductsByShop;
using ProductService.Contracts.Dtos;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,User")]
public class ProductsController(
    IQueryHandler<GetProductsByShopQuery, IReadOnlyCollection<ProductDto>> getProductsByShopQueryHandler,
    IQueryHandler<GetProductByIdQuery, ProductDto?> getProductByIdQueryHandler,
    ICommandHandler<AddProductCommand, ProductDto> addProductCommandHandler,
    ICommandHandler<UpdateProductCommand, ProductDto?> updateProductCommandHandler,
    ICommandHandler<DeleteProductCommand, bool> deleteProductCommandHandler) : ControllerBase
{
    [HttpGet("shop/{shopId:guid}")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> GetProductsByShop(Guid shopId, CancellationToken cancellationToken)
    {
        try
        {
            var products = await getProductsByShopQueryHandler.Handle(new GetProductsByShopQuery(shopId), cancellationToken);
            return Ok(products);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await getProductByIdQueryHandler.Handle(new GetProductByIdQuery(id), cancellationToken);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<ProductDto>> AddProduct([FromBody] ProductDto productDto, CancellationToken cancellationToken)
    {
        try
        {
            var createdProduct = await addProductCommandHandler.Handle(new AddProductCommand(productDto), cancellationToken);
            return Created($"/api/products/{createdProduct.Id}", createdProduct);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] ProductDto productDto, CancellationToken cancellationToken)
    {
        try
        {
            var updatedProduct = await updateProductCommandHandler.Handle(new UpdateProductCommand(id, productDto), cancellationToken);
            if (updatedProduct is null)
            {
                return NotFound();
            }

            return Ok(updatedProduct);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await deleteProductCommandHandler.Handle(new DeleteProductCommand(id), cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
