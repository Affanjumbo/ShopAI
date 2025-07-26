using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.DTOs.ProductDTOs;
using ShopAI.DTOs;
using ShopAI.Models;

namespace ShopAI.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/products/{uniqueFileName}";
        }

        private async Task<bool> IsValidSubCategory(int? subCategoryId, int categoryId)
        {
            if (categoryId == 16) // Groceries
            {
                return subCategoryId != null && await _context.SubCategories.AnyAsync(sc => sc.Id == subCategoryId && sc.CategoryId == categoryId);
            }
            else
            {
                return subCategoryId == null || await _context.SubCategories.AnyAsync(sc => sc.Id == subCategoryId && sc.CategoryId == categoryId);
            }
        }

        public async Task<ApiResponse<ProductResponseDTO>> CreateProductAsync(ProductCreateDTO productDto)
        {
            try
            {
                if (await _context.Products.AnyAsync(p => p.Name.ToLower() == productDto.Name.ToLower()))
                    return new ApiResponse<ProductResponseDTO>(400, "Product name already exists.");

                if (!await _context.Categories.AnyAsync(cat => cat.Id == productDto.CategoryId))
                    return new ApiResponse<ProductResponseDTO>(400, "Specified category does not exist.");

                if (!await IsValidSubCategory(productDto.SubCategoryId, productDto.CategoryId))
                    return new ApiResponse<ProductResponseDTO>(400, "Invalid or missing subcategory for selected category.");

                var imageUrl = await SaveImageAsync(productDto.ProductImage);

                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    SellerId = productDto.SellerId,
                    Price = productDto.Price,
                    StockQuantity = productDto.StockQuantity,
                    ProductImage = imageUrl,
                    DiscountPercentage = productDto.DiscountPercentage,
                    CategoryId = productDto.CategoryId,
                    SubCategoryId = productDto.SubCategoryId,
                    IsAvailable = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var response = new ProductResponseDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    SellerId = product.SellerId,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    ProductImage = product.ProductImage,
                    DiscountPercentage = product.DiscountPercentage,
                    CategoryId = product.CategoryId,
                    SubCategoryId = product.SubCategoryId,
                    IsAvailable = product.IsAvailable
                };

                return new ApiResponse<ProductResponseDTO>(200, response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProductResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductAsync(ProductUpdateDTO productDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(productDto.Id);
                if (product == null)
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");

                if (!string.IsNullOrWhiteSpace(productDto.Name))
                {
                    if (await _context.Products.AnyAsync(p => p.Name.ToLower() == productDto.Name.ToLower() && p.Id != productDto.Id))
                        return new ApiResponse<ConfirmationResponseDTO>(400, "Another product with the same name already exists.");
                    product.Name = productDto.Name;
                }

                if (!string.IsNullOrWhiteSpace(productDto.Description))
                    product.Description = productDto.Description;

                if (productDto.SellerId != 0)
                    product.SellerId = productDto.SellerId;

                if (productDto.Price != 0)
                    product.Price = productDto.Price;

                if (productDto.StockQuantity != 0)
                    product.StockQuantity = productDto.StockQuantity;

                if (productDto.DiscountPercentage != 0)
                    product.DiscountPercentage = productDto.DiscountPercentage;

                if (productDto.CategoryId != 0)
                {
                    if (!await _context.Categories.AnyAsync(cat => cat.Id == productDto.CategoryId))
                        return new ApiResponse<ConfirmationResponseDTO>(400, "Specified category does not exist.");
                    product.CategoryId = productDto.CategoryId;
                }

                if (productDto.SubCategoryId != 0 && productDto.CategoryId != 0)
                {
                    if (!await IsValidSubCategory(productDto.SubCategoryId, productDto.CategoryId))
                        return new ApiResponse<ConfirmationResponseDTO>(400, "Invalid or missing subcategory for selected category.");
                    product.SubCategoryId = productDto.SubCategoryId;
                }

                if (productDto.ProductImage != null)
                {
                    var imageUrl = await SaveImageAsync(productDto.ProductImage);
                    product.ProductImage = imageUrl;
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<ConfirmationResponseDTO>(200, new ConfirmationResponseDTO
                {
                    Message = $"Product with Id {productDto.Id} updated successfully."
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        public async Task<ApiResponse<ProductResponseDTO>> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ApiResponse<ProductResponseDTO>(404, "Product not found.");

            var response = new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SellerId = product.SellerId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ProductImage = product.ProductImage,
                DiscountPercentage = product.DiscountPercentage,
                CategoryId = product.CategoryId,
                SubCategoryId = product.SubCategoryId,
                IsAvailable = product.IsAvailable
            };

            return new ApiResponse<ProductResponseDTO>(200, response);
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return new ApiResponse<ConfirmationResponseDTO>(200, new ConfirmationResponseDTO
            {
                Message = $"Product with Id {id} deleted successfully."
            });
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsAsync()
        {
            var products = await _context.Products.ToListAsync();

            var response = products.Select(product => new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                SellerId = product.SellerId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ProductImage = product.ProductImage,
                DiscountPercentage = product.DiscountPercentage,
                CategoryId = product.CategoryId,
                SubCategoryId = product.SubCategoryId,
                IsAvailable = product.IsAvailable
            }).ToList();

            return new ApiResponse<List<ProductResponseDTO>>(200, response);
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetAllProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                    .ToListAsync();

                if (products == null || products.Count == 0)
                {
                    return new ApiResponse<List<ProductResponseDTO>>(404, "Products not found.");
                }

                var productList = products.Select(p => new ProductResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SellerId = p.SellerId,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ProductImage = p.ProductImage,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    IsAvailable = p.IsAvailable
                }).ToList();

                return new ApiResponse<List<ProductResponseDTO>>(200, productList);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ProductResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> UpdateProductStatusAsync(ProductStatusUpdateDTO productStatusUpdateDTO)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productStatusUpdateDTO.ProductId);

                if (product == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found.");
                }

                product.IsAvailable = productStatusUpdateDTO.IsAvailable;
                await _context.SaveChangesAsync();

                var confirmationMessage = new ConfirmationResponseDTO
                {
                    Message = $"Product with Id {productStatusUpdateDTO.ProductId} Status Updated successfully."
                };

                return new ApiResponse<ConfirmationResponseDTO>(200, confirmationMessage);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"An unexpected error occurred: {ex.Message}, Inner: {ex.InnerException?.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductResponseDTO>>> GetProductsBySellerAsync(int sellerId)
        {
            try
            {
                var products = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.SellerId == sellerId)
                    .ToListAsync();

                if (products == null || products.Count == 0)
                {
                    return new ApiResponse<List<ProductResponseDTO>>(404, "No products found for this seller.");
                }

                var productList = products.Select(p => new ProductResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SellerId = p.SellerId,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ProductImage = p.ProductImage,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryId = p.CategoryId,
                    IsAvailable = p.IsAvailable
                }).ToList();

                return new ApiResponse<List<ProductResponseDTO>>(200, productList);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ProductResponseDTO>>(500, $"An unexpected error occurred while processing your request, Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> AddOrUpdateDiscountAsync(int sellerId, int productId, int discountPercentage)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);

                if (product == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found for this seller.");
                }

                product.DiscountPercentage = discountPercentage;
                await _context.SaveChangesAsync();

                return new ApiResponse<ConfirmationResponseDTO>(200, new ConfirmationResponseDTO
                {
                    Message = $"Discount of {discountPercentage}% applied to product {product.Name}"
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"Error applying discount: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ConfirmationResponseDTO>> DeleteDiscountAsync(int sellerId, int productId)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);

                if (product == null)
                {
                    return new ApiResponse<ConfirmationResponseDTO>(404, "Product not found for this seller.");
                }

                product.DiscountPercentage = 0;
                await _context.SaveChangesAsync();

                return new ApiResponse<ConfirmationResponseDTO>(200, new ConfirmationResponseDTO
                {
                    Message = $"Discount removed from product {product.Name} (ID: {productId})"
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse<ConfirmationResponseDTO>(500, $"Error removing discount: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SubCategoryDTO>>> GetAllSubcategoriesAsync()
        {
            var subcategories = await _context.SubCategories
                .Select(sc => new SubCategoryDTO
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId
                }).ToListAsync();

            return new ApiResponse<List<SubCategoryDTO>>(200, subcategories);
        }

        public async Task<ApiResponse<List<SubCategoryDTO>>> GetSubcategoriesByCategoryIdAsync(int categoryId)
        {
            var subcategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new SubCategoryDTO
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId
                }).ToListAsync();

            return new ApiResponse<List<SubCategoryDTO>>(200, subcategories);
        }

        public async Task<List<ProductResponseDTO>> GetProductsByIdsAsync(List<int> productIds)
        {
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            return products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                SellerId = p.SellerId,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ProductImage = p.ProductImage,
                DiscountPercentage = p.DiscountPercentage,
                CategoryId = p.CategoryId,
                SubCategoryId = p.SubCategoryId,
                IsAvailable = p.IsAvailable
            }).ToList();
        }


    }
}
