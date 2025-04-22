using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Models.Common
{
    public class VerificationResult
    {
        public bool IsVerified { get; set; }
        public float Similarity { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
