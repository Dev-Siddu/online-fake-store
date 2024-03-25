using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Entities;
using System.Text.Json;

namespace FakeStoreApi.Controllers
{
    [Route("api/[Controller]")]
    public class AuthController : Controller
    {
        [HttpGet]
        [Route("[Action]")]
        public async Task<List<User>> getAllUsers()
        {
            List<User>? users = await All();
            return users;
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<Tuple<int, int?,string, string, string>> VerifyUser([FromBody] LoginDTO loginCredentials)
        {
            //Tuple<int,int?, string, string, string> : isValidUser, Id, Role, Name, ImageName
            if (loginCredentials == null) return new Tuple<int,int?, string,string,string>(0,null, "","","");

            int? comparer = null;
            // 0 - phone (Numeric)
            // 1 - Email
            double num;
            if (loginCredentials.EmailOrPhone.Contains("@") && loginCredentials.EmailOrPhone.Contains("."))
            {
                comparer = 1;
            }
            else if (double.TryParse(loginCredentials.EmailOrPhone, out num))
            {
                comparer = 0;
            }
            else
            {
                return new Tuple<int,int?, string, string, string>(0, null,"", "", ""); // invalid email or phone
            }

            List<User>? users = await All();
            List<User>? validUsers = null;
            if (users == null) return new Tuple<int,int?, string, string, string>(0, null, "", "", "");

            if (comparer == 0)
            {
                validUsers = users.FindAll(user =>
                {
                    if (user.Phone.Equals(loginCredentials.EmailOrPhone) && user.Password.Equals(loginCredentials.Password))
                    {
                        return true;
                    }
                    return false;
                });
            }
            else if (comparer == 1)
            {
                validUsers = users.FindAll(user =>
                {
                    if (user.Email.Equals(loginCredentials.EmailOrPhone) && user.Password.Equals(loginCredentials.Password))
                    {
                        return true;
                    }
                    return false;
                });
            }

            if (validUsers.Count == 1)
            {
            return new Tuple<int, int?, string,string, string>(1, validUsers[0].Id, validUsers[0].Role, validUsers[0].Name, validUsers[0].ImageName.Split(",")[0]);
            }
            return new Tuple<int, int?,string, string, string>(0,null, "", "", "");
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> UserAdd([FromBody] User user)
        {
            int lastId = await getLastUserID();
            user.Id = lastId + 1;

            List<User> users = await All();
            users.Add(user);
            await serilizeAndSaveUsers(users);
            return Ok();
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<string> SaveUserProfileImage(IFormFile imageFile)
        {
            if (imageFile == null) return string.Empty;
            if (imageFile.Length <= 0) return string.Empty;

            string FullfileName = imageFile.FileName.Trim().Replace(" ", "");
            string fileName = FullfileName.Insert(imageFile.FileName.LastIndexOf("."), Guid.NewGuid().ToString());
            string path = "Assets/UserImages/" + fileName;

            FileStream fileStream = System.IO.File.Create(path);
            await imageFile.CopyToAsync(fileStream);
            fileStream.Close();
            return fileName;
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<Stream?> getUserProfileImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return null;
            string path = "Assets/UserImages/" + imageName;
            if (!System.IO.File.Exists(path)) return null;

            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(path);
            Stream imageStream = new MemoryStream(imageBytes);
            return imageStream;
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<bool> isMailIDExists(string mailID)
        {
            List<User>? users = await All();
            if (users == null) return false;
            bool isExists = users.Where(temp => temp.Email.Equals(mailID)).Any();
            return isExists;
        }

        [NonAction]
        public async Task<List<User>?> All()
        {
            string path = "DataStore/UserInformation.json";
            string serilizedUserInformation = await System.IO.File.ReadAllTextAsync(path);
            List<User> deserilizedUsers = JsonSerializer.Deserialize<List<User>>(serilizedUserInformation);
            return deserilizedUsers;
        }

        [NonAction]
        public async Task<int> serilizeAndSaveUsers(List<User> users)
        {
            string path = "DataStore/UserInformation.json";
            if (users == null) return -1;
            string serilizedUsers = JsonSerializer.Serialize(users);
            await System.IO.File.WriteAllTextAsync(path, serilizedUsers);
            return 1;
        }
        [NonAction]
        public async Task<int> getLastUserID()
        {
            List<User>? users = await All();
            int lastId = users.Max(user => user.Id);
            return lastId;
        }
    }
}
