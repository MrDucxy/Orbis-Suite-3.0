#pragma once

class ByteStream
{
public:
	ByteStream(std::vector<uint8_t> data);
	~ByteStream();

	int ReadInt(bool littleEndian = true);
	std::string ReadString(int length);
	std::vector<uint8_t> ReadBytes(int length);

private:
	std::vector<uint8_t> Data;
};

