using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test.Models.Responces
{
	internal class CanbanIssiesResponce
	{
		public string Id { get; set; } = null;

		public IssueResponse Content { get; set; }

		public FieldValue FieldValues { get; set; }
	}
}
