using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tasks.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Title is mandatory")]
        [DataType(DataType.Text)][MaxLength(25)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is mandatory")]
        [MaxLength(200)]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public string OrganizerId { get; set; }
        public virtual ApplicationUser Organizer { get; set; }
        
        public int TeamId { get; set; }
        public virtual Team Team { get; set; }

        public string[] SelectedUserIds { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }

}