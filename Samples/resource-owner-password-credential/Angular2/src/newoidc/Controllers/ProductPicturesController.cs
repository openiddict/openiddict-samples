using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newoidc.Data;
using newoidc.Models;

namespace newoidc.Controllers
{
    [Produces("application/json")]
    [Route("api/ProductPictures")]
    public class ProductPicturesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductPicturesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProductPictures
        [HttpGet]
        public IEnumerable<ProductPicture> GetProductPicture()
        {
            return _context.ProductPicture;
        }

        // GET: api/ProductPictures/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductPicture([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ProductPicture productPicture = await _context.ProductPicture.SingleOrDefaultAsync(m => m.id == id);

            if (productPicture == null)
            {
                return NotFound();
            }

            return Ok(productPicture);
        }

        // PUT: api/ProductPictures/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductPicture([FromRoute] int id, [FromBody] ProductPicture productPicture)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productPicture.id)
            {
                return BadRequest();
            }

            _context.Entry(productPicture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductPictureExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductPictures
        [HttpPost]
        public async Task<IActionResult> PostProductPicture([FromBody] ProductPicture productPicture)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ProductPicture.Add(productPicture);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductPictureExists(productPicture.id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProductPicture", new { id = productPicture.id }, productPicture);
        }

        // DELETE: api/ProductPictures/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductPicture([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ProductPicture productPicture = await _context.ProductPicture.SingleOrDefaultAsync(m => m.id == id);
            if (productPicture == null)
            {
                return NotFound();
            }

            _context.ProductPicture.Remove(productPicture);
            await _context.SaveChangesAsync();

            return Ok(productPicture);
        }

        private bool ProductPictureExists(int id)
        {
            return _context.ProductPicture.Any(e => e.id == id);
        }
    }
}