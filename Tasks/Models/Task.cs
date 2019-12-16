using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tasks.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }
        [Required(ErrorMessage = "Title is mandatory")]
        [MaxLength(25)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Description is mandatory")]
        [MaxLength(400)]
        public string TaskDescription { get; set; }
        [Required(ErrorMessage = "Status is mandatory")]
        public string Status { get; set; }
        [Required(ErrorMessage = "Start Date is mandatory")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End Date is mandatory")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Assigned To is mandatory")]
        public string AssignedToId { get; set; }
        public virtual ApplicationUser AssignedTo{ get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; }
    }
}