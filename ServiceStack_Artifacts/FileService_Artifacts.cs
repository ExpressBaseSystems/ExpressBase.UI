﻿using ExpressBase.Common;
using MongoDB.Bson;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
    [DataContract]
    public class FileMeta
    {
        [DataMember(Order = 1)]
        public string ObjectId { get; set; }

        [DataMember(Order = 2)]
        public string FileName { get; set; }

        [DataMember(Order = 3)]
        public IDictionary<string, List<string>> MetaDataDictionary { get; set; }

        [DataMember(Order = 4)]
        public DateTime UploadDateTime{ get; set; }

        [DataMember(Order = 5)]
        public Int64 Length { get; set; }

        [DataMember(Order = 6)]
        public int ContentType { get; set; }
    }

    [DataContract]
    public class UploadFileRequest : EbServiceStackRequest, IReturn<string>
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public byte[] ByteArray { get; set; }

        [DataMember(Order = 3)]
        public string BucketName { get; set; }

        [DataMember(Order = 4)]
        public bool IsAsync { get; set; }

    }

    [DataContract]
    public class UploadFileMqRequest : EbServiceStackRequest
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public byte[] ByteArray { get; set; }

        [DataMember(Order = 3)]
        public string BucketName { get; set; }
    }

    [DataContract]
    public class UploadFileMqRequestTest : EbServiceStackRequest
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public byte[] ByteArray { get; set; }

        [DataMember(Order = 3)]
        public string BucketName { get; set; }
    }

    public class UploadFileControllerResponse
    {
        public string Uploaded { get; set; }

        public string initialPreview { get; set; }

        public string objId { get; set; }
    }

    public class UploadFileControllerError
    {
        public string Uploaded { get; set; }
    }

    [DataContract]
    public class DownloadFileRequest : EbServiceStackRequest
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public string BucketName { get; set; }
    }

    [DataContract]
    public class FindFilesByTagRequest : EbServiceStackRequest, IReturn<List<FileMeta>>
    {
        [DataMember(Order = 1)]
        public KeyValuePair<string, List<string>> Filter { get; set; }

        [DataMember(Order = 2)]
        public string BucketName { get; set; }
    }

    [DataContract]
    public class ImageResizeMqRequest : EbServiceStackRequest
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public string BucketName { get; set; }

        [DataMember(Order = 3)]
        public byte[] ImageByte { get; set; }
    }

    [DataContract]
    public class ImageResizeMqRequestTest : EbServiceStackRequest
    {
        [DataMember(Order = 1)]
        public FileMeta FileDetails { get; set; }

        [DataMember(Order = 2)]
        public string BucketName { get; set; }

        [DataMember(Order = 3)]
        public byte[] ImageByte { get; set; }

    }
}
