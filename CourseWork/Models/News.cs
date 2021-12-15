using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class News
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime DateOfNews { get; set; }
        public string NewsTitle { get; set; }
        public string NewsText { get; set; }
        public bool IsHidden { get; set; }
    }
}
