#pragma once

enum FieldTypes
{
	FT_INT,
	FT_STR,
	FT_BYTES,
};

class IDataType
{
public:
	int FieldType;
	int Size;
};

template <typename T>
class DataField : public IDataType 
{
public:
	T Value;
};

class PacketReader
{
public:
	PacketReader(OrbisNetId sock);
	~PacketReader();

	bool ReadPacket();
	bool ReadField();

private:
	static const int PacketVersion = 1;

	struct InitialPacket
	{
		char Magic[12];
		int Version;
		int Size;
		int FieldCount;
	};

	OrbisNetId Socket;
	
	std::shared_ptr<ByteStream> Data;
	std::map<std::string, std::shared_ptr<IDataType>> Fields;
};
