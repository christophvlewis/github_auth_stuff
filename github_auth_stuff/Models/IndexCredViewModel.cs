using System.Collections.Generic;
using Octokit;

namespace OctokitDemo.Models
{
	public class IndexCredViewModel
	{
		public IndexCredViewModel(Credentials creds)
		{
			Creds = creds;
		}

		public Credentials Creds { get; private set; }
	}
}