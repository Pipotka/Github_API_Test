using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test
{
	internal class UserProjectsRequest
	{
		public string Query { get; set; } = string.Empty;

		public UserProjectsRequest(string login, int countPerPage)
		{
			Query = $@"{{user(login: ""{login}"") {{projectsV2(first: {countPerPage}) {{nodes {{id title}}}}}}}}";
		}
	}
}
