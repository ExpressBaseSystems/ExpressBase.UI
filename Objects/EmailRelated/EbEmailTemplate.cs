﻿using ExpressBase.Common.JsonConverters;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Objects.EmailRelated
{
    public enum EmailPriority
    {
        High,
        Low,
        Medium
    }

    [EnableInBuilder(BuilderType.EmailBuilder)]
    public class EbEmailTemplateBase : EbObject
    {

    }

    [EnableInBuilder(BuilderType.EmailBuilder)]
    public class EbEmailTemplate : EbEmailTemplateBase
    {
        //[EnableInBuilder(BuilderType.EmailBuilder)]
        //public string Description { get; set; }

        [EnableInBuilder(BuilderType.EmailBuilder)]
        public EmailPriority Priority { get; set; }

        [EnableInBuilder(BuilderType.EmailBuilder)]
        public string Subject { get; set; }

        [EnableInBuilder(BuilderType.EmailBuilder)]
        [JsonConverter(typeof(Base64Converter))]
        [HideInPropertyGrid]
        public string Body { get; set; }

        [EnableInBuilder(BuilderType.EmailBuilder)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectType.DataSource)]
        public string DataSourceRefId { get; set; }

        [JsonIgnore]
        public EbDataSource EbDataSource { get; set; }

        public override void AfterRedisGet(RedisClient Redis)
        {
            try
            {
                this.EbDataSource = Redis.Get<EbDataSource>(this.DataSourceRefId);
                this.EbDataSource.AfterRedisGet(Redis);
            }
            catch (Exception e)
            {

            }
        }
    }
}
