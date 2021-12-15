using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    [Obsolete]
    public static class RolesManager
    {
        static private List<string> roles = new List<string>() { "admin", "moderator", "user" };

        public static List<string> GetAllPossibleRoles()
        {
            return roles;
        }

        public static void AddPossibleRole(string role)
        {
            if (!roles.Contains(role))
            {
                roles.Add(role);
            }
        }

        public static void RemovePossibleRole(string role)
        {
            if (roles.Contains(role))
            {
                roles.Remove(role);
            }
        }

        public static void AddRoleToUser(User user, string role)
        {
            if (roles.Contains(role))
            {
                List<string> userRoles = GetAllUserRoles(user);
                userRoles.Add(role);
                user.Roles = JsonConvert.SerializeObject(userRoles);
            }
        }

        public static void RemoveRoleFromUser(User user, string role)
        {
            List<string> userRoles = GetAllUserRoles(user);
            if (userRoles.Contains(role))
            {
                userRoles.Remove(role);
                user.Roles = JsonConvert.SerializeObject(userRoles);
            }
        }

        public static List<string> GetAllUserRoles(User user)
        {
            return JsonConvert.DeserializeObject<List<string>>(user.Roles);
        }
    }
}