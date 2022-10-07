using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public interface ITHObject
	{
		public string Name{ get; set; }

		public ChildrenList Children { get; set; }

		public string InnerText{ get; set; }
	}
}
