using Microsoft.AspNetCore.Mvc;
using Models.Entities;
using Models.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FakeStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FakeStoreController : ControllerBase
    {
        private readonly ILogger<FakeStoreController> _logger;
        public FakeStoreController(ILogger<FakeStoreController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Products")]
        public async Task<List<Product?>> Products()
        {
            List<Product>? products = await getDSeriProducts();
            return products;
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
            string FileName = addprod.Name + Guid.NewGuid().ToString() + extension;
            string path = "wwwroot/Product_Images/" + FileName ;
            var stream = System.IO.File.Create(path);
            await addprod.ImageFile.CopyToAsync(stream);
            stream.Close();
            

            // Getting the all the products
            List<Product> products = await getDSeriProducts();

            long lastId = products.Max(temp => temp.ID);    // last used id

            // Product to add to list
            Product prod = new Product();
            prod.ID = lastId + 1;
            prod.Name = addprod.Name;
            prod.Price = addprod.Price;
            prod.Description = addprod.Description;
            prod.ImagePath = "http://localhost:5035/" + "Product_Images/" + FileName;

            products.Add(prod);
            bool result = await SerilizaAndSaveProds(products);
            if (result) return Ok();
            return StatusCode(500);
        }

        [NonAction]
        public async Task<List<Product>?> getDSeriProducts()
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string json = await System.IO.File.ReadAllTextAsync(productsPath);
            List<Product>? prods = JsonSerializer.Deserialize<List<Product>>(json);
            return prods;
        }
        [NonAction]
        public async Task<bool> SerilizaAndSaveProds(List<Product> prods)
        {
            string productsPath = "DataStore/ProductsInformation.json";
            string serilizedProds = JsonSerializer.Serialize(prods);
            await System.IO.File.WriteAllTextAsync(productsPath,serilizedProds);
            return true;
        }
    }
}