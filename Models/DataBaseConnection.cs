using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freelance.Models
{
    public class DataBaseConnection
    {
        SystemDBEntities db = new SystemDBEntities();
        public int GetUserID(string userName)
        {
            var data = db.Users.Where(u => u.UserName == userName);
            if (data.Count() == 1)
            {
                return data.FirstOrDefault().Id;
            }
            else
            {
                return 0;
            }

        }
        public string GetUserType(string userName)
        {
            var data = db.Users.Where(u => u.UserName == userName);
            if (data.Count() == 1)
            {
                return data.FirstOrDefault().UserType;
            }
            else
            {
                return "";
            }
            
                
            
        }
    }
}