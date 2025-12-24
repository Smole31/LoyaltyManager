using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace LoyaltyManager
{
    public static class DatabaseHelper
    {
        private static string dbPath = "LoyaltyDB.db";
        public static string ConnectionString => $"Data Source={dbPath};";

        public static void Initialize()
        {
            if (!File.Exists(dbPath))
            {
                CreateDatabase();
            }
        }

        private static void CreateDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                string sql = @"
CREATE TABLE users (
    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL,
    full_name TEXT NOT NULL,
    phone TEXT UNIQUE NOT NULL,
    email TEXT,
    points INTEGER DEFAULT 0,
    registration_date TEXT DEFAULT (datetime('now','localtime')),
    is_active INTEGER DEFAULT 1
);

CREATE TABLE products (
    product_id INTEGER PRIMARY KEY AUTOINCREMENT,
    product_name TEXT NOT NULL,
    price REAL NOT NULL,
    points_per_unit INTEGER DEFAULT 10,
    stock_quantity INTEGER DEFAULT 0
);

CREATE TABLE purchases (
    purchase_id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    total_price REAL NOT NULL,
    points_earned INTEGER NOT NULL,
    purchase_date TEXT DEFAULT (datetime('now','localtime')),
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

INSERT INTO users (username, password, full_name, phone, points) VALUES
('admin', '1234', 'Адміністратор', '+380501111111', 0),
('user1', '1234', 'Іван Петренко', '+380502222222', 150),
('user2', '1234', 'Марія Коваленко', '+380503333333', 250);

INSERT INTO products (product_name, price, points_per_unit, stock_quantity) VALUES
('Молоко 1л', 35.50, 4, 100),
('Хліб білий', 18.00, 2, 200),
('Яблука 1кг', 45.00, 5, 150),
('Сік 1л', 55.00, 6, 80),
('Сир 200г', 85.00, 9, 60);
";

                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        public static User? Login(string username, string password)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT user_id, username, full_name, phone, email, points 
                    FROM users 
                    WHERE username = @username AND password = @password AND is_active = 1";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            Phone = reader.GetString(3),
                            Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Points = reader.GetInt32(5)
                        };
                    }
                }
            }
            return null;
        }

        public static void AddPoints(int userId, int points)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "UPDATE users SET points = points + @points WHERE user_id = @id";
                cmd.Parameters.AddWithValue("@points", points);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static int GetUserPoints(int userId)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT points FROM users WHERE user_id = @id";
                cmd.Parameters.AddWithValue("@id", userId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int Points { get; set; }
    }
}