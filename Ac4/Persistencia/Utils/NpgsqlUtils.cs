using Ac4.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ac4.Persistencia.Utils
{
    public class NpgsqlUtils
    {
        public static NpgsqlConnection OpenConnection()
        {
            // Carregar la cadena de connexió a la base de dades des de l'arxiu de configuració
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(@"config.json", optional: false, reloadOnChange: true)
                .Build();

            var s = config.GetConnectionString("MyPostgresConn");
            NpgsqlConnection conn = new(s);
            conn.Open();
            return conn;
        }

    }
}
