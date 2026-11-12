using System.Linq;

namespace KursoviyProjectHirich
{
    public partial class Register
    {
        public bool IsUserExists(string email)
        {
            return db.Users.Any(u => u.Email == email);
        }

        public void RegisterUser(string name, string email, string pass)
        {
            User user = new User
            {
                Name = name,
                Email = email,
                Password = pass
            };

            db.Users.Add(user);
            db.SaveChanges();
        }
    }
}