using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess
{
    public class Requests : IRequests
    {
        public Requests() { }

        public int CreateRequest(Request request)
        {
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("CreateRequest", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@requesterId", request.RequesterId);
            command.Parameters.AddWithValue("@giverId", request.GiverId);
            command.Parameters.AddWithValue("@quantity", request.Quantity);
            SqlParameter newRequestId = command.CreateParameter();
            newRequestId.ParameterName = "@id";
            newRequestId.Direction = ParameterDirection.Output;
            newRequestId.DbType = DbType.Int32;
            command.Parameters.Add(newRequestId);
            command.ExecuteNonQuery();
            return (int)newRequestId.Value;
        }

        public void AcceptRequest(int id)
        {
            UpdateRequestStatus(id, Status.Accepted);
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("[UpdateBadgeQuantityByRequestId]", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@requestId", id);
            command.ExecuteNonQuery();
        }

        public void DeclineRequest(int id)
        {
            UpdateRequestStatus(id, Status.Declined);
        }

        private void UpdateRequestStatus(int id, Status status)
        {
            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("UpdateRequestStatus", connection);
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            command.Parameters.AddWithValue("@requestId", id);
            command.Parameters.AddWithValue("@status", status);
            command.ExecuteNonQuery();
        }

        public List<RequestCredentials> GetReceivedBadges(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("user id is empty");

            return GetRequests(userId);
        }

        public List<RequestCredentials> GetMyRequests(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("user id is empty");

            return GetRequests(null, userId);
        }

        private List<RequestCredentials> GetRequests(string requesterId = null, string giverId = null)
        {
            List<RequestCredentials> requests = new List<RequestCredentials>();

            using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
            using SqlCommand command = new SqlCommand("GetRequests", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@requesterId", requesterId);
            command.Parameters.AddWithValue("@giverId", giverId);
            connection.Open();
            using SqlDataReader dataReader = command.ExecuteReader();

            if (dataReader.HasRows)
            {
                int colRequesterId = dataReader.GetOrdinal("requesterId");
                int colGiverId = dataReader.GetOrdinal("giverId");
                int colRequestId = dataReader.GetOrdinal("requestId");
                int colGiverUserName = dataReader.GetOrdinal("giverUserName");
                int colRequesterUserName = dataReader.GetOrdinal("requesterUserName");
                int colQuantity = dataReader.GetOrdinal("quantity");
                int colStatus = dataReader.GetOrdinal("status");
                while (dataReader.Read())
                {
                    Request request = new Request();
                    request.Id = dataReader.GetInt32(colRequestId);
                    request.RequesterId = dataReader.GetString(colRequesterId);
                    request.GiverId = dataReader.GetString(colGiverId);
                    request.Quantity = dataReader.GetInt32(colQuantity);
                    request.Status = dataReader.GetInt32(colStatus);
                    requests.Add(new RequestCredentials
                    {
                        Request = request,
                        RequesterUserName = dataReader.GetString(colRequesterUserName),
                        GiverUserName = dataReader.GetString(colGiverUserName)
                    }) ;
                }
            }

            return requests;
        }
    }
}
