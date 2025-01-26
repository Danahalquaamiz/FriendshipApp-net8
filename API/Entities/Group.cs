using System;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group
{
    [Key]
    public required string Name { get; set; } // force Name to be the Primary key.
    public ICollection<Connection> Connections { get; set; } = [];
    
}
