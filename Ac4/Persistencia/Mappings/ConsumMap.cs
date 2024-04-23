using Ac4.DTOs;
using Ac4.Persistencia.DAOs;
using Ac4.Persistencia.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ac4.Persistencia.Mappings
{
    public class ConsumMap : IConsumDAO
    {
        public List<ConsumDTO> GetAllRecords()
        {
            List<ConsumDTO> cons = new List<ConsumDTO>();
            using (var conn = NpgsqlUtils.OpenConnection())
            {
                var query = "SELECT * FROM Records";
                NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ConsumDTO newCons = new ConsumDTO();
                    newCons.Any = reader.GetInt32(1);
                    newCons.CodComarca = reader.GetInt32(2);
                    newCons.Comarca = reader.GetString(3);
                    newCons.Poblacio = reader.GetInt32(4);
                    newCons.XarxaDomestica = reader.GetInt32(5);
                    newCons.ActivitatsEconomiques = reader.GetInt32(6);
                    newCons.Total = reader.GetInt32(7);
                    newCons.ConsumPerCapita = reader.GetInt32(8);
                    cons.Add(newCons);
                }
            }
            return cons;
        }

        public void InsertRecord(ConsumDTO record)
        {
            using (var conn = NpgsqlUtils.OpenConnection())
            {
                var query = "INSERT INTO Records (Year, CodiComarca, Comarca, Poblacio, DomesticXarxa, ActividadesEconomicas, Total, ConsumoCapita) VALUES " +
                    "(@any, @codcom, @com, @pob, @domxar, @acteco, @total, @concap)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("any", record.Any);
                cmd.Parameters.AddWithValue("codcom", record.CodComarca);
                cmd.Parameters.AddWithValue("com", record.Comarca);
                cmd.Parameters.AddWithValue("pob", record.Poblacio);
                cmd.Parameters.AddWithValue("domxar", record.XarxaDomestica);
                cmd.Parameters.AddWithValue("acteco", record.ActivitatsEconomiques);
                cmd.Parameters.AddWithValue("total", record.Total);
                cmd.Parameters.AddWithValue("concap", record.ConsumPerCapita);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
