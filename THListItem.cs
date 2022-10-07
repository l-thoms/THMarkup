using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public class THListItem:ITHObject
	{
		public string Name { get => null; set { throw new Exception("The name is unsupported."); } }
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
			get => _innerText;
			set
			{
				_innerText = value;
				_children.Clear();
			}
		}

		public THListItem()
		{
			_children.Checksum += _children_Checksum; _children.ChecksumRange += _children_ChecksumRange;
		}

		private void _children_Checksum(object sender, EventArgs e)
		{
			_innerText = null;
			if (sender.GetType() != typeof(THObject) && sender.GetType() != typeof(THList)) throw new Exception("Type error");
		}
		private void _children_ChecksumRange(object sender, EventArgs e)
		{
			_innerText = null;
			for (int i = 0; i < _children.Count; i++)
			{
				if (_children[i].GetType() != typeof(THObject)) throw new Exception("Type error");
			}
		}
	}
}
