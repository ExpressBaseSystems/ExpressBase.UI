﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.EbServiceStack;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.ServiceClients;
using Newtonsoft.Json;
using Npgsql;
using RestSharp;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
    public class EbBaseService : Service
    {
        private EbConnectionFactory __EbConnectionFactory;
        public EbConnectionFactory EbConnectionFactory
        {
            get { return __EbConnectionFactory; }
            set
            {
                if (value != null)
                    __EbConnectionFactory = value;
            }
        }

        protected RestClient RestClient { get; private set; }

        protected RabbitMqProducer MessageProducer3 { get; private set; }

        protected RabbitMqQueueClient MessageQueueClient { get; private set; }

        private EbConnectionFactory _infraConnectionFactory = null;

        protected EbServerEventClient ServerEventClient { get; private set; }

        protected EbMqClient MQClient { get; private set; }

        protected EbStaticFileClient FileClient { get; private set; }

        protected EbConnectionFactory InfraConnectionFactory
        {
            get
            {
                if (_infraConnectionFactory == null)
                    _infraConnectionFactory = new EbConnectionFactory(CoreConstants.EXPRESSBASE, this.Redis);

                return _infraConnectionFactory;
            }
        }

        //protected RedisServerEvents ServerEvents { get; private set; }

        public EbBaseService() { }

        public EbBaseService(IEbConnectionFactory _dbf)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
        }

        public EbBaseService(IMessageProducer _mqp)
        {
            this.MessageProducer3 = _mqp as RabbitMqProducer;
        }

        public EbBaseService(RestSharp.IRestClient _rest)
        {
            this.RestClient = _rest as RestClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IEbStaticFileClient _sfc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.FileClient = _sfc as EbStaticFileClient;

        }

        public EbBaseService(IEbConnectionFactory _dbf, IEbServerEventClient _sec)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.ServerEventClient = _sec as EbServerEventClient;
        }

        public EbBaseService(IMessageProducer _mqp, IMessageQueueClient _mqc)
        {
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
        }

        public EbBaseService(IMessageProducer _mqp, IMessageQueueClient _mqc, IEbServerEventClient _se)
        {
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
            this.ServerEventClient = _se as EbServerEventClient;
        }

        public EbBaseService(IEbMqClient _mq, IEbServerEventClient _se)
        {
            this.MQClient = _mq as EbMqClient;
            this.ServerEventClient = _se as EbServerEventClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IMessageProducer _mqp)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IEbMqClient _mq)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MQClient = _mq as EbMqClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IEbMqClient _mqc, IEbServerEventClient _sec)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MQClient = _mqc as EbMqClient;
            this.ServerEventClient = _sec as EbServerEventClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IMessageProducer _mqp, IMessageQueueClient _mqc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IMessageProducer _mqp, IMessageQueueClient _mqc, IEbServerEventClient _sec)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
            this.ServerEventClient = _sec as EbServerEventClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IMessageProducer _mqp, IMessageQueueClient _mqc, IEbMqClient _mq)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
            this.MQClient = _mq as EbMqClient;
        }

        public EbBaseService(IEbConnectionFactory _dbf, IEbStaticFileClient _sfc, IMessageProducer _mqp, IMessageQueueClient _mqc)
        {
            this.EbConnectionFactory = _dbf as EbConnectionFactory;
            this.FileClient = _sfc as EbStaticFileClient;
            this.MessageProducer3 = _mqp as RabbitMqProducer;
            this.MessageQueueClient = _mqc as RabbitMqQueueClient;
        }

        private static Dictionary<string, string> _infraDbSqlQueries;

        public static Dictionary<string, string> InfraDbSqlQueries
        {
            get
            {
                if (_infraDbSqlQueries == null)
                {
                    _infraDbSqlQueries = new Dictionary<string, string>();
                    _infraDbSqlQueries.Add("KEY1", "SELECT id, accountname, profilelogo FROM eb_tenantaccount WHERE tenantid=@tenantid");
                }

                return _infraDbSqlQueries;
            }
        }

        public ILog Log { get { return LogManager.GetLogger(GetType()); } }

        public bool InsertExecutionLog(string rows, TimeSpan t, DateTime starttime, int userid, List<Param> param, string refid)
        {
            string start = starttime.ToString("yyyy-MM-dd hh:mm:ss");        
            string _params = EbSerializers.Json_Serialize(param); 
            try
            {
                string query = @"INSERT INTO executionlogs(rows, exec_time, created_by, created_at, params, refid) 
                                VALUES(" + "'" + rows + "'" + "," + t.TotalMilliseconds + "," + userid + ",'" +
                                start + "'," + "'" + _params + "'"+ ",'"+ refid+"'" + ")";
                this.EbConnectionFactory.ObjectsDB.DoNonQuery(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }

        //private void LoadCache()
        //{
        //    using (var redisClient = this.Redis)
        //    {
        //        if (!string.IsNullOrEmpty(this.ClientID))
        //        {
        //            EbTableCollection tcol = redisClient.Get<EbTableCollection>(string.Format("EbTableCollection_{0}",this.ClientID));
        //            EbTableColumnCollection ccol = redisClient.Get<EbTableColumnCollection>(string.Format("EbTableColumnCollection_{0}", this.ClientID));
        //            if (tcol == null || ccol == null)
        //            {
        //                tcol = new EbTableCollection();
        //                ccol = new EbTableColumnCollection();
        //                string sql = "SELECT id,tablename FROM eb_tables;" + "SELECT id,columnname,columntype,table_id FROM eb_tablecolumns;";
        //                var dt1 = this.DatabaseFactory.ObjectsDB.DoQueries(sql);
        //                foreach (EbDataRow dr in dt1.Tables[0].Rows)
        //                {
        //                    EbTable ebt = new EbTable
        //                    {
        //                        Id = Convert.ToInt32(dr[0]),
        //                        Name = dr[1].ToString()
        //                    };

        //                    tcol.Add(ebt.Id, ebt);
        //                }

        //                foreach (EbDataRow dr1 in dt1.Tables[1].Rows)
        //                {
        //                    EbTableColumn ebtc = new EbTableColumn
        //                    {
        //                        Type = (DbType)(dr1[2]),
        //                        Id = Convert.ToInt32(dr1[0]),
        //                        Name = dr1[1].ToString(),
        //                        TableId = Convert.ToInt32(dr1[3])
        //                    };
        //                    ccol.Add(ebtc.Name, ebtc);

        //                }

        //                redisClient.Set<EbTableCollection>(string.Format("EbTableCollection_{0}", this.ClientID), tcol);
        //                redisClient.Set<EbTableColumnCollection>(string.Format("EbTableColumnCollection_{0}", this.ClientID), ccol);
        //            }
        //        }
        //        else
        //        {
        //            EbTableCollection tcol = redisClient.Get<EbTableCollection>("EbInfraTableCollection");
        //            EbTableColumnCollection ccol = redisClient.Get<EbTableColumnCollection>("EbInfraTableColumnCollection");

        //            if (tcol == null || ccol == null)
        //            {
        //                tcol = new EbTableCollection();
        //                ccol = new EbTableColumnCollection();

        //                string sql = "SELECT id,tablename FROM eb_tables;" + "SELECT id,columnname,columntype FROM eb_tablecolumns;";
        //                var dt1 = this.DatabaseFactory.ObjectsDB.DoQueries(sql);

        //                foreach (EbDataRow dr in dt1.Tables[0].Rows)
        //                {
        //                    EbTable ebt = new EbTable
        //                    {
        //                        Id = Convert.ToInt32(dr[0]),
        //                        Name = dr[1].ToString()
        //                    };

        //                    tcol.Add(ebt.Id, ebt);
        //                }

        //                foreach (EbDataRow dr1 in dt1.Tables[1].Rows)
        //                {
        //                    EbTableColumn ebtc = new EbTableColumn
        //                    {
        //                        Type = (DbType)(dr1[2]),
        //                        Id = Convert.ToInt32(dr1[0]),
        //                        Name = dr1[1].ToString(),
        //                    };
        //                   ccol.Add(ebtc.Name, ebtc);

        //                }

        //                redisClient.Set<EbTableCollection>("EbInfraTableCollection", tcol);
        //                redisClient.Set<EbTableColumnCollection>("EbInfraTableColumnCollection", ccol);
        //            }
        //        }
        //    }
        //}
    }
}
