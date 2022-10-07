using System;
using System.Collections.Generic;

namespace THMarkup
{
	public class THObject : ITHObject
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
			get => _innerText; set
			{
				_children.Clear();
				_innerText = value;
			}
		}

		public THObject()
		{
			_children.Checksum += _children_Checksum; _children.ChecksumRange += _children_ChecksumRange;
		}

		private void _children_Checksum(object sender, EventArgs e)
		{
			_innerText = null;
			if (sender.GetType() == typeof(THListItem)|| sender.GetType() == typeof(THDocument)) throw new Exception("Type error");
		}
		private void _children_ChecksumRange(object sender, EventArgs e)
		{
			_innerText = null;
			for (int i = 0; i < _children.Count; i++)
			{
				if (_children[i].GetType() == typeof(THListItem) || _children[i].GetType() == typeof(THDocument)) throw new Exception("Type error");
			}
		}
	}
}
