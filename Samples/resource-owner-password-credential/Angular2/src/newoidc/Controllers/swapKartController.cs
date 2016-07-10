using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using newoidc.Models;
using newoidc.Data;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace newoidc.Controllers
{
    public class swapKartController : Controller
    {
        private readonly ILogger<swapKartController> _logger;
        public ApplicationDbContext DbContext { get; }
        public swapKartController(ApplicationDbContext dbContext, ILogger<swapKartController> logger)
        {
            DbContext = dbContext;
            _logger = logger;
        }
        public async Task<ShoppingCartViewModel> Index()
        {
            var cart = swapKart.GetCart(DbContext, HttpContext);

            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = await cart.GetCartItems(),
                CartTotal = await cart.GetTotal()
            };

            // Return the view
            return viewModel;
        }

        public async Task<IActionResult> AddToCart(int id, CancellationToken requestAborted)
        {
            // Retrieve the album from the database
            var addedAlbum = await DbContext.Product
                .SingleAsync(album => album.Id == id);

            // Add it to the shopping cart
            var cart = swapKart.GetCart(DbContext, HttpContext);

            await cart.AddToCart(addedAlbum);

            await DbContext.SaveChangesAsync(requestAborted);
            _logger.LogInformation("Album {albumId} was added to the cart.", addedAlbum.Id);

            // Go back to the main store page for more shopping
            return RedirectToAction("Index");
        }

        // AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(
            int id,
            CancellationToken requestAborted)
        {
            // Retrieve the current user's shopping cart
            var cart = swapKart.GetCart(DbContext, HttpContext);

            // Get the name of the album to display confirmation
            var cartItem = await DbContext.tempCarts
                .Where(item => item.productId == id)
                .Include(c => c.product)
                .SingleOrDefaultAsync();

            string message;
            int itemCount;
            if (cartItem != null)
            {
                // Remove from cart
                itemCount = cart.RemoveFromCart(id);

                await DbContext.SaveChangesAsync(requestAborted);

                string removed = (itemCount > 0) ? " 1 copy of " : string.Empty;
                message = removed + cartItem.cartProduct.ProductName + " has been removed from your shopping cart.";
            }
            else
            {
                itemCount = 0;
                message = "Could not find this item, nothing has been removed from your shopping cart.";
            }

            // Display the confirmation message
            /*
            var results = new ShoppingCartRemoveViewModel
            {
                Message = message,
                CartTotal = await cart.GetTotal(),
                CartCount = await cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };

            _logger.LogInformation("Album {id} was removed from a cart.", id);
            */
            return Json(message="s");
        }
    }
}