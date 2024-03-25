using Models.Entities;
using System.Text.Json;

namespace FakeStoreApi.Helpers
{
    public class UsersHelper
    {
        public async static Task<List<User>> AllUsers()
        {
            string path = "DataStore/UserInformation.json";
            string serilizedUserInformation = await System.IO.File.ReadAllTextAsync(path);
            List<User> deserilizedUsers = JsonSerializer.Deserialize<List<User>>(serilizedUserInformation);
            return deserilizedUsers;
        }

        public async static Task<User?> GetUserById(int id)
        {
            List<User> user = await AllUsers();
            List<User> foundUsers = user.Where(temp => temp.Id == id).ToList();
            if(foundUsers.Count > 1 || foundUsers.Count == 0)
            {
                return null;
            }
            return foundUsers[0];
        }

        public async Task<int> getLastUserID()
        {
            List<User>? users = await AllUsers();
            int lastId = users.Max(user => user.Id);
            return lastId;
        }

        public async Task<int> serilizeAndSaveUsers(List<User> users)
        {
            string path = "DataStore/UserInformation.json";
            if (users == null) return -1;
            string serilizedUsers = JsonSerializer.Serialize(users);
            await System.IO.File.WriteAllTextAsync(path, serilizedUsers);
            return 1;
        }
    }
}
