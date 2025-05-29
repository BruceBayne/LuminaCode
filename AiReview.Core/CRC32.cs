using System;
using System.Text;

namespace AiReview.Core;

public class CRC32
{
	private static readonly uint[] CrcTable;

	static CRC32()
	{
		CrcTable = new uint[256];
		const uint polynomial = 0xedb88320;
		for (uint i = 0; i < 256; i++)
		{
			uint crc = i;
			for (int j = 8; j > 0; j--)
			{
				if ((crc & 1) == 1)
					crc = crc >> 1 ^ polynomial;
				else
					crc >>= 1;
			}
			CrcTable[i] = crc;
		}
	}

	public static uint Compute(string input, Encoding? encoding = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		encoding ??= Encoding.UTF8; // Default to UTF-8 if no encoding is provided
		byte[] data = encoding.GetBytes(input);
		uint crc = 0xffffffff;

		foreach (byte b in data)
		{
			byte tableIndex = (byte)((crc ^ b) & 0xff);
			crc = crc >> 8 ^ CrcTable[tableIndex];
		}

		return ~crc;
	}
}