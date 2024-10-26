using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace TgBot_YT2Audio
{
    public class StartArgsOptions
    {
        [Option('c', "config", Required = false, HelpText = "Set configuration file")]
        public string? Config { get; set; }
        [Option('d', "daemon", Required = false, HelpText = "Enable daemon mode for systemd")]
        public bool Daemon { get; set; }
    }
}
