using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github_API_Test.Models.Responces
{
	internal class SingleSelectFieldResponce : Node
	{
		public Node[] Options { get; set; }
	}
}
