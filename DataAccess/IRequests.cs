using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public interface IRequests
    {
        public int CreateRequest(Request request);
        public void AcceptRequest(int id);
        public void DeclineRequest(int id);
        public List<RequestCredentials> GetMyRequests(string userId);
        public List<RequestCredentials> GetPendingRequests(string userId);
        public List<RequestCredentials> GetReceivedBadges(string userId);
    }
}
