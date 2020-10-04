using ExitGames.Client.Photon;

public static class CustomTypeSerialization
{
    const byte BOARD_POSITION_CODE = 255;

    public static void RegisterTypes()
    {
        PhotonPeer.RegisterType(typeof(BoardPosition), BOARD_POSITION_CODE, SerializeBoardPosition, DeserializeBoardPosition);
    }

    public static object DeserializeBoardPosition(byte[] data)
    {
        return new BoardPosition(data[0], data[1]);
    }

    public static byte[] SerializeBoardPosition(object customType)
    {
        var c = (BoardPosition)customType;
        return new byte[] { c.x, c.y };
    }
}