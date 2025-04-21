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
    public class FaceAuthenticationSettingService : GenericService<FaceAuthenticationSetting>, IFaceAuthenticationSettingService
    {
        private readonly IFaceAuthenticationSettingRepository _faceAuthenticationSettingRepository;

        public FaceAuthenticationSettingService(IFaceAuthenticationSettingRepository faceAuthenticationSettingRepository)
            : base(faceAuthenticationSettingRepository)
        {
            _faceAuthenticationSettingRepository = faceAuthenticationSettingRepository;
        }
    }
}
