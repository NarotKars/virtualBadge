using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public interface IAdmins
    {
        public void UpdateBadgeQuantity(int quantity);
        public void UpdateDayCount(int days);
        public void AddBadgesToUser(string userId, int quantity);
    }
}
