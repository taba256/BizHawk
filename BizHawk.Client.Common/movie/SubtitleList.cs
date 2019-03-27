﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BizHawk.Client.Common
{
	public class SubtitleList : List<Subtitle>
	{
		public bool ConcatMultilines { get; set; }
		public bool AddColorTag { get; set; }

		public SubtitleList()
		{
			ConcatMultilines = false;
			AddColorTag = false;
		}

		public IEnumerable<Subtitle> GetSubtitles(int frame)
		{
			return this.Where(t => frame >= t.Frame && frame <= t.Frame + t.Duration);
		}

		public override string ToString()
		{
			Sort();
			return string.Join("\n", this) + "\n";
		}

		public bool AddFromString(string subtitleStr)
		{
			if (!string.IsNullOrWhiteSpace(subtitleStr))
			{
				try
				{
					var subparts = subtitleStr.Split(' ');

					Add(new Subtitle 
					{
						Frame = int.Parse(subparts[1]),
						X = int.Parse(subparts[2]),
						Y = int.Parse(subparts[3]),
						Duration = int.Parse(subparts[4]),
						Color = uint.Parse(subparts[5], NumberStyles.HexNumber),
						Message = string.Join(" ", subparts.Skip(6)) // Unfortunately this is necessary to get the value of Message because the format is space-delimited
					});

					return true;
				}
				catch
				{
					return false;
				}
			}
			
			return false;
		}

		public new void Sort()
		{
			Sort((x, y) =>
			{
				int result = x.Frame.CompareTo(y.Frame);
				return result != 0 ? result : x.Y.CompareTo(y.Y);
			});
		}

		public string ToSubRip(double fps)
		{
			List<Subtitle> subs = new List<Subtitle>();
			foreach (var subtitle in this)
			{
				subs.Add(subtitle);
			}

			// absense of line wrap forces miltiline subtitle macros
			// so we sort them just in case and optionally concat back to a single unit
			// todo: instead of making this pretty, add the line wrap feature to subtitles
			if (ConcatMultilines)
			{
				int lastframe = 0;
				subs = subs.OrderBy(s => s.Frame).ThenBy(s => s.Y).ToList();

				for (int i = 0;; i++)
				{
					if (i == subs.Count) // we're modifying it
					{
						break;
					}

					subs[i].Message = subs[i].Message.Trim();

					if (i > 0 && lastframe == subs[i].Frame)
					{
						subs[i].Message = subs[i - 1].Message + " " + subs[i].Message;
						subs.RemoveAt(i - 1);
						i--;
					}

					lastframe = subs[i].Frame;
				}
			}
			else
			{
				// srt stacks musltilines upwards
				subs = subs.OrderBy(s => s.Frame).ThenByDescending(s => s.Y).ToList();
			}

			var index = 1;
			return string.Concat(subs.Select(subtitle => subtitle.ToSubRip(index++, fps, AddColorTag)));
		}
	}
}