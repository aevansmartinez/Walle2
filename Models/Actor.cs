using System;
namespace testing2.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IMDBLink { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public byte[]? Photo { get; set; }
    }
}
