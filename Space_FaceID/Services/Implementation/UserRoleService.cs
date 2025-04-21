using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class UserRoleService : GenericService<UserRole>, IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository)
            : base(userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }
    }
}
