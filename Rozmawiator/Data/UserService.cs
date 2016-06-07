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

        public static User GetUser(string nickname)
        {
            return Users.FirstOrDefault(u => u.Nickname == nickname);
        }

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
                Id = userViewModel.Id,
                Nickname = userViewModel.UserName,
                Avatar = GetImage(userViewModel.Avatar),
                RegistrationDateTime = userViewModel.RegistrationDateTime
            };

            LoggedUser = user;
            Users.Add(LoggedUser);
        }

        public static async Task<User> GetUser(Guid id)
        {
            var response = await RestService.UserApi.Get(RestService.CurrentToken, id);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var userViewModel = response.GetModel<UserViewModel>();
            var user = new User
            {
                Id = userViewModel.Id,
                Nickname = userViewModel.UserName,
                Avatar = GetImage(userViewModel.Avatar),
                RegistrationDateTime = userViewModel.RegistrationDateTime
            };

            return user;
        }

        public static async Task<User[]> Search(string query)
        {
            var response = await RestService.UserApi.Search(RestService.CurrentToken, query);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException();
            }

            var usersViewModels = response.GetModel<UserViewModel[]>();
            var result = new List<User>();

            foreach (var model in usersViewModels)
            {
                result.Add(new User
                {
                    Id = model.Id,
                    Nickname = model.UserName,
                    Avatar = GetImage(model.Avatar),
                    RegistrationDateTime = model.RegistrationDateTime
                });
            }
            return result.ToArray();
        }

        public static async Task<ImageSource> GetAvatar(Guid id)
        {
            var response = await RestService.UserApi.GetAvatar(RestService.CurrentToken, id);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var imageBytes = response.ResponseObject as byte[];
            if (imageBytes == null)
            {
                return null;
            }

            return GetImage(imageBytes);
        }

        public static async Task AddUsers(params Guid[] ids)
        {
            foreach (var id in ids)
            {
                if (LoggedUser.Id == id || Users.Any(u => u.Id == id))
                {
                    continue;
                }

                var user = await GetUser(id);
                Users.Add(user);
            }
        }

        public static ImageSource GetImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
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
    }
}

