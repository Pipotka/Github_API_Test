using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test.Models.Responces
{
	internal class FieldValueNode
	{
		public string Name { get; set; }

		public Node Field {  get; set; }
	}
}
