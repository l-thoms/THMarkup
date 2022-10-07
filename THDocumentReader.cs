using System;
using System.Collections.Generic;
using System.Text;

namespace THMarkup
{
	public enum ReadState
	{
		Space,
		Name,
		NameBreak,
		BeginOfObject,
		InnerText,
		InnerTextGap
	}
	public enum TextType
	{
		Normal,
		Single,
		Multiple
	}
	public enum CommentType
	{
		None,
		LineComment,
		BlockComment
	}
	public enum SlashInfo
	{
		None,
		LineComment,
		BlockComment,
		LF,
		CR,
		CRLF,
		EscapeBegin,
		EscapeEnd
	}
	public enum LineFeed
	{
		None,
		LF,
		CR,
		CRLF
	}
	public class THDocumentReader
	{
		string[] LineFeed = { "\r", "\n", "\r\n" };
		bool IsWhiteSpace(char target)
		{
			return target == ' ' || target == '\t';
		}
		bool IsLineFeed(char target)
		{
			return target == '\r' || target == '\n';
		}

		int CountSlash(string document, int position)
		{
			int count = 0;
			while (position < document.Length)
			{
				if (document[position] != '\\') break;
				count++; position++;
			}
			return count;
		}
		SlashInfo GetSlashInfo(string document, int position)
		{
			int count = CountSlash(document, position);
			SlashInfo info = SlashInfo.None;
			if (count % 2 == 0 || position + count >= document.Length)
				return info;
			position += count;
			if(position < document.Length)
			{
				if (document[position] == '.')
				{
					info = SlashInfo.BlockComment;
					if(document.Length > position+1)
					{
						if (document[position + 1] == '\\')
							info = SlashInfo.LF;
						else if (document[position + 1] == '.') info = SlashInfo.LineComment;
						else if (position + 2 < document.Length && document[position + 2] == '\\')
						{
							if (document[position + 1] == ';') info = SlashInfo.CRLF;
							else if (document[position + 1] == ':') info = SlashInfo.CR;
						}
					}
				}
				else if (document[position] == ';')
					info = SlashInfo.EscapeEnd;
				else if (document[position] == ':')
					info = SlashInfo.EscapeBegin;
			}
			return info;
		}

		string Repeat(string source,int count)
		{
			string output = "";
			for(int i =0;i<count;i++)
			{
				output += source;
			}
			return output;
		}

		int GetSpaceEnd(string document, int position)
		{
			while (position < document.Length)
			{
				if (!IsWhiteSpace(document[position]) && !IsLineFeed(document[position]))
				{
					if(CountSlash(document,position) == 1)
					{
						if(GetSlashInfo(document,position) == SlashInfo.LineComment || GetSlashInfo(document,position) == SlashInfo.BlockComment)
						{
							position = GetCommentEnd(document, position);
							continue;
						}
					}
					return position;
				}
				position++;
			}
			return -1;
		}
		int GetLineSpaceEnd(string document, int position)
		{
			while (position < document.Length)
			{
				if (!IsWhiteSpace(document[position]) || IsLineFeed(document[position]))
				{
					if (CountSlash(document, position) == 1)
					{
						if (GetSlashInfo(document, position) == SlashInfo.LineComment || GetSlashInfo(document, position) == SlashInfo.BlockComment)
						{
							position = GetCommentEnd(document, position);
							continue;
						}
					}
					return position;
				}
				position++;
			}
			return document.Length - 1;
		}
		LineFeed GetLineFeed(string document, int position)
		{
			if (position >= document.Length) return THMarkup.LineFeed.None;
			if (IsLineFeed(document[position]))
				if (document[position] == '\n') return THMarkup.LineFeed.LF;
				else if (document[position] == '\r' && (position == document.Length - 1 || document[position + 1] != '\n')) return THMarkup.LineFeed.CR;
				else return THMarkup.LineFeed.CRLF;
			else return THMarkup.LineFeed.None;
		}

