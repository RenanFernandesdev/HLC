using System.IO;

namespace HLC.Util
{
    internal class DirectoryUtilities
    {
        public static DirectoryUtilities GetInstance = new DirectoryUtilities();
        public string CreateDirectory()
        {
            string path = Directory.GetCurrentDirectory() + @"\BackData";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
