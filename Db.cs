using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using BCrypt.Net;

namespace Bakalarka_Jelinek
{
    public class Db
    {
        private readonly SQLiteAsyncConnection _connection;

        public Db()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mydatabase.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            _ = InitializeDatabaseAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            
            try
            {
                var info = await _connection.GetTableInfoAsync("Favorites");

                if (info.Any() && (!info.Any(c => c.Name == "Lat") || !info.Any(c => c.Name == "Lon")))
                {
                    await _connection.ExecuteAsync("DROP TABLE IF EXISTS Favorites");
                }
            }
            catch
            {
                
            }

            
            await _connection.CreateTableAsync<User>();
            await _connection.CreateTableAsync<Favorite>();
        
        }

        public async Task AddUserAsync(User user)
        {
            await _connection.InsertAsync(user);
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            var user = await _connection.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            var user = await _connection.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            return user != null;
        }

        public async Task UpdateUserPasswordAsync(string email, string newHashedPassword)
        {
            var user = await _connection.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user != null)
            {
                user.Password = newHashedPassword;
                await _connection.UpdateAsync(user);
            }
        }

        public async Task UpdateUserPhotoAsync(string email, string photoPath)
        {
            var user = await _connection.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user != null)
            {
                user.ProfilePhotoPath = photoPath;
                await _connection.UpdateAsync(user);
            }
        }

        public async Task AddFavoriteAsync(string email, string nazev, string typ, double lat, double lon)
        {
            var exists = await _connection.Table<Favorite>()
                .Where(f => f.UserEmail == email && f.Nazev == nazev && f.Typ == typ && f.Lat == lat && f.Lon == lon)
                .FirstOrDefaultAsync();

            if (exists == null)
            {
                var favorite = new Favorite
                {
                    UserEmail = email,
                    Nazev = nazev,
                    Typ = typ,
                    Lat = lat,
                    Lon = lon
                };
                await _connection.InsertAsync(favorite);
            }
        }

        public async Task RemoveFavoriteAsync(string email, string nazev, string typ, double lat, double lon)
        {
            var favorite = await _connection.Table<Favorite>()
                .Where(f => f.UserEmail == email && f.Nazev == nazev && f.Typ == typ && f.Lat == lat && f.Lon == lon)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                await _connection.DeleteAsync(favorite);
            }
        }

        public async Task<bool> IsFavoriteAsync(string email, string nazev, string typ, double lat, double lon)
        {
            var favorite = await _connection.Table<Favorite>()
                .Where(f => f.UserEmail == email && f.Nazev == nazev && f.Typ == typ && f.Lat == lat && f.Lon == lon)
                .FirstOrDefaultAsync();

            return favorite != null;
        }

        public async Task<List<Mapa.Pamatka>> GetFavoritesByTypeAsync(string email, string typ)
        {
            var favorites = await _connection.Table<Favorite>()
                .Where(f => f.UserEmail == email && f.Typ == typ)
                .ToListAsync();

            List<Mapa.Pamatka> vsechny = typ switch
            {
                "pamatka" => await Mapa.ZiskejPamatkyVPardubicichAsync(),
                "restaurace" => await Mapa.ZiskejRestauraceVPardubicichAsync(),
                "obchod" => await Mapa.ZiskejObchodyVPardubicichAsync(),
                "bar" => await Mapa.ZiskejBaryVPardubicichAsync(),
                _ => new List<Mapa.Pamatka>()
            };

            return vsechny
                .Where(p => favorites.Any(f => f.Nazev == p.Nazev && f.Lat == p.Lat && f.Lon == p.Lon))
                .ToList();
        }
    }
}