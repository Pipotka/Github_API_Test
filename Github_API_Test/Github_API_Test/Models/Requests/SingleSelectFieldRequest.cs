using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test.Models.Requests
{
	internal class SingleSelectFieldRequest
	{
		public string Query { get; set; } = string.Empty;

		public SingleSelectFieldRequest(string projectId, int countPerPage)
		{
			Query = $@"query{{ node(id: ""{projectId}"") {{ ... on ProjectV2 {{ fields(first: {countPerPage}) {{ nodes {{... on ProjectV2SingleSelectField {{ id name options {{ id name }}}}}}}}}}}}}}";
		}
	}
}
