using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public class ChildrenList:List<ITHObject>
	{
		public new void Add(ITHObject item)
		{
			Checksum.Invoke(item, new EventArgs());
			base.Add(item);
		}
		public new void AddRange(IEnumerable<ITHObject> collection)
		{
			ChecksumRange.Invoke(collection,new EventArgs());
			base.AddRange(collection);
		}
		public new void Insert(int index, ITHObject item)
		{
			base.Insert(index, item);
			Checksum(item, new EventArgs());
		}
		public new void InsertRange(int index,IEnumerable<ITHObject> collection)
		{
			base.InsertRange(index, collection);
			ChecksumRange(collection, new EventArgs());
		}
		public event EventHandler Checksum;
		public event EventHandler ChecksumRange;
	}
}
