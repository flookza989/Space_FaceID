using Space_FaceID.Models.Entities;
using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class RoleService(IRoleRepository roleRepository) : GenericService<Role>(roleRepository), IRoleService
    {
        private readonly IRoleRepository _roleRepository = roleRepository;

        public async Task<List<Role>> GetRolesWithoutAdminAsync()
        {
            return await _roleRepository.GetRolesWithoutAdminAsync();
        }
    }
}
