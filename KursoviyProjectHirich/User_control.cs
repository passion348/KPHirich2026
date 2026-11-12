using System.Linq;

namespace KursoviyProjectHirich
{
    public partial class User_control
    {
        public User Authenticate(string email, string pass)
        {
            using (DataContext db = new DataContext())
            {
                return db.Users.Where(b => b.Email == email && b.Password == pass).FirstOrDefault();
            }
        }
    }
}