using System.IO;
using Tommy;

namespace Horizon
{
    public class Config
    {
        public Config()
        {
            using StreamReader sr = File.OpenText("config.toml");
            TomlTable table = TOML.Parse(sr);
            Token = table["token"];
            Prefix = table["prefix"];
        }

        public string Token { get; set; }
        public string Prefix { get; set; }
    }
}