using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tasks.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [MaxLength(20)]
        public string Name { get; set; }
        
        public virtual ICollection<ApplicationUser> Members { get; set; }

        public string[] SelectedUserIds { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}