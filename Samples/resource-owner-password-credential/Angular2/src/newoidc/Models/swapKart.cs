using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using newoidc.Models;
using newoidc.Data;
using Microsoft.EntityFrameworkCore;

namespace newoidc.Models
{
    public class swapKart
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _swapCartId;

        private swapKart(ApplicationDbContext dbContext, string id)
        {
            _dbContext = dbContext;
            _swapCartId = id;
        }

        public static swapKart GetCart(ApplicationDbContext db, HttpContext context)
            => GetCart(db, GetCartId(context));

        public static swapKart GetCart(ApplicationDbContext db, string cartId)
            => new swapKart(db, cartId);

        public async Task AddToCart(Product product)
        {
            // Get the matching cart and album instances
            var cartItem = await _dbContext.tempCarts.SingleOrDefaultAsync(
                c => c.CartId == _swapCartId
                && c.productId == product.Id);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new tempCart
                {
                    id = product.Id,
                    CartId = _swapCartId,
                    count = 1
                };

                _dbContext.tempCarts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, then add one to the quantity
                cartItem.count++;
            }
        }

        public int RemoveFromCart(int id)
        {
            // Get the cart
            var cartItem = _dbContext.tempCarts.SingleOrDefault(
                cart => cart.CartId == _swapCartId
                && cart.id == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.count > 1)
                {
                    cartItem.count--;
                    itemCount = cartItem.count;
                }
                else
                {
                    _dbContext.tempCarts.Remove(cartItem);
                }
            }

            return itemCount;
        }

        public async Task EmptyCart()
        {
            var cartItems = await _dbContext
                .tempCarts
                .Where(cart => cart.CartId == _swapCartId)
                .ToArrayAsync();

            _dbContext.tempCarts.RemoveRange(cartItems);
        }

        public Task<List<tempCart>> GetCartItems()
        {
            return _dbContext
                .tempCarts
                .Where(cart => cart.CartId == _swapCartId)
                .Include(c => c.product)
                .ToListAsync();
        }

        public Task<List<string>> GetCartAlbumTitles()
        {
            return _dbContext
                .tempCarts
                .Where(cart => cart.CartId == _swapCartId)
                .Select(c => c.cartProduct.ProductName)
                .OrderBy(n => n)
                .ToListAsync();
        }

        public Task<int> GetCount()
        {
            // Get the count of each item in the cart and sum them up
            return _dbContext
                .tempCarts
                .Where(c => c.CartId == _swapCartId)
                .Select(c => c.count)
                .SumAsync();
        }

        public Task<int> GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total

            return _dbContext
                .tempCarts
                .Include(c => c.cartProduct)
                .Where(c => c.CartId == _swapCartId)
                .Select(c => c.cartProduct.price * c.count)
                .SumAsync();
        }

        public async Task<int> CreateOrder(offer offer)
        {
            decimal orderTotal = 0;

            var cartItems = await GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                //var album = _db.Albums.Find(item.AlbumId);
                var product = await _dbContext.Product.SingleAsync(a => a.Id == item.id);

                var orderDetail = new OfferDetail
                {
                    ProductId = item.id,
                    OfferId = offer.OfferId,
                    UnitPrice = product.price,
                    Quantity = item.count,
                };

                // Set the order total of the shopping cart
                orderTotal += (item.count * product.price);

                _dbContext.OfferDetail.Add(orderDetail);
            }

            // Set the order's total to the orderTotal count
           // offer.extraString = orderTotal;

            // Empty the shopping cart
            await EmptyCart();

            // Return the OrderId as the confirmation number
            return offer.OfferId;
        }

        // We're using HttpContextBase to allow access to sessions.
        private static string GetCartId(HttpContext context)
        {
            var cartId = context.Session.GetString("Session");

            if (cartId == null)
            {
                //A GUID to hold the cartId. 
                cartId = Guid.NewGuid().ToString();

                // Send cart Id as a cookie to the client.
                context.Session.SetString("Session", cartId);
            }

            return cartId;
        }
    }
}
