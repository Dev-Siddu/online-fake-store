using FakeStoreApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Entities;
using System.Text.Json;

namespace FakeStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FakeStoreController : ControllerBase
    {
        private readonly ILogger<FakeStoreController> _logger;
        private static List<Product>? Products = null;

        public FakeStoreController(ILogger<FakeStoreController> logger)
        {
            _logger = logger;
        }

        #region AllProducts
        [HttpGet]
        [Route("AllProducts")]
        public async Task<List<Product?>> AllProducts()
        {
            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            return Products;
        }
        #endregion

        #region GetProductById
        [HttpGet]
        [Route("GetProductByID")]
        public async Task<Product?> GetProductByID(int id)
        {
            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            Product? prod = Products?.FirstOrDefault(temp => temp.ID == id);
            if (prod == null) return null;
            return prod;
        }
        #endregion

        #region SearchProducts
        [HttpGet]
        [Route("SearchProducts")]
        public async Task<List<Product>?> SearchProducts(string search)
        {
            List<Product>? matchedProducts = new List<Product>();

            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            // TO : DO search Products
            matchedProducts = Products.Where(temp => temp.Name.Contains($"{search}", StringComparison.OrdinalIgnoreCase)).ToList();
            if (matchedProducts.Count <= 0) return null;
            return matchedProducts;
        }
        #endregion

        #region AddProduct
        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct(AddProductDTO addprod)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            Console.WriteLine(addprod.Name);
            Console.WriteLine(addprod.Price);
            Console.WriteLine(addprod.Description);
            //if (!ModelState.IsValid) return BadRequest();
            // Save the image
            string extension = Path.GetExtension(addprod.ImageFile.FileName);
            string prodImagePath = addprod.Name.Trim().Replace(" ", "") + Guid.NewGuid().ToString() + extension;
            string path = "wwwroot/Product_Images/" + prodImagePath;
            var stream = System.IO.File.Create(path);
            await addprod.ImageFile.CopyToAsync(stream);
            stream.Close();


            // Getting the all the products
            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            //List<Product> products = await getDSeriProductsAsync();

            long lastId = Products.Max(temp => temp.ID);    // last used id

            // Product to add to list
            Product prod = addprod.toProduct(lastId + 1, prodImagePath);

            //Product prod = new Product();
            //prod.ID = lastId + 1;
            //prod.Name = addprod.Name;
            //prod.Price = addprod.Price;
            //prod.Description = addprod.Description;
            //prod.ImagePath = "http://localhost:5035/" + "Product_Images/" + prodImagePath;

            Products.Add(prod);
            bool result = await SerilizaAndSaveProdsAsync(Products);
            if (result) return Ok();
            return StatusCode(500);
        }
        #endregion

        #region RemoveProduct
        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> RemoveProduct(RemoveProductDTO prod)
        {
            //List<Product> products = await getDSeriProductsAsync();
            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            int count = Products.RemoveAll(temp =>
            {
                if (temp.ID == prod.ID && temp.Name == prod.Name && temp.Price == prod.Price && temp.ImagePath == prod.ImagePath)
                {
                    return true;
                };
                return false;
            });
            if (!(count > 0))
            {
                return NotFound();
            }
            //Remove the image
            string path = prod.ImagePath.Replace("http://localhost:5035", "wwwroot");
            System.IO.File.Delete(path);
            bool isSaved = await SerilizaAndSaveProdsAsync(Products);
            if (isSaved) return Ok();
            else return StatusCode(500);
        }
        #endregion

        #region Purchase_Product
        [HttpGet]
        [Route("[Action]")]
        public async Task<Tuple<bool, string>> PurchaseProduct(int userID, int prodID)
        {
            User? user = await UsersHelper.GetUserById(userID);
            if (user == null)
            {
                return new Tuple<bool, string>(false, "Invalid user");
            }

            if (!await isProductExistsWithThisID(prodID))
            {
                return new Tuple<bool, string>(false, "Invalid product");
            }

            string path = "DataStore/purchased.json";
            string allPurchaseDetails = await System.IO.File.ReadAllTextAsync(path);
            List<Purchase>? purchaseDetails = JsonSerializer.Deserialize<List<Purchase>>(allPurchaseDetails);

            Purchase? userPurchase = purchaseDetails?.FirstOrDefault(temp => temp.UserID == userID);
            if (userPurchase == null)
            {
                Purchase FirstPurchase = new Purchase();
                FirstPurchase.UserID = userID;
                FirstPurchase.ProdIDs = new List<int> { prodID };

                purchaseDetails.Add(FirstPurchase);

            }
            else
            {
                userPurchase.ProdIDs.Add(prodID);
            }
            string serilizedPurchaseDetails = JsonSerializer.Serialize(purchaseDetails);
            await System.IO.File.WriteAllTextAsync(path, serilizedPurchaseDetails);
            return new Tuple<bool, string>(true, "Product purchase successfully");
        }
        #endregion

        #region AllMyPurchase
        [HttpGet]
        [Route("[Action]")]
        public async Task<List<Product>?> AllMyPurchase(int userID)
        {
            User? user = await UsersHelper.GetUserById(userID);
            if (user == null) return null;

            string path = "DataStore/purchased.json";
            string purchaseRegistry = await System.IO.File.ReadAllTextAsync(path);
            List<Purchase> purchaseList = JsonSerializer.Deserialize<List<Purchase>>(purchaseRegistry);
            Purchase? userPurchase = purchaseList.FirstOrDefault(temp => temp.UserID == userID);
            if (userPurchase == null) { return null; }

            List<int>? prodIds = userPurchase.ProdIDs;
            List<Product> purchasedProducts = new List<Product>();
            foreach (int id in prodIds)
            {
                Product? prd = await GetProductByID(id);
                if (prd != null) purchasedProducts.Add(prd);
            }
            return purchasedProducts;
        }
        #endregion

        #region Cart

        #region Add to Cart
        [HttpGet]
        [Route("[Action]")]
        public async Task<Tuple<bool, string>?> AddToCart(int userID, int prodID)
        {
            if (!ModelState.IsValid) return new Tuple<bool, string>(false, "Invalid details");
            User? user = await UsersHelper.GetUserById(userID);
            if (user == null)
            {
                return new Tuple<bool, string>(false, "Invalid user");
            }

            if (!await isProductExistsWithThisID(prodID))
            {
                return new Tuple<bool, string>(false, "Invalid product");
            }

            string path = "DataStore/CartInformation.json";
            string serilzedCartInformation = await System.IO.File.ReadAllTextAsync(path);
            List<Cart?> deSerilzedcartInformation = System.Text.Json.JsonSerializer.Deserialize<List<Cart>>(serilzedCartInformation);

            Cart? userCart = deSerilzedcartInformation?.FirstOrDefault(temp => temp.UserID == userID);
            if (userCart == null)
            {
                Cart cart = new Cart()
                {
                    UserID = userID,
                    ProdIDs = new List<int>()
                    {
                        prodID
                    }
                };
                deSerilzedcartInformation?.Add(cart);
            }
            else
            {
                userCart.ProdIDs.Insert(0,prodID);
            }
            string serilizeCartInformation = System.Text.Json.JsonSerializer.Serialize(deSerilzedcartInformation);
            await System.IO.File.WriteAllTextAsync(path, serilizeCartInformation);
            return new Tuple<bool, string>(true, "Product added to yout cart.");
        }
        #endregion

        #region Remove from Cart
        [HttpGet]
        [Route("[Action]")]
        public async Task<Tuple<bool, string>> RemoveFromCart(int userID, int prodID)
        {
            if (!ModelState.IsValid) return new Tuple<bool, string>(false, "Invalid details");
            User? user = await UsersHelper.GetUserById(userID);
            if (user == null)
            {
                return new Tuple<bool, string>(false, "Invalid user");
            }

            if (!await isProductExistsWithThisID(prodID))
            {
                return new Tuple<bool, string>(false, "Invalid product");
            }

            string path = "DataStore/CartInformation.json";
            string serilzedCartInformation = await System.IO.File.ReadAllTextAsync(path);
            List<Cart>? deSerilzedcartInformation = System.Text.Json.JsonSerializer.Deserialize<List<Cart>>(serilzedCartInformation);
            Cart? userCart = deSerilzedcartInformation?.FirstOrDefault(temp => temp.UserID == userID);
            int prodIndex = userCart.ProdIDs.IndexOf(prodID); // getting the prodId index
            if (prodIndex == -1) return new Tuple<bool, string>(false, "invalid product id");
            userCart.ProdIDs.RemoveAt(prodIndex); // removing the prodId usign it's index
            string serilizeCartInformation = System.Text.Json.JsonSerializer.Serialize(deSerilzedcartInformation);
            await System.IO.File.WriteAllTextAsync(path, serilizeCartInformation);
            return new Tuple<bool, string>(true, "Product Removed from yout cart.");

        }
        #endregion

        #region  MyCart
        [HttpGet]
        [Route("[Action]")]
        public async Task<List<Product>?> MyCart(int userID){
            User? user = await UsersHelper.GetUserById(userID);
            if (user == null)
            {
                return null;
            }
            //List of products that user added to this cart
            List<Product> userCartProducts = new List<Product>();

            string path = "DataStore/CartInformation.json";
            string serilzedCartInformation = await System.IO.File.ReadAllTextAsync(path);
            List<Cart?> deSerilzedcartInformation = System.Text.Json.JsonSerializer.Deserialize<List<Cart>>(serilzedCartInformation);
            Cart userCart = deSerilzedcartInformation.FirstOrDefault(temp => temp.UserID == userID);

            if (userCart == null) return null;

            foreach (int prodID in userCart.ProdIDs)
            {
                Product? prod = await GetProductByID(prodID);
                if (prod != null) userCartProducts.Add(prod);
            }
            return userCartProducts;
        }
        
        #endregion

        #endregion
        // ---------------------------------------------------------------------
        #region Non Action Methods

        #region getDSeriProductsAsync
        [NonAction]
        public async Task<List<Product>?> getDSeriProductsAsync()
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string json = await System.IO.File.ReadAllTextAsync(productsPath);
            List<Product>? prods = JsonSerializer.Deserialize<List<Product>>(json);
            return prods;
        }
        #endregion

        #region SerilizaAndSaveProdsAsync
        [NonAction]
        public async Task<bool> SerilizaAndSaveProdsAsync(List<Product> prods)
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string serilizedProds = JsonSerializer.Serialize(prods);
            await System.IO.File.WriteAllTextAsync(productsPath, serilizedProds);
            return true;
        }
        #endregion

        #region loadProducts
        [NonAction]
        public async Task<List<Product>?> loadProducts()
        {
            List<Product>? prods = await getDSeriProductsAsync();
            return prods;
        }
        #endregion

        #region IsProductExistsWithThisID
        [NonAction]
        public async Task<bool> isProductExistsWithThisID(int prodID)
        {
            List<Product>? prods = await loadProducts();
            if (prods == null) return false;
            List<Product>? matchedProds = prods.Where(temp => temp.ID == prodID).ToList();
            if (matchedProds.Count > 1 || matchedProds.Count == 0 || matchedProds == null)
            {
                return false;
            }
            return true;
        }
        #endregion

        #endregion
    }
}