using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.Entities
{
    public enum ServerState
    {
        Offline,
        Online
    }

    public class Server
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string IpAddress { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public ServerState State { get; set; }
    }
}
