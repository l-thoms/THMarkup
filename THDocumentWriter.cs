using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	enum TextPos
	{
		Name,
		InnerText
	}
	enum OsInformation
	{
		Windows,
		Linux
	}
	public class THDocumentWriter
	{
		Stack<ITHObject> Operator = new Stack<ITHObject>();
		THDocument Document;
		string Text = "";
		OsInformation OsInformation;
		bool ScanList(ITHObject list)
		{
			if (list.GetType() != typeof(THList)) return false;
			bool result = false;
			for(int i = 0; i<list.Children.Count;i++)
			{
				if (HaveLineFeed(list.Children[i].InnerText) || list.Children[i].Children.Count > 0) result = true;
			}
			return result;
		}
		public THDocumentWriter(THDocument document)
		{
			if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
			{
				OsInformation = OsInformation.Windows;
			}
			else
			{
				OsInformation = OsInformation.Linux;
			}
			Document = document;
		}
		string LineFeed
		{
			get
			{
				if (OsInformation == OsInformation.Windows) return "\r\n";
				else return "\n";
			}
		}
		public string Write()
		{
			Text = null;
			Fill(Document);
			return Text;
		}
		void Fill(ITHObject tHObject)
		{
			bool begins = true;
			while (begins || Operator.Peek().Children.Count != 0)
			{
				if (begins)
					Operator.Push(tHObject);
				else
					Operator.Push(Operator.Peek().Children[0]);
				WriteName();
				if ((Operator.Peek().GetType() == typeof(THObject) || Operator.Peek().GetType() == typeof(THListItem)) && Operator.Peek().Children.Count>0)
					Text += LineFeed;
				begins = false;
			}
			if (Operator.Peek().InnerText != null) Text += FormatText(Operator.Peek().InnerText, TextPos.InnerText);
			ITHObject currentObject = Operator.Pop();
			if (Operator.Count == 0) return;
			if(currentObject.GetType() == typeof(THListItem))
			{
				if (Operator.Peek().Children.IndexOf(currentObject) != Operator.Peek().Children.Count - 1)
				{
					if (Text[Text.Length - 1] == '\\') Text += " ";
					Text += ".. ";
				}
			}
			else
			{
				Text += ";" + LineFeed;
			}
			if (currentObject == Operator.Peek().Children[Operator.Peek().Children.Count - 1])
			{
				Close();
			}
			else
			{
				Operator.Push(currentObject);
				MoveNext();
			}
		}
		void MoveNext()
		{
			ITHObject pop = Operator.Pop();
			if (Operator.Count == 0) return;
			int nextCount = Operator.Peek().Children.IndexOf(pop) + 1;
			if (nextCount == Operator.Peek().Children.Count)
			{
				Close();
				if (Operator.Count == 0) return;
				MoveNext();
			}
			else
				Fill(Operator.Peek().Children[nextCount]);
		}
		bool HaveLineFeed(string text)
		{
			if (text == null) return false;
			for (int i = 0; i < text.Length; i++)
				if (IsLineFeed(text[i])) return true;
			return false;
		}
		bool HaveLineFeed(ITHObject thObject)
		{
			if (thObject.InnerText != null) return HaveLineFeed(thObject.InnerText);
			else if(thObject.Children.Count>0)
			{
				if (thObject.GetType() == typeof(THList))
				{
					for (int i = 0; i < thObject.Children.Count; i++)
					{
						if (thObject.Children[i].Children.Count == 0)
						{
							if (thObject.Children[i].InnerText != null && HaveLineFeed(thObject.Children[i].InnerText)) return true;
						}
						else return true;
					}
				}
				else return true;
			}
			return false;
		}
		void Close()
		{
			ITHObject pop = Operator.Pop();
			//string tmpName = pop.Name;
			//if (tmpName == null) tmpName = "NULL";
			//Console.WriteLine("Name: " + tmpName + ", Operator count:" + Operator.Count);
			if (Operator.Count == 0) return;
			if (pop.GetType() == typeof(THListItem))
			{
				if (Operator.Peek().Children.IndexOf(pop) != Operator.Peek().Children.Count - 1)
				{
					if (Text[Text.Length - 1] == '\\') Text += " ";
					if (pop.Children.Count > 0 || HaveLineFeed(pop.InnerText)) Text += Repeat("    ", Operator.Count - ItemCount - 1);
					Text += ".. ";
					ITHObject next = Operator.Peek().Children[Operator.Peek().Children.IndexOf(pop) + 1];
					if ((pop.Children.Count>0 || HaveLineFeed(pop.InnerText)) && (!HaveLineFeed(next.InnerText) && next.Children.Count == 0))
					Text += LineFeed + Repeat("    ", Operator.Count - ItemCount - 1);
				}
			}
			else
			{
				if(ScanList(pop) &&!IsLineFeed(Text[Text.Length-1]))
				{
					Text += LineFeed;
				}
				if(HaveLineFeed(pop))
				{
					Text += Repeat("    ", Operator.Count - ItemCount - 1) + ";" + LineFeed;
				}
				else
					Text += ";" + LineFeed;
			}
			if (pop == Operator.Peek()) Close();
			else 
			{
				Operator.Push(pop);
				MoveNext();
			}
		}
		bool IsWhiteSpace(char target)
		{
			return target == ' ' || target == '\t';
		}
		bool IsLineFeed(char target)
		{
			return target == '\r' || target == '\n';
		}
		string Repeat(string source, int count)
		{
			if (count < 0) throw new Exception("Negative times.");
			string output = "";
			for (int i = 0; i < count; i++)
			{
				output += source;
			}
			return output;
		}
		int CountSlash(string document, int position)
		{
			int count = 0;
			while (position < document.Length)
			{
				if (document[position] != '\\') break;
				count++;position++;
				if (position == document.Length - 1) break;
			}
			return count;
		}
		int SelectSlashCount(string text)
		{
			List<int> slashCounts = new List<int>();
			for(int i = 0;i<text.Length;i++)
			{
				int currentCount = CountSlash(text, i);
				if (text[i] == '\\' && currentCount >= 3)
				{
					if (currentCount % 2 == 1 && !slashCounts.Contains(currentCount)) slashCounts.Add(currentCount);
					i += currentCount - 1;
				}
			}
			int count = 3;
			while(true)
			{
				if (slashCounts.Contains(count))
				{
					count += 2;
				}
				else break;
			}
			return count;
		}
		string FormatText(string text, TextPos pos)
		{
			TextType type = TextType.Normal;
			bool haveLineFeed = false;
			int totalSlashCount = 0;
			if (text == null) return null;
			if(text == "") return "\\ \\";
			switch(pos)
			{
				case TextPos.Name:
					for(int i =0;i<text.Length;i++)
					{
						if (IsWhiteSpace(text[i])||IsLineFeed(text[i]) || text[i] == ':' || text[i] == '.' || text[i] == ';') type = TextType.Single;
						if (text[i] == '\\') totalSlashCount++;
					}
					break;
				case TextPos.InnerText:
					if ((IsWhiteSpace(text[0]) || IsLineFeed(text[0]) || IsWhiteSpace(text[text.Length - 1]) || IsLineFeed(text[text.Length - 1])) && type == TextType.Normal)
						type = TextType.Single;
					for (int i = 0; i < text.Length; i++)
					{
						if (IsLineFeed(text[i])) haveLineFeed = true;
						if ((text[i] == ':' || text[i] == '.' || text[i] == ';') && type == TextType.Normal) type = TextType.Single;
						if (IsLineFeed(text[i]) && i < text.Length - 1 && IsWhiteSpace(text[i + 1]) && type == TextType.Normal) type = TextType.Single;
						if (text[i] == '\\') type = TextType.Multiple;
					}
					break;
			}
			string result = "";
			if (haveLineFeed)
				result += LineFeed;
			switch (type)
			{
				case TextType.Normal:
					if (haveLineFeed) result += Repeat("    ", Operator.Count - ItemCount - 1);
					for (int i =0;i<text.Length;i++)
					{
						if (text[i] == '\\') result += "\\\\";
						else if (text[i] == ':') result += "\\:";
						else if (text[i] == ';') result += "\\;";
						else if (text[i] == '\n') result += "\n" + Repeat("    ", Operator.Count - ItemCount - 1);
						else if (text[i] == '\r')
							if (i < text.Length - 1 && text[i + 1] == '\n') result += text[i];
							else result += "\r" + Repeat("    ", Operator.Count - ItemCount);
						else result += text[i];
					}
					if (haveLineFeed) result += LineFeed +Repeat("    ", Operator.Count - ItemCount - 2);
					break;
				case TextType.Single:
					if (haveLineFeed) result += Repeat("    ", Operator.Count - ItemCount - 1);
					result += "\\";
					for (int i = 0; i < text.Length; i++)//单行文本处理空格，多行不需要
					{
						if (i == 0)
						{
							if (haveLineFeed)
							{
								if (text[i] == '\n') result += "\n" + Repeat("    ", Operator.Count - ItemCount - 1);
								else if (text[i] == '\r')
								{
									if (i < text.Length - 1 && text[i + 1] == '\n')
									{
										result += "\r\n" + Repeat("    ", Operator.Count - ItemCount - 1);
										i++;
									}
									else result += "\r" + Repeat("    ", Operator.Count - ItemCount - 1);
								}
								else result += LineFeed + Repeat("    ", Operator.Count - ItemCount - 1) + text[i];
							}
							else
							{
								if (IsWhiteSpace(text[i]) || text[i] == '\\' || text[i] == ':' || text[i] == ';' || text[i] == '.') result += " " + text[i];
								else if (text[i] == '\n')
									result += " \\.\\";
								else if (text[i] == '\r')
								{
									if (i < text.Length - 1 && text[i + 1] == '\n')
									{
										result += " \\.;\\";
										i++;
									}
									else result += " \\.:\\";
								}
								else result += text[i];
							}
							continue;
						}
						if (text[i] == '\\') result += "\\\\";
						else if (text[i] == '\n')
							if (haveLineFeed)
								result += "\n" + Repeat("    ", Operator.Count - ItemCount - 1);
							else result += "\\.\\";
						else if (text[i] == '\r')
							if (i < text.Length - 1 && text[i + 1] == '\n')
								if (haveLineFeed) result += text[i];
								else
								{
									result += "\\.;\\";
									i++;
								}

							else if (haveLineFeed) result += "\r" + Repeat("    ", Operator.Count - ItemCount - 1);
							else result += "\\.:\\";
						else if (i != 0) result += text[i];
						if (i == text.Length - 1)
						{
							if (haveLineFeed) result += LineFeed;
						}
					}
					if (haveLineFeed) result += Repeat("    ", Operator.Count - ItemCount - 1);
					result += "\\";
					if (haveLineFeed) result += LineFeed + Repeat("    ", Operator.Count - ItemCount - 2);
					break;
				case TextType.Multiple:
					int selectCount = SelectSlashCount(text);
					if (haveLineFeed) result += Repeat("    ", Operator.Count - ItemCount - 1);
					result += Repeat("\\",selectCount);
					for (int i = 0; i < text.Length; i++)//单行文本处理空格，多行不需要
					{
						if (i == 0)
						{
							if (haveLineFeed) result += LineFeed + Repeat("    ", Operator.Count - ItemCount - 1) + text[i];
							else if (IsWhiteSpace(text[i])) result += " " + text[i];
							else if (text[i] == '\\') result += " \\";
							else if (text[i] == '\n') result += "\n" + Repeat("    ", Operator.Count - ItemCount - 1);
							else if (text[i] == '\r')
							{
								if (i < text.Length - 1 && text[i + 1] == '\n') result += text[i];
								else result += "\r" + Repeat("    ", Operator.Count - ItemCount);
							}
							else result += text[i];
						}
						else if (text[i] == '\n') result += "\n" + Repeat("    ", Operator.Count - ItemCount - 1);
						else if (text[i] == '\r')
							if (i < text.Length - 1 && text[i + 1] == '\n') result += text[i];
							else result += "\r" + Repeat("    ", Operator.Count - ItemCount - 1);
						else result += text[i];
						if (i == text.Length - 1)
						{
							if (!haveLineFeed&& text[i] == '\\') result += " ";
							if (haveLineFeed) result += LineFeed;
						}
					}
					if (haveLineFeed) result += Repeat("    ", Operator.Count - ItemCount - 1);
					result += Repeat("\\", selectCount);
					if (haveLineFeed) result += LineFeed + Repeat("    ", Operator.Count - ItemCount - 2);
					break;
			}
			return result;
		}
		int ItemCount
		{
			get
			{
				int count = 0;
				Stack<ITHObject> countObject = new Stack<ITHObject>();
				while(Operator.Count>0)
				{
					ITHObject pop = Operator.Pop();
					if (pop.GetType() == typeof(THListItem)) count++;
					countObject.Push(pop);
				}
				while (countObject.Count > 0)
					Operator.Push(countObject.Pop());
				return count;
			}
		}
		void WriteName()
		{
			if (Operator.Peek().GetType() == typeof(THObject) || Operator.Peek().GetType() == typeof(THList))
				Text += Repeat("    ", Operator.Count - ItemCount - 2);
			if (Operator.Peek().GetType() == typeof(THObject))
			{
				Text += FormatText(Operator.Peek().Name, TextPos.Name) + ": ";
			}
			else if (Operator.Peek().GetType() == typeof(THList))
			{
				if (Operator.Peek().Children.Count <= 1)
				{
					Text += FormatText(Operator.Peek().Name, TextPos.Name);
					if (Text[Text.Length - 1] == '\\') Text += " ..: ";
					else Text += "..: ";
				}
				else
					Text += FormatText(Operator.Peek().Name, TextPos.Name) + ": ";
			}
		}
		public void WriteTo(string path)
		{
			System.IO.File.WriteAllText(path, Write());
		}
	}
}