using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
namespace WebServer
{
    public class DbContext
    {   
        private const string DbConnectionString = "Host=localhost;Port=5555;Username=postgres;Password=20242424; Database=Dayli";
        //там было private readonly
        public NpgsqlConnection _dbConnection= new NpgsqlConnection(DbConnectionString);
        
        public async Task<User> CreateUser(string login, string password, CancellationToken cancellationToken = default)
        {

            await _dbConnection.OpenAsync();
            const string sqlQuery = "INSERT INTO users (login, password) VALUES (@login, @password) RETURNING id";
            var cmd = new NpgsqlCommand(sqlQuery, _dbConnection);
            cmd.Parameters.AddWithValue("login", login);
            cmd.Parameters.AddWithValue("password", password);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            Console.WriteLine("пупупу");
            return new User
            {
                Login = login,
                Password = password,
                Id = Convert.ToInt32(result)
            };
            await _dbConnection.CloseAsync();
            
        }

        /*public async Task<User> GetUser(string login,CancellationToken cancellationToken=default)
        {
            await _dbConnection.OpenAsync(cancellationToken);
            try
            {
                const string sqlQuery= "SELECT * FROM users WHERE login =@login and password=@password";
                var cmd = new NpgsqlCommand(sqlQuery, _dbConnection);
                cmd.Parameters.AddWithValue("login", login);
                var reader = await cmd.ExecuteReaderAsync(cancellationToken);//это метод для выполнения SQL-запроса, который возвращает полный набор результатов
                if (reader.HasRows && await reader.ReadAsync(cancellationToken))
                {
                    return new User
                    {
                        Id = reader.GetInt64("id"),
                        Password = reader.GetString("password"),
                        Role = reader.GetString("role"),
                    };
                }
            }
            finally
            {
                await _dbConnection.CloseAsync();
            }

            return null;


        }*/
    }
}
