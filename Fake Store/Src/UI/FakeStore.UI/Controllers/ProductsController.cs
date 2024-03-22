using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Entities;
using Newtonsoft.Json;

namespace FakeStore.UI.Controllers
{
    [Controller]
    [Route("[Controller]")]
    public class ProductsController : Controller
    {
        private readonly ILogger<Product> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public ProductsController(ILogger<Product> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> AllProducts()
        {
            HttpClient client = _httpClientFactory.CreateClient(); // HttpClient created.
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5035/api/FakeStore/AllProducts")
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            //var products = JsonSerializer.Deserialize<List<Product>>(content);
            var products = JsonConvert.DeserializeObject<List<Product>>(content);

            return View(products);
        }

        // For Adding new product
        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = "Seller")]
        public IActionResult AddNewProd()
        {
            return View(new AddProductDTO());
        }

        [HttpPost]
        [Route("[Action]")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddNewProd(AddProductDTO addProd)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            HttpClient client = _httpClientFactory.CreateClient(); // HttpClient Created

            // Formatting the content of model
            //addProd.ImageFile = addProd.ImageFile.F.Trim().Replace(" ","");

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(addProd.Name, System.Text.Encoding.UTF8), "Name");
            formData.Add(new StringContent(addProd.Description.Trim(), System.Text.Encoding.UTF8), "Description");
            formData.Add(new StringContent(addProd.Price.ToString().Trim(), System.Text.Encoding.UTF8), "Price");
            formData.Add(new StreamContent(addProd.ImageFile.OpenReadStream()), "ImageFile", addProd.ImageFile.FileName.Trim().Replace(" ",""));
            HttpResponseMessage response = await client.PostAsync("http://localhost:5035/api/FakeStore/AddProduct", formData);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Response = 0;
                return View(new AddProductDTO());
            };
            ViewBag.Response = 1;
            return View(new AddProductDTO());
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveProd()
        {
            HttpClient client = _httpClientFactory.CreateClient(); // HttpClient created.
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5035/api/FakeStore/AllProducts")
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            //var products = JsonSerializer.Deserialize<List<Product>>(content);
            var products = JsonConvert.DeserializeObject<List<Product>>(content);

            return View(products);
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemovingProductCompleteDetails(RemoveProductDTO prod)
        {
            List<Product> prods = await GetProductsAsync();
            Product? gotProd = prods.FirstOrDefault(temp =>
            {
                if(temp.ID == prod.ID && temp.Name == prod.Name && temp.Price == prod.Price && temp.ImagePath == prod.ImagePath)
                {
                    return true;
                }
                return false;
            });
            if (gotProd == null) return NotFound();

            return View(gotProd);
        }

        [HttpPost]
        [Route("[Action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinallyRemoveProduct(Product prod)
        {
            RemoveProductDTO rProd = new RemoveProductDTO()
            {
                ID = prod.ID,
                Name = prod.Name,
                Price = prod.Price,
                ImagePath = prod.ImagePath
            };

            string jsonContent = JsonConvert.SerializeObject(rProd);
            HttpContent content = new StringContent(jsonContent,System.Text.Encoding.UTF8,"application/json");

            HttpClient client = _httpClientFactory.CreateClient();

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("http://localhost:5035/api/FakeStore/RemoveProduct"),
                Content = content
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                return LocalRedirect("~/Products/AllProducts");
            }
            return Content("<h3>Couldn't remove the product. Some internal server error occured. Contact admin</h3>", "text/html");
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> BuyNowDetails(int ProdID)
        {
            // To : Do
            Product? prd = await GetProductByIdAsync(ProdID);
            if (prd == null) return BadRequest("Product not found / available");
            // 
            return View(prd);
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> BuyNow(int id)
        {
            // Validation
            int userId;
            bool isIdAvailable = int.TryParse(HttpContext.User?.FindFirst("UserID").Value,out userId);
            if (!isIdAvailable) { return  BadRequest(); }

            Product? prod = await GetProductByIdAsync(id);
            if(prod == null) return NotFound();

            // Finally calling api for purchase
            string url = $"http://localhost:5035/api/FakeStore/PurchaseProduct?userID={userId}&prodID={prod.ID}";
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage() { 
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),  
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string responseString = await response.Content.ReadAsStringAsync();
            Tuple<bool,string>? responseTuple = JsonConvert.DeserializeObject<Tuple<bool,string> >(responseString);

            if (responseTuple == null) return StatusCode(500);
            if(responseTuple.Item1 == false) return BadRequest(responseTuple.Item2);
            return StatusCode(200,responseTuple.Item2);
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> AllMyPurchases()
        {
            int userId;
            bool isIdAvailable = int.TryParse(HttpContext.User?.FindFirst("UserID").Value, out userId);
            if (!isIdAvailable) { return BadRequest(); }

            string url = $"http://localhost:5035/api/FakeStore/AllMyPurchase?userID={userId}";
            
            // calling api
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url)
            };

            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            string response = await responseMessage.Content.ReadAsStringAsync();
            List<Product>? purchasedProducts = JsonConvert.DeserializeObject<List<Product>>(response);
            purchasedProducts.Reverse();
            return View(purchasedProducts);
        }


        // ----------------------------- Non Action ---------------------------------------
        [NonAction]
        private async Task<List<Product>?> GetProductsAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient(); // HttpClient created.
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://localhost:5035/api/FakeStore/AllProducts")
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            //var products = JsonSerializer.Deserialize<List<Product>>(content);
            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(content);
            return products;
        }

        [NonAction]
        public async Task<Product?> GetProductByIdAsync(int prodID)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://localhost:5035/api/FakeStore/GetProductByID?id={prodID}")
            };
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            //var products = JsonSerializer.Deserialize<List<Product>>(content);
            Product? products = JsonConvert.DeserializeObject<Product>(content);
            return products??null;
        }
    }
}
