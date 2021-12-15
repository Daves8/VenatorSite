using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime DateOfComment { get; set; }
        public string CommentText { get; set; }
        public bool IsHidden { get; set; }
    }
}
