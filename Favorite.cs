using SQLite;

namespace Bakalarka_Jelinek
{
    [Table("Favorites")]
    public class Favorite
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [NotNull, Indexed]
        public string UserEmail { get; set; }

        [NotNull]
        public string Nazev { get; set; }  

        [NotNull]
        public string Typ { get; set; }

        [NotNull]
        public double Lat { get; set; }

        [NotNull]
        public double Lon { get; set; }
    }
}
