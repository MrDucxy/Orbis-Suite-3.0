#include "Common.h"
#include "ByteStream.h"

ByteStream::ByteStream(std::vector<unsigned char> data)
{
	Data = data;
}

ByteStream::~ByteStream()
{

}

int ByteStream::ReadInt(bool littleEndian)
{
	int value = 0;

	// Convert the data to int depending on the endianess.
	if (littleEndian)
	{
		for (int j = 0; j < sizeof(int); j++) {
			value |= Data[j] << (8 * j);
		}
	}
	else
	{
		for (int j = sizeof(int) - 1; j >= 0; j--) {
			value |= Data[j] << (8 * (sizeof(int) - j - 1));
		}
	}

	// Remove this data from the stream.
	Data.erase(Data.begin(), Data.begin() + sizeof(int));

	// return the int.
	return value;
}

std::string ByteStream::ReadString(int length)
{
	// Read the string.
	std::string tempStr(Data.begin(), Data.begin() + length);

	// Erase this data from the stream.
	Data.erase(Data.begin(), Data.begin() + length);

	// Return the desired string.
	return tempStr;
}

std::vector<uint8_t> ByteStream::ReadBytes(int length)
{
	std::vector<uint8_t> block;
	for (size_t i = 0; i < length; i++)
	{
		if (i >= Data.size())
		{
			// Handle out-of-bounds access
			break;
		}
		block.push_back(Data.at(i));
	}

	return block;
}