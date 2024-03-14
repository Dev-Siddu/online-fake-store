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
        public FakeStoreController(ILogger<FakeStoreController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Products")]
        public async Task<List<Product?>> Products()
        {
            List<Product>? products = await getDSeriProductsAsync();
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
            List<Product> products = await getDSeriProductsAsync();

            long lastId = products.Max(temp => temp.ID);    // last used id

            // Product to add to list
            Product prod = new Product();
            prod.ID = lastId + 1;
            prod.Name = addprod.Name;
            prod.Price = addprod.Price;
            prod.Description = addprod.Description;
            prod.ImagePath = "http://localhost:5035/" + "Product_Images/" + FileName;

            products.Add(prod);
            bool result = await SerilizaAndSaveProdsAsync(products);
            if (result) return Ok();
            return StatusCode(500);
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> RemoveProduct(RemoveProductDTO prod)
        {
            List<Product> products = await getDSeriProductsAsync();
            int count = products.RemoveAll(temp =>
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
            bool isSaved = await SerilizaAndSaveProdsAsync(products);
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
    }
}