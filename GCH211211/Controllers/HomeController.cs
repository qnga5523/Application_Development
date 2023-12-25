using GCH211211.Data;
using GCH211211.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace GCH211211.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            this._context = context;
            this._userManager = userManager;
        }

        public const string CARTKEY = "cart";
        List<Cart> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsoncart = session.GetString(CARTKEY);
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<Cart>>(jsoncart);
            }
            return new List<Cart>();
        }

        // remove cart 
        void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove(CARTKEY);
        }

        // Lưu Cart (Danh sách CartItem) vào session
        void SaveCartSession(List<Cart> ls)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(ls));

            var session = HttpContext.Session;
            string jsoncart = JsonConvert.SerializeObject(ls);
            session.SetString(CARTKEY, jsoncart);
        }
        void SaveCartSession(List<Cart> ls, decimal total)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(ls));
            HttpContext.Session.SetString("Total", total.ToString("n0"));
        }
        [Route("/")]
        public IActionResult Index()
        {
            var products = _context.Product.ToList();
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View(products);
        }
        public IActionResult Index2(int? id)
        {
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;

            // Retrieve the specific category along with its associated products
            var productSearch = _context.Category
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (productSearch == null)
            {
                // Category not found, you may want to handle this case (e.g., show an error message)
                return NotFound();
            }

            return View(productSearch);
        }
        [HttpPost]
        public IActionResult Search(string search)
        {
            var products = _context.Product.Where(p => p.Name.Contains(search)).ToList();
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            TempData["search"] = search;
            return View("Index", products);
        }
        public IActionResult Details(int? id)
        {
            var products = _context.Product.ToList();
            var item = products.FirstOrDefault(c => c.Id == id);
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View(item);
        }
        public Product GetDetailProduct(int? id)
        {
            var products = _context.Product.Find(id);
            return products;
        }
            public IActionResult AddtoCart(int? id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart == null)
            {
                var product = GetDetailProduct(id);
                List<Cart> listCart = new List<Cart>()
                {
                    new Cart
                    {
                        product = product,
                        Quantity = 1
                    }
                };
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(listCart));
            }
            else
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                bool check = true;
                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].product.Id == id)
                    {
                        dataCart[i].Quantity++;
                        check = false;
                    }
                }
                if (check)
                {
                    dataCart.Add(new Cart
                    {
                        product = GetDetailProduct(id),
                        Quantity = 1
                    });
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
            }
            return RedirectToAction("Index");
        }

        public IActionResult ViewCart()
        {
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;
            return View(GetCartItems());
        }

        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                if (quantity > 0)
                {
                    for (int i = 0; i < dataCart.Count; i++)
                    {
                        if (dataCart[i].product.Id == id)
                        {
                            dataCart[i].Quantity = quantity;
                        }
                    }
                    HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                }
                return Ok(quantity);
            }
            return BadRequest();
        }
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);

                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].product.Id == id)
                    {
                        dataCart.RemoveAt(i);
                    }
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));

                return RedirectToAction(nameof(ViewCart));
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Checkout()
        {
            try
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                string currentUserId = claims.Value;

                // calculate total price
                var cart = GetCartItems();
                decimal total = cart.Sum(item => item.Quantity * item.product.Price);

                foreach (var item in cart)
                {
                    var order = new Order()
                    {
                        UserId = currentUserId,
                        ProductId = item.product.Id,
                        ProductName = item.product.Name,
                        Image = item.product.Image,
                        Price = item.product.Price,
                        Quantity = item.Quantity,
                        TotalPrice = total,
                    };

                    _context.Order.Add(order);
                }

                _context.SaveChanges();

                ClearCart();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details or handle it appropriately
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // Optionally, redirect to an error page or show an error message to the user
                return RedirectToAction("Error");
            }
        }
        public IActionResult ViewOrder()
        {
            var categories = _context.Category.ToList();
            ViewBag.Categories = categories;

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string currentUserId = claims.Value;

            // Remove the following lines, as currentUserId is already a string
            // int UsrID = Int32.Parse(currentUserId);

            // Query orders for the current user
            var orders = _context.Order.Where(p => p.UserId == currentUserId).ToList();

            return View("ViewOrder", orders);
        }



            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