		int GetCommentEnd(string document, int position)
		{
			CommentType commentType;
			if (position < document.Length - 2 && document.Substring(position, 3) == "\\..") commentType = CommentType.LineComment;
			else if (document.Substring(position, 2) == "\\.") commentType = CommentType.BlockComment;
			else return -1;
			while (position < document.Length)
			{
				switch (commentType)
				{
					case CommentType.LineComment:
						{
							if (position >= document.Length - 1) break;
							else if (GetLineFeed(document, position) == THMarkup.LineFeed.LF || GetLineFeed(document, position) == THMarkup.LineFeed.CR) return position + 1;
							else if (GetLineFeed(document, position) == THMarkup.LineFeed.CRLF) return position + 2;
						}
						break;
					case CommentType.BlockComment:
						{
							if (position >= document.Length - 2) break;
							else if (document.Substring(position, 2) == ".\\") return position + 2;
						}
						break;
				}
				position++;
			}
			return document.Length;
		}
		int GetSpaceAlignment(string text)
		{
			int spaceAlignment = 0;
			for(int i = 0; i <text.Length;i++)
			{
				if(text[i] == ' ')
				{
					spaceAlignment++;
				}
				else if(text[i] == '\t')
				{
					spaceAlignment = (spaceAlignment + 4) / 4 * 4;
				}
				else
				{
					return spaceAlignment / 4 * 4;
				}
			}
			return 0;
		}
		int GetContentLineCommentEnd(string content, int position, int alignment)
		{
			bool beginOfLine = false;
			int linePos = 0;
			while (position < content.Length)
			{
				if (!beginOfLine)
				{
					if (GetLineFeed(content, position) == THMarkup.LineFeed.LF || GetLineFeed(content, position) == THMarkup.LineFeed.CR)
					{
						beginOfLine = true; position++; linePos = position; continue;
					}
					else if (GetLineFeed(content, position) == THMarkup.LineFeed.CRLF)
					{
						beginOfLine = true; position += 2; linePos = position; continue;
					}
				}
				else if (alignment == 0) return position;
				else
				{
					return GetBeginOfLine(content, position, alignment) + 1;
				}
				position++;
			}
			return content.Length - 1;
		}
		int GetBeginOfLine(string content,int position,int alignment)
		{
			alignment = alignment / 4 * 4;
			int linePos = position;
			int tmpAlignment = 0;
			while(position < content.Length)
			{
				if (alignment < 0)
				{
					if (!IsWhiteSpace(content[position]))
					{
						int maxPos = GetSpaceAlignment(content.Substring(linePos, position - linePos));
						for (int i = linePos; i < position; i++)
						{
							if (GetSpaceAlignment(content.Substring(linePos, i - linePos)) == maxPos)
							{
								return i;
							}
						}
						return linePos;
					}
				}
				else if (alignment == 0) return position - 1;
				else
				{
					if (content[position] == ' ') tmpAlignment++;
					else if (content[position] == '\t') tmpAlignment = (tmpAlignment + 4) / 4 * 4;
					if (tmpAlignment == alignment)
						return position;
					else if (!IsWhiteSpace(content[position])) return position - 1;
				}
				position++;
			}
			return content.Length - 1;
		}
		void CloseObject()
		{
			if (Buffer.Peek().GetType() == typeof(THListItem))
			{
				ITHObject item = Buffer.Pop();
				ITHObject list = Buffer.Pop();
				list.Children.Add(item);
				if (Buffer.Count > 0)
					Buffer.Peek().Children.Add(list);
				else Document.Children.Add(list); 
			}
			else
			{
				ITHObject pop = Buffer.Pop();
				if (Buffer.Count > 0)
					Buffer.Peek().Children.Add(pop);
				else Document.Children.Add(pop);
			}
			CurrentState = ReadState.Space;
		}
		void CloseItem()
		{
			if(Buffer.Peek().GetType() == typeof(THListItem))
			{
				ITHObject pop = Buffer.Pop();
				if (pop.Children.Count == 0) pop.InnerText =FormatText( CurrentText,Alignment);
				Buffer.Peek().Children.Add(pop);
				CurrentText = "";
			}
			CurrentState = ReadState.BeginOfObject;
		}
		void ConvertToList()
		{
			if(Buffer.Peek().GetType() != typeof(THListItem))
			{
				ITHObject pop = Buffer.Pop();
				THList list = new THList() { Name = pop.Name };
				THListItem item = new THListItem();
				item.InnerText = pop.InnerText;
				if (pop.Children.Count > 0)
				{
					for (int i = 0; i < pop.Children.Count; i++)
					{
						item.Children.Add(pop.Children[i]);
					}
				}
				Buffer.Push(list);
				Buffer.Push(item);
			}
		}
		string GetTextRange(string document, int position)
		{
			int p0 = position;
			if(GetSlashInfo(document,position) == SlashInfo.None && CountSlash(document,position) % 2 != 0)//Single, Multiple
			{
				if(CountSlash(document,position) == 1)//Single
				{
					position++;
					while (position < document.Length)
					{
						if (document[position] == '\\')
						{
							SlashInfo info = GetSlashInfo(document, position);
							if (info == SlashInfo.None || info == SlashInfo.EscapeBegin || info == SlashInfo.EscapeEnd)
							{
								if (CountSlash(document, position) % 2 == 1)
								{
									position += CountSlash(document, position);
									break;
								}
								position += CountSlash(document, position);
							}
							else if (info == SlashInfo.CR || info == SlashInfo.CRLF)
							{
								position += CountSlash(document, position) + 3;
								continue;
							}
							else if (info == SlashInfo.LF)
							{
								position += CountSlash(document, position) + 2;
								continue;
							}
							else if (info == SlashInfo.LineComment || info == SlashInfo.BlockComment)
							{
								position = GetCommentEnd(document, position);
								continue;
							}
						}
						position++;
					}
				}
				else//Multiple
				{
					int currentCount = CountSlash(document, position);
					position += currentCount;
					while (position < document.Length)
					{
						if (CountSlash(document, position) == currentCount)
						{
							position += CountSlash(document, position);
							break;
						}
						else if (document[position] == '\\')
						{
							position += CountSlash(document, position);
						}
						else position++;
					}
				}
			}
			else//Normal
			{
				while(position < document.Length)
				{
					if(IsWhiteSpace(document[position]) || IsLineFeed(document[position]))
					{
						if(CurrentState == ReadState.Name)
						{
							position--;
							break;
						}
					}
					else if(document[position] == '\\')
					{
						SlashInfo info = GetSlashInfo(document, position);
						if (info == SlashInfo.None)
						{
							if (CountSlash(document, position) % 2 == 1) throw new Exception("Escape invalid");
							position += CountSlash(document, position);
						}
						else if(info == SlashInfo.EscapeBegin || info == SlashInfo.EscapeEnd)
						{
							position += CountSlash(document,position) + 1;
							continue;
						}
						else if(info == SlashInfo.CR || info == SlashInfo.CRLF)
						{
							position += CountSlash(document, position) + 3;
							continue;
						}
						else if(info == SlashInfo.LF)
						{
							position += CountSlash(document, position) + 2;
							continue;
						}
						else if(info == SlashInfo.LineComment || info == SlashInfo.BlockComment)
						{
							if (CurrentState == ReadState.Name)
							{
								position += CountSlash(document, position) - 2;
								break;
							}
							position = GetCommentEnd(document, position);
							continue;
						}
					}
					else if(position < document.Length-1 && document.Substring(position,2) == ".."|| document[position] == ':' || document[position] == ';')
					{
						position--;
						break;
					}
					position++;
				}
				position ++;
			}
			if (position == document.Length) position = document.Length - 1;
			string rangeText = document.Substring(p0, position - p0);
			return rangeText;
		}
		int GetAlignment(string document, int position)
		{
			int headPos = -1;
			position--;
			if (position == -1 || IsLineFeed(document[position])) return 0;//Head of line
			while (position >= 0 &&IsWhiteSpace(document[position]))
			{
				if (headPos == -1) headPos = 0;
				if (document[position] == ' ') headPos++;
				else if (document[position] == '\t') headPos = (headPos + 4) / 4 * 4;
				position--;
				if (position == -1 || IsLineFeed(document[position])) break;
				else if (!IsLineFeed(document[position]) && !IsWhiteSpace(document[position]))
				{
					headPos = -1;
					break;
				}
			}
			if (headPos != -1)
				headPos = headPos / 4 * 4;
			return headPos;
		}
		int GetNextLine(string content, int position)
		{
			while(position < content.Length)
			{
				switch(GetLineFeed(content,position))
				{
					case THMarkup.LineFeed.LF:
					case THMarkup.LineFeed.CR:
						return position + 1;
					case THMarkup.LineFeed.CRLF:
						return position + 2;
				}
				position++;
			}
			return -1;
		}
		string FormatText(string content, int alignment)
		{
			string result = "";
			int subAlignment = 0, tmpAlignment = 0;
			//Get Alignment, 空行不参与计数, 至下一行开始计数
			int cp = GetNextLine(content,0);
			bool beginOfLine = false, beginAlignment = true;
			if (cp == -1)
			{
				subAlignment = alignment;
			}
			else
			{
				while (cp < content.Length)
				{
					if (GetLineFeed(content, cp) == THMarkup.LineFeed.LF || GetLineFeed(content, cp) == THMarkup.LineFeed.CR)
					{
						cp++;
						tmpAlignment = 0;
						continue;
					}
					else if (GetLineFeed(content, cp) == THMarkup.LineFeed.CRLF)
					{
						cp += 2;
						tmpAlignment = 0;
						continue;
					}
					if (IsWhiteSpace(content[cp]))
					{
						if (content[cp] == ' ')
						{
							tmpAlignment++;
						}
						else if (content[cp] == '\t')
						{
							tmpAlignment = (tmpAlignment + 4) / 4 * 4;
						}
					}
					else
					{
						tmpAlignment = tmpAlignment / 4 * 4;
						if (beginAlignment)
						{
							subAlignment = tmpAlignment;
							beginAlignment = false;
						}
						else subAlignment = Math.Min(subAlignment, tmpAlignment);
						cp = GetNextLine(content, cp);
						tmpAlignment = 0;
						if (cp == -1) break;
						continue;
					}
					cp++;
				}
			}
			//Console.WriteLine("Alignment: " + alignment + ", SubAlignment: " + subAlignment);
			if (alignment != -1)
				subAlignment = Math.Min(subAlignment, alignment);
			//End of alignment
			if(CountSlash(content,0) % 2 == 1 && GetSlashInfo(content,0) == SlashInfo.None)//Single,Multiple
			{
				if(CountSlash(content,0) == 1)//single
				{
					int pos = 1;
					while(pos < content.Length)
					{ 
						if(beginOfLine)
						{
							pos = GetBeginOfLine(content, pos, subAlignment);
							beginOfLine = false;
						}
						else if (content[pos] == '\\')
						{
							SlashInfo info = GetSlashInfo(content, pos);
							result += Repeat("\\", CountSlash(content, pos) / 2);
							if (info == SlashInfo.None || info == SlashInfo.EscapeBegin || info == SlashInfo.EscapeEnd)
							{
								if (CountSlash(content, pos) % 2 == 0)
									pos += CountSlash(content, pos) - 1;
								else
									break;
							}
							if (info == SlashInfo.LF)
							{
								result += "\n";
								pos += CountSlash(content, pos) + 2;
								continue;
							}
							else if (info == SlashInfo.CR)
							{
								result += "\r";
								pos += CountSlash(content, pos) + 3;
								continue;
							}
							else if (info == SlashInfo.CRLF)
							{
								result += "\r\n";
								pos += CountSlash(content, pos) + 3;
								continue;
							}
							else if (info == SlashInfo.LineComment)
							{
								pos += CountSlash(content, pos) + 2;
								pos = GetContentLineCommentEnd(content, pos, subAlignment);
								continue;
							}
							else if (info == SlashInfo.BlockComment)
							{
								pos += CountSlash(content, pos) - 1;
								pos = GetCommentEnd(content, pos);
								continue;
							}
						}
						else if (IsLineFeed(content[pos]))
						{
							beginOfLine = true;
							if (GetLineFeed(content, pos) == THMarkup.LineFeed.CR || GetLineFeed(content, pos) == THMarkup.LineFeed.LF)
							{
								result += content[pos];
								pos++;
							}
							else
							{
								result += "\r\n";
								pos += 2;
							}
							continue;
						}
						else
						{
							result += content[pos];
						}
						pos++;
					}
					if (result.Length>0 &&( IsWhiteSpace(result[0]) || GetLineFeed(result, 0) == THMarkup.LineFeed.LF || GetLineFeed(result, 0) == THMarkup.LineFeed.CR))
						result = result.Substring(1, result.Length - 1);
					else if (result.Length>1 && GetLineFeed(result, 0) == THMarkup.LineFeed.CRLF)
						result = result.Substring(2, result.Length - 2);
					if (result.Length > 1 && GetLineFeed(result, result.Length - 2) == THMarkup.LineFeed.CRLF) result = result.Substring(0, result.Length - 2);
					else if (result.Length > 0 && GetLineFeed(result, result.Length - 1) != THMarkup.LineFeed.None) result = result.Substring(0, result.Length - 1);
				}
				else//multiple
				{
					int slashCount = CountSlash(content, 0);
					int pos = slashCount;
					while(pos < content.Length)
					{
						if (beginOfLine)
						{
							pos = GetBeginOfLine(content, pos, subAlignment) + 1;
							beginOfLine = false;
							continue;
						}
						else if (IsLineFeed(content[pos]))
						{
							beginOfLine = true;
							if (GetLineFeed(content, pos) == THMarkup.LineFeed.CR || GetLineFeed(content, pos) == THMarkup.LineFeed.LF)
							{
								result += content[pos];
								pos++;
							}
							else
							{
								result += "\r\n";
								pos += 2;
							}
							continue;
						}
						else if(content[pos] == '\\')
						{
							if (CountSlash(content, pos) == slashCount) break;
							else
							{
								result += Repeat("\\", CountSlash(content, pos));
								pos += CountSlash(content, pos);
								continue;
							}
						}
						else
						{
							result += content[pos];
						}
						pos++;
					}
					if (result.Length > 0 &&(IsWhiteSpace(result[0]) || GetLineFeed(result, 0) == THMarkup.LineFeed.LF || GetLineFeed(result, 0) == THMarkup.LineFeed.CR))
						result = result.Substring(1, result.Length - 1);
					else if (result.Length>1 && GetLineFeed(result, 0) == THMarkup.LineFeed.CRLF)
						result = result.Substring(2, result.Length - 2);
					if (result.Length > 1 && GetLineFeed(result, result.Length - 2) == THMarkup.LineFeed.CRLF) result = result.Substring(0, result.Length - 2);
					else if (result.Length > 0 && (GetLineFeed(result, result.Length - 1) != THMarkup.LineFeed.None || IsWhiteSpace(result[result.Length - 1])))
					result = result.Substring(0, result.Length - 1);
				}
			}
			else // Normal
			{
				int endPos = content.Length - 1;
				//Cut foot
				for(int i = content.Length - 1; i>= 0;i--)
				{
					if (!IsWhiteSpace(content[i]) && !IsLineFeed(content[i]) || i == 0)
					{
						endPos = i + 1;
						break;
					}
				}
				if (endPos <= 0) return null;
				content = content.Substring(0, endPos);
				int pos = 0;
				while(pos < content.Length)
				{
					if(beginOfLine)
					{
						pos = GetLineSpaceEnd(content,pos);
						beginOfLine = false;
					}
					else if(content[pos] == '\\')
					{
						SlashInfo info = GetSlashInfo(content,pos);
						result += Repeat("\\", CountSlash(content, pos) / 2);
						if(info == SlashInfo.None)
						{
							pos += CountSlash(content, pos);
							continue;
						}
						else if(info == SlashInfo.LF)
						{
							result += "\n";
							pos += CountSlash(content, pos) + 2;
							continue;
						}
						else if (info == SlashInfo.CR)
						{
							result += "\r";
							pos += CountSlash(content, pos) + 3;
							continue;
						}
						else if(info == SlashInfo.CRLF)
						{
							result += "\r\n";
							pos += CountSlash(content, pos) + 3;
							continue;
						}
						else if(info == SlashInfo.EscapeBegin)
						{
							result += ":";
							pos += CountSlash(content, pos) + 1;
							continue;
						}
						else if (info == SlashInfo.EscapeEnd)
						{
							result += ";";
							pos += CountSlash(content, pos) + 1;
							continue;
						}
						else if(info == SlashInfo.LineComment)
						{
							pos += CountSlash(content, pos) - 1;
							pos = GetCommentEnd(content, pos);
							beginOfLine = true;
							continue;
						}
						else if(info == SlashInfo.BlockComment)
						{
							pos += CountSlash(content, pos) -1;
							pos = GetCommentEnd(content, pos);
							continue;
						}
					}
					else if(IsLineFeed(content[pos]))
					{
						beginOfLine = true;
						if (GetLineFeed(content, pos) == THMarkup.LineFeed.CR || GetLineFeed(content, pos) == THMarkup.LineFeed.LF)
						{
							result += content[pos];
							pos++;
						}
						else
						{
							result += "\r\n";
							pos += 2;
						}
						continue;
					}
					else
					{
						result += content[pos];
					}
					pos++;
				}
			}
			//Console.WriteLine("Result:\n" + result);
			return result;
		}
		bool GetListForawrd(string document, int position)
		{
			position = GetSpaceEnd(document, position);
			if (document[position] == ':')
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		Stack<ITHObject> Buffer = new Stack<ITHObject>();
		string CurrentText = "";
		int Alignment = 0;
		ReadState CurrentState = ReadState.Space;
		bool IsList = false;
		THDocument Document;
		public THDocument Read(string document)
		{
			/*
			//Get lines for debug
			List<int> lines = new List<int>();
			lines.Add(0);
			int currentLine = 0;
			for (int j = 0; j < document.Length; j++)
			{
				if (GetLineFeed(document, j) == THMarkup.LineFeed.LF || GetLineFeed(document, j) == THMarkup.LineFeed.CR)
				{
					lines.Add(j + 1);
				}
				else if (GetLineFeed(document, j) == THMarkup.LineFeed.CRLF)
				{
					lines.Add(j + 2);
					j++;
				}
			}
			*/
			//Begin to parse
			CurrentText = "";
			CurrentState = ReadState.Space;
			Alignment = 0;
			IsList = false;
			Buffer.Clear();
			Document = new THDocument();
			int i = 0;
			while(i < document.Length)
			{
				/*
				for(int l = 0;l<lines.Count;l++)
				{
					if (i >= lines[l] && (l == lines.Count-1 || i < lines[l+1]))
					{
						if(l != currentLine)
						{
							Console.WriteLine();
							currentLine = l;
						}
						Console.Write("Line: " + (l + 1).ToString() + ", Col: " + (i - lines[l] + 1).ToString() + "(" +i.ToString() + "), BufferCount: " + Buffer.Count.ToString()
						+", State: " + CurrentState.ToString());
						if(Buffer.Count > 0)
						{
							Console.Write(", Type: " + Buffer.Peek().GetType().ToString());
						}
						Console.WriteLine();
						break;
					}
				}
				*/
				if (CurrentState == ReadState.Space)
				{
					IsList = false;
					i = GetSpaceEnd(document, i);
					if (i == -1) break;
					else if (document[i] == ';')
					{
						CloseObject();
						i++;
						continue;
					}
					else if(i < document.Length-1 && document.Substring(i,2) == "..")
					{
						if(Buffer.Peek().GetType() != typeof(THListItem))
						{
							ConvertToList();
						}
						CloseItem();
						i += 2;
						Buffer.Push(new THListItem());
						CurrentState = ReadState.BeginOfObject;
						continue;
					}
					else
						CurrentState = ReadState.Name;
					continue;
				}
				else if (CurrentState == ReadState.Name)
				{
					Alignment = GetAlignment(document, i);
					CurrentText = GetTextRange(document, i);
					i += CurrentText.Length;
					CurrentState = ReadState.NameBreak;
					continue;
				}
				else if (CurrentState == ReadState.NameBreak)
				{
					i = GetSpaceEnd(document, i);
					if(document[i] == ':')
					{
						if (!IsList)
						{
							THObject currentObject = new THObject();
							currentObject.Name = FormatText(CurrentText, Alignment);
							Buffer.Push(currentObject);
						}
						else
						{
							THList currentList = new THList();
							THListItem currentItem = new THListItem();
							currentList.Name = FormatText(CurrentText, Alignment);
							Buffer.Push(currentList);
							Buffer.Push(currentItem);
						}
						CurrentText = "";
						CurrentState = ReadState.BeginOfObject;
					}
					else if(i < document.Length - 1 && document.Substring(i,2) == "..")
					{
						IsList = true;
						i += 2;
						continue;
					}
				}
				else if (CurrentState == ReadState.BeginOfObject)
				{
					i = GetSpaceEnd(document, i);
					if (document[i] == ';')
					{
						CloseObject();
						i++;
						continue;
					}
					else CurrentState = ReadState.InnerText;
					continue;
				}
				else if (CurrentState == ReadState.InnerText)
				{
					Alignment = GetAlignment(document, i);
					CurrentText = GetTextRange(document, i);
					i += CurrentText.Length;
					CurrentState = ReadState.InnerTextGap;
					continue;
				}
				else if(CurrentState == ReadState.InnerTextGap)
				{
					i = GetSpaceEnd(document, i);
					if (document[i] == ';')
					{
						//if (Buffer.Count == 0) break;
						Buffer.Peek().InnerText = FormatText(CurrentText, Alignment);
						CurrentText = "";
						CloseObject();
					}
					else if(document[i] == ':')
					{
						CurrentState = ReadState.NameBreak;
						continue;
					}
					else if (document.Substring(i, 2) == "..")
					{
						Buffer.Peek().InnerText = FormatText(CurrentText, Alignment);
						if (GetListForawrd(document, i + 2))
						{
							CurrentState = ReadState.NameBreak;
							continue;
						}
						else if (Buffer.Peek().GetType() == typeof(THListItem))
						{
							CloseItem();
							Buffer.Push(new THListItem());
						}
						else
						{
							ConvertToList();
							CloseItem();
							Buffer.Push(new THListItem());
							CurrentState = ReadState.BeginOfObject;
							CurrentText = "";
						}
						i += 2;
						continue;
					}
				}
				i++;
			}
			return Document;
		}
		public THDocument ReadFrom(string path)
		{
			return Read(System.IO.File.ReadAllText(path));
		}
	}
}
