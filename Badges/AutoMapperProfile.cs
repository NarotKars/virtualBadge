using AutoMapper;
using Badges.Models;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Badges
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<APILogin, User>();
            CreateMap<APIRegister, User>();
            CreateMap<APIRequest, Request>();
        }
    }
}
