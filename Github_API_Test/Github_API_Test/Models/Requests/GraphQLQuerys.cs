using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test.Models.Requests
{
	public static class GraphQLQuerys
	{
		public static string GetQueryForIssuesFromColumn(string projectId)
			=> $@"query{{ node(id: ""{projectId}"") {{ ... on ProjectV2 {{ items(first: 100) {{ nodes {{ id content {{ ... on Issue {{ id title body }} }} fieldValues(first: 10) {{ nodes {{ ... on ProjectV2ItemFieldSingleSelectValue {{ name field {{ ... on ProjectV2SingleSelectField {{ id name }} }} }} }} }} }} }} }} }} }}";

		public static string GetQueryForMoveIssues(string projectId, string itemId, string fieldId, string newOptionId)
		{
			var query = @"
        mutation MoveIssueToColumn(
            $projectId: ID!,
            $itemId: ID!,
            $fieldId: ID!,
            $newOptionId: String!
        ) {
            updateProjectV2ItemFieldValue(input: {
                projectId: $projectId,
                itemId: $itemId,
                fieldId: $fieldId,
                value: { 
                    singleSelectOptionId: $newOptionId 
                }
            }) {
                projectV2Item {
                    id
                }
            }
        }";

			var variables = new
			{
				projectId,
				itemId,
				fieldId,
				newOptionId
			};

			return JsonConvert.SerializeObject(new
			{
				query = query.Trim().Replace("\r\n", " "),
				variables
			});
		}
	}
}
