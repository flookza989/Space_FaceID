using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewFaceCore.Model;

namespace Space_FaceID.Models.Common
{
    public class FaceDetectionResult
    {
        public int TotalFaces { get; set; }
        public FaceInfo[]? Faces { get; set; }
    }
}
