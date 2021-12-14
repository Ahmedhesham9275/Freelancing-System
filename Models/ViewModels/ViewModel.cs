using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Freelance.Models.ViewModels
{
    public class ViewModel
    {
        public JobPost ps { get; set; }
        public Proposal pl { get; set; }
        public User ur { get; set; }
    }
}