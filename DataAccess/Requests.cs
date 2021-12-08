using AutoMapper;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Requests : IRequests
    {
        private readonly IMapper mapper;

        public Requests(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public int CreateRequest(Request request)
        {
            try
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

            catch (SqlException ex)
            {
                string errorMessage = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessage += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessage);
            }
        }

        public void AcceptRequest(int id)
        {
            UpdateRequestStatus(id, Status.Accepted);
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("UpdateBadgeQuantity", connection);
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();
                command.Parameters.AddWithValue("@requestId", id);
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                string errorMessage = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessage += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessage);
            }
        }

        public void DeclineRequest(int id)
        {
            UpdateRequestStatus(id, Status.Declined);
        }

        private void UpdateRequestStatus(int id, Status status)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("UpdateRequestStatus", connection);
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();
                command.Parameters.AddWithValue("@requestId", id);
                command.Parameters.AddWithValue("@status", status);
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                string errorMessage = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessage += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessage);
            }
        }

        public List<RequestCredentials> GetReceivedBadges(string userId)
        {
            List<RequestCredentials> receivedBadges;
            try
            {
                receivedBadges = GetRequests(Status.Accepted, userId);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return receivedBadges;
        }

        public List<RequestCredentials> GetMyRequests(string userId)
        {
            List<RequestCredentials> myRequests;
            try
            {
                myRequests = GetRequests(Status.Pending, null, userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return myRequests;
        }

        public List<RequestCredentials> GetPendingRequests(string userId)
        {
            List<RequestCredentials> pendingRequests;
            try
            {
                pendingRequests = GetRequests(Status.Pending, userId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return pendingRequests;
        }

        private List<RequestCredentials> GetRequests(Status status, string requesterId = null, string giverId = null)
        {
            List<RequestCredentials> requests = new List<RequestCredentials>();
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionManager.ConnectionString);
                using SqlCommand command = new SqlCommand("GetRequests", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@status", status);
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
                    while (dataReader.Read())
                    {
                        Request request = new Request();
                        request.Id = dataReader.GetInt32(colRequestId);
                        request.RequesterId = dataReader.GetString(colRequesterId);
                        request.GiverId = dataReader.GetString(colGiverId);
                        request.Quantity = dataReader.GetInt32(colQuantity);
                        requests.Add(new RequestCredentials
                        {
                            Request = request,
                            RequesterUserName = dataReader.GetString(colRequesterUserName),
                            GiverUserName = dataReader.GetString(colGiverUserName)
                        }) ;
                    }
                }
            }
            catch (SqlException ex)
            {
                string errorMessages = string.Empty;
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages += $"Message: {ex.Errors[i].Message}\n";
                }
                throw new Exception(errorMessages);
            }

            return requests;
        }
    }
}
