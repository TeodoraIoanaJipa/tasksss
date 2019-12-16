using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tasks.Models
{
    public class Comment
    {
        [Key]
        public int CommentId{ get; set; }
        [Required(ErrorMessage = "Content is mandatory")]
        [MaxLength(30)]
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LeftById { get; set; }
        public virtual ApplicationUser LeftBy { get; set; }

        public int TaskId { get; set; }
        public virtual Task Task { get; set; }
    }
}