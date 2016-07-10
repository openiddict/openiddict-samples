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
    [Route("api/kartErrors")]
    public class kartErrorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public kartErrorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/kartErrors
        [HttpGet]
        public IEnumerable<kartErrors> GetkartErrors()
        {
            return _context.kartErrors;
        }

        // GET: api/kartErrors/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetkartErrors([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            kartErrors kartErrors = await _context.kartErrors.SingleOrDefaultAsync(m => m.id == id);

            if (kartErrors == null)
            {
                return NotFound();
            }

            return Ok(kartErrors);
        }

        // PUT: api/kartErrors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutkartErrors([FromRoute] int id, [FromBody] kartErrors kartErrors)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != kartErrors.id)
            {
                return BadRequest();
            }

            _context.Entry(kartErrors).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!kartErrorsExists(id))
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

        // POST: api/kartErrors
        [HttpPost]
        public async Task<IActionResult> PostkartErrors([FromBody] kartErrors kartErrors)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.kartErrors.Add(kartErrors);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (kartErrorsExists(kartErrors.id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetkartErrors", new { id = kartErrors.id }, kartErrors);
        }

        // DELETE: api/kartErrors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletekartErrors([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            kartErrors kartErrors = await _context.kartErrors.SingleOrDefaultAsync(m => m.id == id);
            if (kartErrors == null)
            {
                return NotFound();
            }

            _context.kartErrors.Remove(kartErrors);
            await _context.SaveChangesAsync();

            return Ok(kartErrors);
        }

        private bool kartErrorsExists(int id)
        {
            return _context.kartErrors.Any(e => e.id == id);
        }
    }
}