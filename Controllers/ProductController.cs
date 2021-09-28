using System.Security.Authentication.ExtendedProtection;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG_Core_Auth.Data;
using NG_Core_Auth.Models;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductController : ControllerBase
    {

        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }


        //GET:api
        [HttpGet("[action]")]
        [Authorize(Policy = "RequiredLoggedIn")]
        public IActionResult GetProducts() => Ok(_db.Products.ToList());


        //POST: api
        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdminRole")]

        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newProduct = new Product
            {
                Title = product.Title,
                Description = product.Description,
                OutOfStock = product.OutOfStock,
                ImageUrl = product.ImageUrl,
                Price = product.Price
            };

            await _db.Products.AddAsync(newProduct);
            await _db.SaveChangesAsync();

            return Ok();

        }

        //PUT:api/product/UpdateProduct/5
        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdminRole")]

        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var FindProduct = _db.Products.FirstOrDefault(p => p.ProductId == id);

            if (FindProduct == null)
            {
                return NotFound();
            }

            FindProduct.Title = product.Title;
            FindProduct.Description = product.Description;
            FindProduct.ImageUrl = product.ImageUrl;
            FindProduct.OutOfStock = product.OutOfStock;
            FindProduct.ProductId = product.ProductId;

            _db.Entry(FindProduct).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Product With Id" + id + "Updated"));
        }

        //PUT:api/product/DeleteProduct/5
        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequireAdminRole")]

        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findproduct = await _db.Products.FindAsync(id);

            if (findproduct == null)
            {
                return NotFound();
            }

            _db.Products.Remove(findproduct);
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("Product Deleted!"));

        }

    }
}