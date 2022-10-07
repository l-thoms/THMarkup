using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public class THList:ITHObject
	{
		public string Name { get; set; }
		private ChildrenList _children = new ChildrenList();
		public ChildrenList Children
		{
			get => _children;
			set
			{
				_children = value;
			}
		}
		private string _innerText = null;
		public string InnerText
		{
			get => null;
			set
			{
				throw new Exception("The InnerText is unsupported.");
			}
		}

		public THList()
		{
			_children.Checksum += _children_Checksum; _children.ChecksumRange += _children_ChecksumRange;
		}

		private void _children_Checksum(object sender, EventArgs e)
		{
			if (sender.GetType() != typeof(THListItem)) throw new Exception("Type error");
		}
		private void _children_ChecksumRange(object sender, EventArgs e)
		{
			for (int i = 0; i < _children.Count; i++)
			{
				if (_children[i].GetType() != typeof(THListItem))throw new Exception("Type error");
			}
		}
	}
}
