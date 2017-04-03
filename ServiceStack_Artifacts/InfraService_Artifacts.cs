﻿using ServiceStack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
    [DataContract]
    public class InfraDb_GENERIC_SELECTRequest : IEbSSRequest
    {
        [DataMember(Order = 1)]
        public string Token { get; set; }

        [DataMember(Order = 2)]
        public string TenantAccountId { get; set; }

        [DataMember(Order = 3)]
        public string InfraDbSqlQueryKey { get; set; }

        [DataMember(Order = 4)]
        public Dictionary<string, object> Parameters { get; set; }
    }

    [DataContract]
    public class InfraDb_GENERIC_SELECTResponse
    {
        [DataMember(Order = 1)]
        public List<Dictionary<string, object>> Data { get; set; }
    }

    [DataContract]
    [Route("/infra", "POST")]
    public class InfraRequest : IReturn<InfraResponse>,IEbSSRequest
    {
        [DataMember(Order = 0)]
        public Dictionary<string, object> Colvalues { get; set; }

        [DataMember(Order = 1)]
        public string ltype { get; set; }

        [DataMember(Order = 2)]
        public string Token { get; set; }

        public string TenantAccountId { get; set; }
    }

    [DataContract]
    public class InfraResponse
    {
        [DataMember(Order = 1)]
        public int id { get; set; }
    }

    [DataContract]
    [Route("/unc", "POST")]
    public class UnRequest : IReturn<bool>,IEbSSRequest
    {
        [DataMember(Order = 0)]
        public Dictionary<string, object> Colvalues { get; set; }

        [DataMember(Order = 1)]
        public string Token { get; set; }

        public string TenantAccountId { get; set; }


    }

    [DataContract]
    public class DbCheckRequest : IReturn<bool>,IEbSSRequest
    {
        [DataMember(Order = 0)]
        public Dictionary<string, object> DBColvalues { get; set; }

        [DataMember(Order = 1)]
        public int CId { get; set; }

        [DataMember(Order = 2)]
        public string Token { get; set; }

        public string TenantAccountId { get; set; }
    }

    [DataContract]
    public class AccountRequest : IReturn<AccountResponse>,IEbSSRequest
    {
        [DataMember(Order = 0)]
        public Dictionary<string, object> Colvalues { get; set; }

        [DataMember(Order = 1)]
        public string op { get; set; }

        [DataMember(Order = 2)]
        public int Id { get; set; }

        [DataMember(Order = 3)]
        public string Token { get; set; }

        [DataMember(Order = 4)]
        public int TId { get; set; }

        public string TenantAccountId { get; set; }
    }

    [DataContract]
    public class AccountResponse
    {
        [DataMember(Order = 1)]
        public int id { get; set; }
    }

    [DataContract]
    public class GetAccount : IReturn<GetAccountResponse>, IEbSSRequest
    {
        [DataMember(Order = 0)]
        public int Uid { get; set; }

        [DataMember(Order = 1)]
        public string restype { get; set; }

        [DataMember(Order = 2)]
        public string Token { get; set; }

        [DataMember(Order = 3)]
        public string Uname { get; set; }

        public string TenantAccountId { get; set; }
    }

    [DataContract]
    public class GetAccountResponse
    {
       
        [DataMember(Order = 1)]
        public List<List<object>> returnlist { get; set; }


    }

    [DataContract]
    public class SendMail : IReturn<bool>
    {
        [DataMember(Order = 0)]
        public Dictionary<string, object> Emailvals { get; set; }
    }
}