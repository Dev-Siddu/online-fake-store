using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.DTO;
using System.Text.Json;
using System.Collections.Specialized;

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
        [HttpGet]
        [Route("SearchProducts")]
        public async Task<List<Product>?> SearchProducts(string search)
        {
            if (Products == null)
            {
                Products = await getDSeriProductsAsync();
            }
            List<Product>? prods = Products?.Where(temp => temp.Name.Contains(search,StringComparison.OrdinalIgnoreCase)).ToList();
            if (prods == null) return null;
            return prods;
        }

        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct(AddProductDTO addprod)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);  
            Console.WriteLine(addprod.Name);
            Console.WriteLine(addprod.Price);
            Console.WriteLine(addprod.Description);
            //if (!ModelState.IsValid) return BadRequest();
            // Save the image
            string extension = Path.GetExtension(addprod.ImageFile.FileName);
            string prodImagePath = addprod.Name.Trim().Replace(" ","") + Guid.NewGuid().ToString() + extension;
            string path = "wwwroot/Product_Images/" + prodImagePath ;
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
            if(!(count > 0))
            {
                return NotFound();
            }
            //Remove the image
            string path = prod.ImagePath.Replace("http://localhost:5035","wwwroot");
            System.IO.File.Delete(path);
            bool isSaved = await SerilizaAndSaveProdsAsync(Products);
            if (isSaved) return Ok();
            else return StatusCode(500);
        }

        [NonAction]
        public async Task<List<Product>?> getDSeriProductsAsync()
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string json = await System.IO.File.ReadAllTextAsync(productsPath);
            List<Product>? prods = JsonSerializer.Deserialize<List<Product>>(json);
            return prods;
        }
        [NonAction]
        public async Task<bool> SerilizaAndSaveProdsAsync(List<Product> prods)
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string serilizedProds = JsonSerializer.Serialize(prods);
            await System.IO.File.WriteAllTextAsync(productsPath,serilizedProds);
            return true;
        }

        [NonAction]
        public async Task<List<Product>?> loadProducts()
        {
            List<Product>? prods = await getDSeriProductsAsync();
            return Products;
        }
    }
}