using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Photos")] // i want the table named: photos not photo.
public class Photo
{
       public int Id { get; set; } 
       public required string Url { get; set; }
       public bool IsMain { get; set; }
       public string? PublicId { get; set; }
       public bool IsApproved { get; set; } = false;
       
       // Navigation Proprties : Requried one-to-many relationship.
       public int AppUserID { get; set; }
       public AppUser AppUser { get; set; } = null!;
}


// The required keyword enforces that the property must be set during object creation. If you need more flexibility 
//to allow the UserName to be set later (e.g., after fetching data from a database or through an update API), removing required might be necessary.

//[] initializes the PasswordHash and PasswordSalt properties as empty byte arrays.
//This ensures that these properties are never null by default, eliminating the need for null checks when accessing them.
