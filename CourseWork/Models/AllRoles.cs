using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class AllRoles
    {
        public int Id { get; set; }
        public string Roles { get; set; }

        public AllRoles()
        {
            Roles = JsonConvert.SerializeObject(new List<string>() { "admin", "user" });
        }
    }
}
