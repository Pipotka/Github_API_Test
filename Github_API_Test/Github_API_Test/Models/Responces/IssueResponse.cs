using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test
{
	internal class IssueResponse
	{
		public string Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string Title {  get; set; }

		/// <summary>
		/// Описание проблемы
		/// </summary>
		public string Body { get; set; }
	}
}
