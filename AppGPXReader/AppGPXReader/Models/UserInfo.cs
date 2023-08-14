using System;
using System.Collections.Generic;
using System.Text;

namespace AppGPXReader.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string Picture { get; set; }
    }
}
