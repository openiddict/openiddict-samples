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
    [Route("api/Products")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public List<proView> GetProduct()
        {
            proView product = new proView();
            var pictures = _context.ProductPicture;
            List<Product> pros = new List<Product>();
            var model = _context.Product.Where(a => a.active == 1).Select(p => new proView
            {
                Id = p.Id,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                ProductPictures = pictures.Where(x => x.ProductId == p.Id),
                picturefirst = pictures.FirstOrDefault(x => x.ProductId == p.Id).pictureurl,
                //cat = p.cat.ToString(),
                AddDate = p.AddDate,
                ApplicationUserId = p.ApplicationUserId,
                New = p.New,
                location = p.location,
                Country = p.Country,
                City = p.City,
                State = p.State,
                views = p.views,
                price = p.price,
                catName = _context.Category.FirstOrDefault(x => x.id == p.cat).CategoryTitle,
                Categories = _context.Category.Where(x =>
                   p.dealCategories.Contains(x.id.ToString())
                           ).ToList()
            }).OrderByDescending(p => p.AddDate).ToList();
            return model;
            //return _context.Product;
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id == 0)
            {
                Product y = new Product();
                _context.Product.Add(y);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return new StatusCodeResult(StatusCodes.Status409Conflict);
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok(y);
            }
            else
            {
                Product product = await _context.Product.SingleOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }

           
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] ProductUpdate product)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return BadRequest();
            }
            Product y = new Product();
            y.Id = product.Id;
            y.ProductName = product.ProductName;
            y.ProductDescription = product.ProductDescription;
            y.cat = product.cat;
            y.location = product.location;
            y.Country = product.Country;
            y.State = product.State;
            y.City = product.City;
            y.active = product.active;
            y.active = product.active;
            y.ApplicationUserId = product.ApplicationUserId;
            y.views = product.views;
            y.price = product.price;
            y.SellAvailable = product.SellAvailable;
            y.returnDeal = product.returnDeal;
            y.dealCategories = product.dealCategories;
            y.New = product.New;
            //y.d = product.returnDeal;
            _context.Entry(y).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(y);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

          
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] Product product)
        {
            Product y = new Product();
            _context.Product.Add(y);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Product product = await _context.Product.SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

        public List<proView> ProductList()
        {
            proView product = new proView();
            var pictures = _context.ProductPicture;
            List<Product> pros = new List<Product>();

            var model = _context.Product.Where(a => a.active == 1).Select(p => new proView
            {
                Id = p.Id,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                ProductPictures = pictures.Where(x => x.ProductId == p.Id),
                picturefirst = pictures.FirstOrDefault(x => x.ProductId == p.Id).pictureurl,
                //cat = p.cat.ToString(),
                AddDate = p.AddDate,
                ApplicationUserId = p.ApplicationUserId,
                New = p.New,
                location = p.location,
                Country = p.Country,
                City = p.City,
                State = p.State,
                views = p.views,
                price = p.price,
                catName = _context.Category.FirstOrDefault(x => x.id == p.cat).CategoryTitle,
                Categories = _context.Category.Where(x =>
                   p.dealCategories.Contains(x.id.ToString())
                ).ToList()
            }).OrderByDescending(p => p.AddDate).ToList();
            return model;
        }
    }
}