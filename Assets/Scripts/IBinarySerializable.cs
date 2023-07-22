using System.IO;

public interface IBinarySerializable
{
    /// <summary>
    /// Serialize the object's data into binary format.
    /// </summary>

    void Serialize(BinaryWriter writer);

    /// <summary>
    /// Deserialize the object's data from binary format.
    /// </summary>

    void Deserialize(BinaryReader reader);
}