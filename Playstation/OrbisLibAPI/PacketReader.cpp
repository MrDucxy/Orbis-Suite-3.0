#include "Common.h"
#include "ByteStream.h"
#include "PacketReader.h"

PacketReader::PacketReader(OrbisNetId socket)
{
	Socket = socket;
}

PacketReader::~PacketReader()
{

}

bool PacketReader::ReadPacket()
{
	// Get the initial packet info.
	auto initialPacket = std::make_unique<InitialPacket>();
	if (sceNetRecv(Socket, initialPacket.get(), sizeof(InitialPacket), 0) < sizeof(InitialPacket))
	{
		klog("[PacketReader] Failed to recieve the initial packet\n");
		return false;
	}

	// Make sure the packet version matches up.
	if (initialPacket->Version != PacketVersion || strcmp(initialPacket->Magic, "ORBIS_SUITE"))
	{
		klog("[PacketReader] Packet validation failed. MagicStr '%s' and version '%d' does not meet the requirements of 'ORBIS_SUITE' and '%d'.\n", initialPacket->Magic, initialPacket->Version, PacketVersion);
		return false;
	}

	// Read the packet data.
	std::vector<unsigned char> rawData;
	rawData.resize(initialPacket->Size);
	if (sceNetRecv(Socket, rawData.data(), initialPacket->Size, 0) < initialPacket->Size)
	{
		klog("[PacketReader] Failed to recieve the packet data\n");
		return false;
	}

	// Set the byte stream.
	Data = std::make_shared<ByteStream>(rawData);

	// Read the fields.
	for (int i = 0; i < initialPacket->FieldCount; i++)
		ReadField();

	return true;
}

bool PacketReader::ReadField()
{
	// Read the field type.
	int fieldType = Data->ReadInt();

	// Read the feild size.
	int fieldSize = Data->ReadInt();

	// Read the field name length.
	int fieldNameLen = Data->ReadInt();

	// Read the field name.
	auto fieldName = Data->ReadString(fieldNameLen);

	// Recieve the field data.
	switch ((FieldTypes)fieldType)
	{
	default:
		klog("[PacketReader] Unknown field type of %d\n", fieldType);
		return false;

	case FT_INT:
	{
		// Set up the field.
		auto fieldTemp = std::make_shared<DataField<int>>();
		fieldTemp->FieldType = fieldType;
		fieldTemp->Size = fieldSize;
		fieldTemp->Value = Data->ReadInt();

		// Set the field into the field list.
		Fields[fieldName] = (std::shared_ptr<IDataType>)fieldTemp.get();

		return true;
	}

	case FT_STR:
	{
		// Set up the field.
		auto fieldTemp = std::make_shared<DataField<std::string>>();
		fieldTemp->FieldType = fieldType;
		fieldTemp->Size = fieldSize;

		// Read the string.
		auto stringLen = Data->ReadInt();
		fieldTemp->Value = Data->ReadString(stringLen);

		// Set the field into the field list.
		Fields[fieldName] = (std::shared_ptr<IDataType>)fieldTemp.get();

		return true;
	}

	case FT_BYTES:
	{
		// Set up the field.
		auto fieldTemp = std::make_shared<DataField<std::vector<uint8_t>>>();
		fieldTemp->FieldType = fieldType;
		fieldTemp->Size = fieldSize;

		// Read the string.
		auto stringLen = Data->ReadInt();
		fieldTemp->Value = Data->ReadBytes(stringLen);

		// Set the field into the field list.
		Fields[fieldName] = (std::shared_ptr<IDataType>)fieldTemp.get();

		return true;
	}
	}
}
