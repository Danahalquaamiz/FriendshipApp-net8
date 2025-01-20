using System;
using API.Extensions;

namespace API.Entities;

public class AppUser
{
        public int Id { get; set; }
        public required string UserName { get; set; }
        public byte[] PasswordHash { get; set; } = [];
        public byte[] PasswordSalt { get; set; } = [];
        public DateOnly DateOfBirth { get; set; }
        public required string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public required string Gender { get; set; }
        public string? Introduction { get; set; }
        public string? Interests { get; set; }
        public string? LookingFor { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public List<Photo> Photos { get; set; } = [];
        public List<UserLike> LikedByUsers { get; set; } = [];
         public List<UserLike> LikedUsers { get; set; } = [];
}


// The required keyword enforces that the property must be set during object creation. If you need more flexibility 
//to allow the UserName to be set later (e.g., after fetching data from a database or through an update API), removing required might be necessary.

//[] initializes the PasswordHash and PasswordSalt properties as empty byte arrays.
//This ensures that these properties are never null by default, eliminating the need for null checks when accessing them.
