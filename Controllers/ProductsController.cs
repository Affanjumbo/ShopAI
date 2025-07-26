using Microsoft.AspNetCore.Mvc;
using ShopAI.DTOs.ProductDTOs;
using ShopAI.DTOs;
using ShopAI.Services;

namespace ShopAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // Creates a new product (with image upload)
        [HttpPost("CreateProduct")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> CreateProduct([FromForm] ProductCreateDTO productDto)
        {
            var response = await _productService.CreateProductAsync(productDto);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves a product by ID
        [HttpGet("GetProductById/{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> GetProductById(int id)
        {
            var response = await _productService.GetProductByIdAsync(id);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Updates an existing product (with image upload)
        [HttpPut("UpdateProduct")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdateProduct([FromForm] ProductUpdateDTO productDto)
        {
            var response = await _productService.UpdateProductAsync(productDto);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Deletes a product by ID
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> DeleteProduct(int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves all products
        [HttpGet("GetAllProducts")]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDTO>>>> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync();
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Retrieves all products by category
        [HttpGet("GetAllProductsByCategory/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDTO>>>> GetAllProductsByCategory(int categoryId)
        {
            var response = await _productService.GetAllProductsByCategoryAsync(categoryId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        [HttpGet("GetProductsBySeller/{sellerId}")]
        public async Task<ActionResult<ApiResponse<List<ProductResponseDTO>>>> GetProductsBySeller(int sellerId)
        {
            var response = await _productService.GetProductsBySellerAsync(sellerId);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }





        // Update Product Status
        [HttpPut("UpdateProductStatus")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponseDTO>>> UpdateProductStatus(ProductStatusUpdateDTO productStatusUpdateDTO)
        {
            var response = await _productService.UpdateProductStatusAsync(productStatusUpdateDTO);
            if (response.StatusCode != 200)
            {
                return StatusCode(response.StatusCode, response);
            }
            return Ok(response);
        }

        // Add or update discount for a product
        [HttpPost("SetProductDiscount")]
        public async Task<IActionResult> AddOrUpdateDiscount([FromBody] DiscountRequestDTO request)
        {
            var result = await _productService.AddOrUpdateDiscountAsync(
                request.SellerId,
                request.ProductId,
                request.DiscountPercentage
            );

            return StatusCode(result.StatusCode, result);
        }

        // Delete discount for a product
        [HttpDelete("RemoveProductDiscount")]
        public async Task<IActionResult> DeleteDiscount([FromQuery] int sellerId, [FromQuery] int productId)
        {
            var result = await _productService.DeleteDiscountAsync(sellerId, productId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetAllSubcategories")]
        public async Task<ActionResult<ApiResponse<List<SubCategoryDTO>>>> GetAllSubcategories()
        {
            var response = await _productService.GetAllSubcategoriesAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetSubcategoriesByCategory/{categoryId}")]
public async Task<ActionResult<ApiResponse<List<SubCategoryDTO>>>> GetSubcategoriesByCategory(int categoryId)
{
    var response = await _productService.GetSubcategoriesByCategoryIdAsync(categoryId);
    return StatusCode(response.StatusCode, response);
}

    }
}