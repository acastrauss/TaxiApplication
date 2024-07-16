using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Models.Blob
{
    [DataContract]
    [KnownType(typeof(Stream))]
    public class BlobUploadData
    {
        [DataMember]
        public string BlobName { get; set; }
        [DataMember]
        public Stream BlobStream { get; set; } 
    }
}
