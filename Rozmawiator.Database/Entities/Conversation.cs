﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rozmawiator.Database.Entities
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual ICollection<User> Participants { get; set; }
        public virtual ICollection<Call> Calls { get; set; }
        
    }
}
