using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test
{
	internal class CreateIssueRequest
	{
		/// <summary>
		/// Название
		/// </summary>
		public string Title {  get; set; }

		/// <summary>
		/// Описание проблемы
		/// </summary>
		public string Body { get; set; }

		public string Milestone { get; set; } = null;

		public string[] Labels { get; set; } = new string[0];

		public string[] Assignees { get; set; } = new string[0];
	}
}
