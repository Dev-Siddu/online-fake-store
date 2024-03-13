using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Entities;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

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
                RequestUri = new Uri("http://localhost:5035/api/FakeStore/Products")
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
        public IActionResult AddNewProd()
        {
            return View(new AddProductDTO());
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> AddNewProd(AddProductDTO addProd)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            HttpClient client = _httpClientFactory.CreateClient(); // HttpClient Created

            // Formatting the content of model
            addProd.Name = addProd.Name.Trim().Replace(" ", "");
            addProd.Description = addProd.Description.Trim().Replace(" ","");

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(addProd.Name, System.Text.Encoding.UTF8), "Name");
            formData.Add(new StringContent(addProd.Description.Trim(), System.Text.Encoding.UTF8), "Description");
            formData.Add(new StringContent(addProd.Price.ToString().Trim(), System.Text.Encoding.UTF8), "Price");
            formData.Add(new StreamContent(addProd.ImageFile.OpenReadStream()), "ImageFile", addProd.ImageFile.FileName);
            HttpResponseMessage response = await client.PostAsync("http://localhost:5035/api/FakeStore/AddProduct", formData);
            if (!response.IsSuccessStatusCode) 
                {
                ViewBag.Response = 0;
                return View(new AddProductDTO());
            };
            ViewBag.Response = 1;
            return View(new AddProductDTO());
        }

        /*
        [NonAction]
        public async Task<MultipartFormDataContent> SerilizeModel(AddProductDTO prod)
        {
            var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(prod.Name, Encoding.UTF8, MediaTypeNames.Text.Plain),"Name");
            formData.Add(new StringContent(prod.Description, Encoding.UTF8, MediaTypeNames.Text.Plain), "Description");
            formData.Add(new StringContent(prod.Price.ToString(), Encoding.UTF8, MediaTypeNames.Text.Plain), "Price");

            var FileContent = new StreamContent(prod.ImageFile.OpenReadStream());
            FileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Image.Jpeg);

            formData.Add(FileContent,"product_picture",prod.ImageFile.FileName);
            return formData;
        }
        */
    }
}
