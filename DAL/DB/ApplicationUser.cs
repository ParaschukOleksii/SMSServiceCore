﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebCustomerApp.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        ICollection<Message> Messages { get; set; }
        ICollection<Phone> Phones { get; set; }

        public ApplicationUser()
        {
            Messages = new List<Message>();
            Phones = new List<Phone>();
        }
    }
}
