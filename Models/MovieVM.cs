using System;
namespace testing2.Models
{
	public class MovieVM
	{
		public Movie Movie { get; set; }
        public List<ActorMovie> ActorMovie { get; set; }
        public string Sentiment { get; set; }

        public List<string> wikiPages { get; set; }
        public List<double> pageSentiment { get; set; }
    }
}

