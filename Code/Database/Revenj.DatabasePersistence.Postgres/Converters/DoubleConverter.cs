﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Revenj.DatabasePersistence.Postgres.Converters
{
	public static class DoubleConverter
	{
		public static double? ParseNullable(TextReader reader)
		{
			var cur = reader.Read();
			if (cur == ',' || cur == ')')
				return null;
			return ParseDouble(reader, ref cur);
		}

		public static double Parse(TextReader reader)
		{
			var cur = reader.Read();
			if (cur == ',' || cur == ')')
				return 0;
			return ParseDouble(reader, ref cur);
		}

		private static double ParseDouble(TextReader reader, ref int cur)
		{
			var buf = new char[24];
			var ind = 0;
			do
			{
				buf[ind++] = (char)cur;
				cur = reader.Read();
			} while (cur != -1 && ind < 24 && cur != ',' && cur != ')' && cur != '}');
			return double.Parse(new string(buf, 0, ind), NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		public static List<double?> ParseNullableCollection(TextReader reader, int context)
		{
			var cur = reader.Read();
			if (cur == ',' || cur == ')')
				return null;
			var espaced = cur != '{';
			if (espaced)
			{
				for (int i = 0; i < context; i++)
					reader.Read();
			}
			var list = new List<double?>();
			cur = reader.Peek();
			if (cur == '}')
				reader.Read();
			while (cur != -1 && cur != '}')
			{
				cur = reader.Read();
				if (cur == 'N')
				{
					cur = reader.Read();
					if (cur == 'U')
					{
						reader.Read();
						reader.Read();
						list.Add(null);
					}
					else
					{
						list.Add(double.NaN);
						reader.Read();
					}
					cur = reader.Read();
				}
				else
				{
					list.Add(ParseDouble(reader, ref cur));
				}
			}
			if (espaced)
			{
				for (int i = 0; i < context; i++)
					reader.Read();
			}
			reader.Read();
			return list;
		}

		public static List<double> ParseCollection(TextReader reader, int context)
		{
			var cur = reader.Read();
			if (cur == ',' || cur == ')')
				return null;
			var espaced = cur != '{';
			if (espaced)
			{
				for (int i = 0; i < context; i++)
					reader.Read();
			}
			var list = new List<double>();
			cur = reader.Peek();
			if (cur == '}')
				reader.Read();
			while (cur != -1 && cur != '}')
			{
				cur = reader.Read();
				if (cur == 'N')
				{
					cur = reader.Read();
					if (cur == 'U')
					{
						reader.Read();
						reader.Read();
						list.Add(0);
					}
					else
					{
						list.Add(double.NaN);
						reader.Read();
					}
					cur = reader.Read();
				}
				else
				{
					list.Add(ParseDouble(reader, ref cur));
				}
			}
			if (espaced)
			{
				for (int i = 0; i < context; i++)
					reader.Read();
			}
			reader.Read();
			return list;
		}
	}
}
