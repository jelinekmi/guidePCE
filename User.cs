using System;
using SQLite;

namespace Bakalarka_Jelinek
{
    [Table("users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull, Unique]
        public string Email { get; set; }

        [NotNull]
        public string Password { get; set; }

        [NotNull]
        public string Role { get; set; }

        public string ProfilePhotoPath { get; set; }

        
        [Ignore]
        public static User? CurrentUser { get; set; }

        [Ignore]
        public static bool IsLoggedIn { get; internal set; }
    }
}
