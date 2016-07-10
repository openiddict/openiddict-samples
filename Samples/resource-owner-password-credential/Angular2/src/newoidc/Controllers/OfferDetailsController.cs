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
    [Route("api/OfferDetails")]
    public class OfferDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfferDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OfferDetails
        [HttpGet]
        public IEnumerable<OfferDetail> GetOfferDetail()
        {
            return _context.OfferDetail;
        }

        // GET: api/OfferDetails/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOfferDetail([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            OfferDetail offerDetail = await _context.OfferDetail.SingleOrDefaultAsync(m => m.OfferDetailId == id);

            if (offerDetail == null)
            {
                return NotFound();
            }

            return Ok(offerDetail);
        }

        // PUT: api/OfferDetails/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOfferDetail([FromRoute] int id, [FromBody] OfferDetail offerDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != offerDetail.OfferDetailId)
            {
                return BadRequest();
            }

            _context.Entry(offerDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfferDetailExists(id))
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

        // POST: api/OfferDetails
        [HttpPost]
        public async Task<IActionResult> PostOfferDetail([FromBody] OfferDetail offerDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.OfferDetail.Add(offerDetail);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OfferDetailExists(offerDetail.OfferDetailId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetOfferDetail", new { id = offerDetail.OfferDetailId }, offerDetail);
        }

        // DELETE: api/OfferDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOfferDetail([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            OfferDetail offerDetail = await _context.OfferDetail.SingleOrDefaultAsync(m => m.OfferDetailId == id);
            if (offerDetail == null)
            {
                return NotFound();
            }

            _context.OfferDetail.Remove(offerDetail);
            await _context.SaveChangesAsync();

            return Ok(offerDetail);
        }

        private bool OfferDetailExists(int id)
        {
            return _context.OfferDetail.Any(e => e.OfferDetailId == id);
        }
    }
}