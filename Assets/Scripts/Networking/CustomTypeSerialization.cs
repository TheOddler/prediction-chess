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
        int x = data[0];
        int y = data[1];
        return new BoardPosition(x, y);
    }

    public static byte[] SerializeBoardPosition(object customType)
    {
        var c = (BoardPosition)customType;
        return new byte[] { (byte)c.x, (byte)c.y };
    }
}