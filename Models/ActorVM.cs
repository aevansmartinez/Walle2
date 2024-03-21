using System;
namespace testing2.Models
{
	public class ActorVM
	{
        public Actor Actor { get; set; }
        public List<ActorMovie> ActorMovie { get; set; }
        public string Sentiment { get; set; }
    }
}

