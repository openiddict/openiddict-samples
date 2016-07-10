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
    [Route("api/offers")]
    public class offersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public offersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/offers
        [HttpGet]
        public IEnumerable<offer> Getoffer()
        {
            return _context.offer;
        }

        // GET: api/offers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Getoffer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            offer offer = await _context.offer.SingleOrDefaultAsync(m => m.OfferId == id);

            if (offer == null)
            {
                return NotFound();
            }

            return Ok(offer);
        }

        // PUT: api/offers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Putoffer([FromRoute] int id, [FromBody] offer offer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != offer.OfferId)
            {
                return BadRequest();
            }

            _context.Entry(offer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!offerExists(id))
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

        // POST: api/offers
        [HttpPost]
        public async Task<IActionResult> Postoffer([FromBody] offer offer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.offer.Add(offer);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (offerExists(offer.OfferId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("Getoffer", new { id = offer.OfferId }, offer);
        }

        // DELETE: api/offers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteoffer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            offer offer = await _context.offer.SingleOrDefaultAsync(m => m.OfferId == id);
            if (offer == null)
            {
                return NotFound();
            }

            _context.offer.Remove(offer);
            await _context.SaveChangesAsync();

            return Ok(offer);
        }

        private bool offerExists(int id)
        {
            return _context.offer.Any(e => e.OfferId == id);
        }
    }
}