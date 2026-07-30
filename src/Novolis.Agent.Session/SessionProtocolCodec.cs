using MessagePack;

namespace Novolis.Agent.Session;

public static class SessionProtocolCodec
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard;

    public static byte[] Serialize<T>(T value) => MessagePackSerializer.Serialize(value!, Options);

    public static T Deserialize<T>(byte[] payload) =>
        MessagePackSerializer.Deserialize<T>(payload, Options);
}
