using MessagePack;

public static class SerializationUtils
{
    public static byte[] SerializeMessage(Command message)
    {
        return MessagePackSerializer.Serialize(message);
    }

    public static Command DeserializeMessage(byte[] data)
    {
        return MessagePackSerializer.Deserialize<Command>(data);
    }
}
