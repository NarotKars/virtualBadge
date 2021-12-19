using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DataAccess
{
    public class Admins : IAdmins
    {
        public void UpdateBadgeQuantity(int quantity)
        {
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("UpdateParamQuantity", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@quantity", quantity);
            command.ExecuteNonQuery();
        }

        public void UpdateDayCount(int days)
        {
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("UpdateParamDays", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@days", days);
            command.ExecuteNonQuery();
        }

        public void AddBadgesToUser(string userId, int quantity)
        {
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("UpdateBadgeQuantityByUserId", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.ExecuteNonQuery();
        }
    }
}
