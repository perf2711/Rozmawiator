using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rozmawiator.Database.ViewModels;
using Rozmawiator.Models;

namespace Rozmawiator.Data
{
    public static class UserService
    {
        public static User LoggedUser { get; set; }

        public static List<User> Users { get; } = new List<User>();

        public static async Task UpdateLoggedUser()
        {
            var response = await RestService.UserApi.GetLogged(RestService.CurrentToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to get logged user.");
            }

            var userViewModel = response.GetModel<UserViewModel>();

            if (LoggedUser != null)
            {
                Users.Remove(Users.FirstOrDefault(u => u.Nickname == LoggedUser.Nickname));
            }

            var user = new User
            {
                Nickname = userViewModel.UserName,
                Avatar = await GetAvatar(userViewModel.UserName)
            };

            LoggedUser = user;
            Users.Add(LoggedUser);
        }

        public static async Task<ImageSource> GetAvatar(string username)
        {
            var response = await RestService.UserApi.GetAvatar(RestService.CurrentToken, username);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var imageBytes = response.ResponseObject as byte[];
            if (imageBytes == null)
            {
                return null;
            }

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageBytes))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static async Task AddUsers(params string[] usernames)
        {
            foreach (var username in usernames)
            {
                if (LoggedUser.Nickname == username || Users.Any(u => u.Nickname == username))
                {
                    continue;
                }

                var user = new User
                {
                    Nickname = username,
                    Avatar = await GetAvatar(username)
                };

                Users.Add(user);
            }
        }
    }
}

