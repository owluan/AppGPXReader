using System;
using System.Collections.Generic;
using System.Text;

namespace AppGPXReader.Models
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Given_Name { get; set; }
        public string Family_Name { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
        public bool Verified_Email { get; set; }
    }
}
