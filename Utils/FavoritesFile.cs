

using System.Collections.Generic;
using System.IO;

namespace Launcher.Utils
{
  internal class FavoritesFile
  {
    private static string mFavoritesFilename = "fav.io";

    public static void AddServer(string serverAddress)
    {
      try
      {
        using (FileStream fileStream = new FileStream(FavoritesFile.mFavoritesFilename, FileMode.Append))
        {
          using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
            binaryWriter.Write(serverAddress);
        }
      }
      catch
      {
      }
    }

    public static void RemoveServer(string serverAddress)
    {
      try
      {
        string contents = File.ReadAllText(FavoritesFile.mFavoritesFilename).Replace('\x0012'.ToString() + serverAddress, "");
        File.WriteAllText(FavoritesFile.mFavoritesFilename, contents);
      }
      catch
      {
      }
    }

    public static string[] GetServerList()
    {
      List<string> stringList = new List<string>();
      try
      {
        using (FileStream fileStream = new FileStream(FavoritesFile.mFavoritesFilename, FileMode.OpenOrCreate))
        {
          using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
          {
            while (true)
              stringList.Add(binaryReader.ReadString());
          }
        }
      }
      catch
      {
      }
      return stringList.ToArray();
    }
  }
}
