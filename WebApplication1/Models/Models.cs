using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class Moderation
    {
        public int ModerationID { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
        public int ActionID { get; set; }
        public Action Action { get; set; }
    }

    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public ICollection<Moderation> Moderations { get; set; }
    }

    public class Action
    {
        public int ActionID { get; set; }
        public string ActionName { get; set; }
        public ICollection<Moderation> Moderations { get; set; }
    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
