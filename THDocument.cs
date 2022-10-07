using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public class THDocument:ITHObject
	{
		public string Name { get=>null; set { throw new Exception("The name is unsupported."); } }
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

		public THDocument()
		{
			_children.Checksum += _children_Checksum; _children.ChecksumRange += _children_ChecksumRange;
		}

		private void _children_Checksum(object sender, EventArgs e)
		{
			if (sender.GetType() == typeof(THListItem) || sender.GetType() == typeof(THDocument)) throw new Exception("Type error");
		}
		private void _children_ChecksumRange(object sender, EventArgs e)
		{
			for (int i = 0; i < _children.Count; i++)
			{
				if (_children[i].GetType() == typeof(THListItem) || _children[i].GetType() == typeof(THDocument)) throw new Exception("Type error");
			}
		}
	}
}
