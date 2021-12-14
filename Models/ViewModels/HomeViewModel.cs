using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Freelance.Models;
namespace Freelance.Models.ViewModels

{
    public class HomeViewModel
    {
        public List<ViewModel> testModel { get; set; }
        public List<JobPost> posts { get; set; }
        public JobPost post { get; set; }
        public List<User> users { get; set; }
        public UserLogin userLogin { get; set; }
        public User user { get; set; }
        public SavedJob savedPost { get; set; }
        public List<SavedJob> savedPosts { get; set; }

    }
   
}