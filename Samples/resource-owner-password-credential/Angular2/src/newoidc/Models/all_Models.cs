using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace newoidc.Models
{
    public class ShoppingCartViewModel
    {
        public List<tempCart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
    public class Product
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public Product()
        {
            this.AddDate = DateTime.Now;
        }

        public DateTime AddDate { get; private set; }

        public int cat { get; set; }

        public string location { get; set; }

        public string Country { get; set; }



        public string State { get; set; }

        public string City { get; set; }

        public int price { get; set; }

        public int active { get; set; }

        public int New { get; set; }

        public string dealCategories { get; set; }

        public bool SellAvailable { get; set; }

        public bool recycle { get; set; }

        public int views { get; set; }

        public string ApplicationUserId { get; set; }

        public string returnDeal { get; set; }
    }

    public class ProductPicture
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Something is just not right, Try Later!")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Please upload atleast one Picture")]
        public string pictureurl { get; set; }
    }
    public class ProductCategory
    {
        public int id { get; set; }
        public int productId { get; set; }
        public int CategoryId { get; set; }
    }
    public class OfferDetail
    {
        public int OfferDetailId { get; set; }
        public int OfferId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public virtual Product offerProduct { get; set; }
        public virtual offer offer { get; set; }
    }

    public class offer
    {
        public int OfferId { get; set; }
        public int views { get; set; }
        public int ProductId { get; set; }
        public int MessageId { get; set; }
        public int Extra { get; set; }
        public string extraString { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string ApplicationUserId { get; set; }
        public System.DateTime OrderDate { get; set; }
        public List<OfferDetail> OfferDetails { get; set; }
        public int status { get; set; }
        public offer()
        {
            OrderDate = DateTime.Now;
            status = 0;
            active = 0;
        }
        public int active { get; set; }
    }
    public class Notification
    {
        public int id { get; set; }
        public DateTime Adddate { get; set; }
        public Notification() { Adddate = DateTime.Now; }
        public string ApplicationUserId { get; set; }
        public string noteMessage { get; set; }
        public string noteUrl { get; set; }
        public bool read { get; set; }
        public string icon { get; set; }
    }

    public class Message
    {
        public int Messageid { get; set; }
        [Required(ErrorMessage = "Enter Message")]
        public string message { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string ApplicationUserId { get; set; }
        public Message() { this.AddDate = DateTime.Now; }
        public DateTime AddDate { get; private set; }
        public int parentId { get; set; }
        public virtual offer Offer { get; set; }
    }

    public class kartErrors
    {
        public int id { get; set; }
        public DateTime addDate { get; set; }
        public kartErrors() { addDate = System.DateTime.Now; }
        public string Error { get; set; }
        public string ErrorStack { get; set; }
        public string ErrorSource { get; set; }
    }

    public class invite
    {
        [Required, EmailAddress, Display(Name = "Send Invites to your friend one at a time")]
        public string email { get; set; }
    }

    public class tempCart
    {
        public int id { get; set; }
        public string CartId { get; set; }
        public int productId { get; set; }
        public int product { get; set; }
        public int count { get; set; }
        public int userId { get; set; }
        public int messageId { get; set; }
        public tempCart() { this.AddDate = DateTime.Now; }
        public DateTime AddDate { get; private set; }
        public virtual Product cartProduct { get; set; }
    }
    public class Category
    {
        [Display(Name = "Category Id")]
        public int id { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Enter Category")]
        public string CategoryTitle { get; set; }


        [Display(Name = "Parent Id")]
        public int? parentId { get; set; }


        public IList<Category> ChildMenu { get; set; }
    }

    public class ProductUpdate
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Enter Product name")]
        [StringLength(50)]
        [Display(Name = "Product Name")]
        [RegularExpression("^[a-zA-Z0-9\\(\\)\\-\\@\\#\\-\\s]+$", ErrorMessage = "Special characters allowed - You can use only ( ) @ # -")]
        public string ProductName { get; set; }


        [Required(ErrorMessage = "Enter Product Description")]
        [StringLength(500)]
        [RegularExpression("^[a-zA-Z0-9\\.\\,\\(\\[\\)\\]\\-\\@\\#\\-\\s]+$", ErrorMessage = "Special characters allowed - You can use only ( ) @ # - [ ] . ,")]
        [Display(Name = "Description")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Select Category")]
        [Display(Name = "Category")]
        public int cat { get; set; }

        [Required(ErrorMessage = "Location")]
        public string location { get; set; }

        [Required(ErrorMessage = "Select Country")]
        [StringLength(50)]
        public string Country { get; set; }


        [StringLength(50)]
        public string State { get; set; }


        [StringLength(50)]
        public string City { get; set; }

        public int price { get; set; }

        public int active { get; set; }

        public int New { get; set; }

        public string dealCategories { get; set; }

        public bool SellAvailable { get; set; }

        public bool recycle { get; set; }

        public int views { get; set; }

        public string ApplicationUserId { get; set; }

        public string returnDeal { get; set; }
    }

    public class proView
    {
        public string picturefirst { get; set; }
        public string catName { get; set; }

        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }


        public DateTime AddDate { get; set; }

        public int cat { get; set; }

        public string location { get; set; }

        public string Country { get; set; }



        public string State { get; set; }

        public string City { get; set; }

        public int price { get; set; }

        public int active { get; set; }

        public int New { get; set; }

        public string dealCategories { get; set; }

        public bool SellAvailable { get; set; }

        public bool recycle { get; set; }

        public int views { get; set; }

        public string ApplicationUserId { get; set; }

        public string returnDeal { get; set; }


        public IEnumerable<ProductPicture> ProductPictures { get; set; }
        public virtual List<Category> Categories { get; set; }
    }
}
