using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeBoard
{
    public class Meme
    {
        public string Path { get; private set; }
        public string Prefix { get; private set; }
        public bool IsAnimated { get; private set; }

        public Meme(string file)
        {
            var fileInfo = new FileInfo(file);

            this.Path = fileInfo.FullName;
            this.IsAnimated = fileInfo.Extension.ToLower() == ".gif";
            this.Prefix = fileInfo.Name.Split('_').First();
        }
    }
}
