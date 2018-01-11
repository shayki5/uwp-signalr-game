using ServerSide.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerSide.Repositories
{
    public class UserRepository
    {
        GameDBEntities context = new GameDBEntities();

        public DAL.User CreateUser(User user)
        {
            var checkUsername = (User)context.Users.FirstOrDefault(i => i.UserName == user.UserName);
            if (checkUsername != null)
            {
                return null;
            }
            user.IsOnline = true;
            context.Users.Add(user);

            try
            {
                context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        raise = new InvalidOperationException(message, raise);
                    }
                }
            }

            var newUser = (User)context.Users.FirstOrDefault(i => i.UserName == user.UserName);
            return newUser;

        }

        public List<User> GetAll()
        {
            var users = context.Users.OrderBy(i => i.ID).ToList();
            return users;
        }

        public User GetUser(int id)
        {
            var user = (User)context.Users.Where(i => i.ID == id);
            return user;
        }

        public bool IsValid(User user, ref User currentUser)
        {
            bool IsValid = false;

            var userToCheck = context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (user != null)
            {
                if (userToCheck.Password == user.Password)
                {
                    if (userToCheck.IsOnline == false)
                    {
                        IsValid = true;
                        userToCheck.IsOnline = true;
                        context.SaveChanges();
                        currentUser = userToCheck;
                    }
                }
                else
                {
                    currentUser = null;
                }
            }

            return IsValid;
        }

        public void OfflineUser(User user)
        {
            var userToOffline = context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            userToOffline.IsOnline = false;
            context.SaveChanges();
        }

        public IEnumerable<User> GetAllOnline()
        {
            var users = context.Users.Where(i => i.IsOnline == true).ToList();
            return users;
        }

        public IEnumerable<User> GetAllOffline()
        {
            var users = context.Users.Where(i => i.IsOnline == false).ToList();
            return users;
        }
    }
}