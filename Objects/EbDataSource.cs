﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.JsonConverters;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.ServiceStack_Artifacts;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    public abstract class EbDataSourceMain : EbObject
    {
        [EnableInBuilder(BuilderType.DataReader, BuilderType.DataWriter, BuilderType.SqlFunctions)]
        [HideInPropertyGrid]
        [JsonConverter(typeof(Base64Converter))]
        public string Sql { get; set; }

        [EnableInBuilder(BuilderType.DataReader, BuilderType.DataWriter, BuilderType.SqlFunctions)]
        [HideInPropertyGrid]
        public List<Param> InputParams { get; set; }
    }

    [BuilderTypeEnum(BuilderType.DataReader)]
    [EnableInBuilder(BuilderType.DataReader)]
    public class EbDataReader : EbDataSourceMain,IEBRootObject
    {
        [EnableInBuilder(BuilderType.DataReader)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iFilterDialog)]
        public string FilterDialogRefId { get; set; }

        //public string VersionNumber { get; set; }

        //public string Status { get; set; }

        [JsonIgnore]
        public EbFilterDialog FilterDialog { get; set; }

        public string SqlDecoded()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(this.Sql));
        }

        public override void AfterRedisGet(RedisClient Redis)
        {
            try
            {
                this.FilterDialog = Redis.Get<EbFilterDialog>(this.FilterDialogRefId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.ToString());
            }
        }

        public override void AfterRedisGet(RedisClient Redis, IServiceClient client)
        {
            try
            {
                this.FilterDialog = Redis.Get<EbFilterDialog>(this.FilterDialogRefId);
                if (this.FilterDialog == null && this.FilterDialogRefId != "")
                {
                    var result = client.Get<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = this.FilterDialogRefId });
                    this.FilterDialog = EbSerializers.Json_Deserialize(result.Data[0].Json);
                    Redis.Set<EbFilterDialog>(this.FilterDialogRefId, this.FilterDialog);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + e.ToString());
            }
        }

        public override void ReplaceRefid(Dictionary<string, string> RefidMap)
        {
            if (!FilterDialogRefId.IsEmpty())
            {
                if (RefidMap.ContainsKey(FilterDialogRefId))
                    FilterDialogRefId = RefidMap[FilterDialogRefId];
                else
                    FilterDialogRefId = "failed-to-update-";
            }
        }

        public override string DiscoverRelatedRefids()
        {
            if (!FilterDialogRefId.IsEmpty())
            {
                EbFilterDialog fd = FilterDialog;
                if (fd is null)
                {
                    //  Console.WriteLine(this.RefId + "-->" + FilterDialogRefId);
                    return FilterDialogRefId;
                }
            }
            return "";
        }
    }

    [BuilderTypeEnum(BuilderType.DataWriter)]
    [EnableInBuilder(BuilderType.DataWriter)]
    public class EbDataWriter : EbDataSourceMain,IEBRootObject
    {

    }

    [EnableInBuilder(BuilderType.SqlFunctions)]
    [BuilderTypeEnum(BuilderType.SqlFunctions)]
    public class EbSqlFunction : EbDataSourceMain,IEBRootObject
    {
        [JsonIgnore]
        public WebFormSchema FormSchema { set; get; }

        [EnableInBuilder(BuilderType.SqlFunctions)]
        [HideInPropertyGrid]
        public string FunctionName {set { } get {
                return GetFuncNameByRegex();
            }
        }

        [JsonIgnore]
        private IEbConnectionFactory ConnectionFactory { get; set; }

        public EbSqlFunction() {

        }

        public EbSqlFunction(WebFormSchema data, IEbConnectionFactory con)
        {
            this.FormSchema = data;
            DisplayName = FormSchema.FormName + "_data_insert";
            Name = DisplayName;
            this.ConnectionFactory = con;
            this.Sql = this.GenSqlFunc();
        }

        private string GenSqlFunc()
        {
            StringBuilder qry = new StringBuilder();
            qry.AppendFormat(SqlConstants.SQL_FUNC_HEADER, this.FormSchema.FormName, "'plpgsql'");
            qry.AppendLine();
            qry.Append(@" DECLARE 
temp_table jsonb;
_row jsonb;
_master_id integer;
BEGIN ");
            for (int k = 0; k < this.FormSchema.Tables.Count; k++)
            {
                if (this.FormSchema.Tables[k].TableName == this.FormSchema.MasterTable)
                {
                    qry.AppendLine(GetQueryBlockInsert(this.FormSchema.Tables[k]));
                    qry.AppendFormat(@"SELECT 
    CURRVAL('{0}_id_seq') 
INTO 
    master_id;", this.FormSchema.MasterTable);
                    qry.AppendLine();
                    qry.AppendLine(GetQueryBlockUpdate(this.FormSchema.Tables[k]));
                    break;
                }
            }
            qry.AppendLine();
            for (int i = 0; i < this.FormSchema.Tables.Count; i++)
            {
                if (this.FormSchema.Tables[i].TableName == this.FormSchema.MasterTable)
                    continue;
                else
                {
                    qry.AppendLine();
                    qry.AppendLine(GetQueryBlockInsert(this.FormSchema.Tables[i]));
                    qry.AppendLine();
                    qry.AppendLine(GetQueryBlockUpdate(this.FormSchema.Tables[i]));
                }
            }
            qry.AppendLine("\r\n$BODY$");
            return qry.ToString();
        }

        private string GetQueryBlockInsert(TableSchema _schema)
        {
            return string.Format(@"SELECT
    _table->'Rows' 
FROM 
    jsonb_array_elements(insert_json) _table 
INTO 
    temp_table 
WHERE 
    _table->>'TableName' = '{0}';
FOR _row IN SELECT * FROM jsonb_array_elements(temp_table)
LOOP 
    {1} 
END LOOP;", _schema.TableName, GetExecuteQryI(_schema));
        }

        private string GetQueryBlockUpdate(TableSchema _schema)
        {
            return string.Format(@"SELECT
    _table->'Rows' 
FROM 
    jsonb_array_elements(update_json) _table 
INTO 
    temp_table 
WHERE 
    _table->>'TableName' = '{0}';
FOR _row IN SELECT * FROM jsonb_array_elements(temp_table)
LOOP 
    {1} 
END LOOP;", _schema.TableName, GetExecuteQryU(_schema));

        }

        private string GetExecuteQryI(TableSchema _schema)
        {
            string m_tablename=string.Empty, m_table_id = string.Empty;

            if (_schema.TableName != this.FormSchema.MasterTable)
            {
                m_tablename = this.FormSchema.MasterTable + "_id,";
                m_table_id = "master_id,";
            }

            string qry = "EXECUTE 'INSERT INTO " + _schema.TableName + "("+ m_tablename;
            string _using_clas = string.Empty;

            foreach (ColumnSchema col in _schema.Columns)
            {
                if (!col.Equals(_schema.Columns.Last()))
                {
                    qry = qry + col.ColumnName + CharConstants.COMMA;
                    _using_clas = _using_clas + "_row->>'" + col.ColumnName + "'" + "::" + this.GetVendorDbText(col.EbDbType) + CharConstants.COMMA;
                }
                else
                {
                    qry = qry + col.ColumnName + ") VALUES("+ m_table_id;
                    _using_clas = _using_clas + "_row->>'" + col.ColumnName + "'" + "::" + this.GetVendorDbText(col.EbDbType) + CharConstants.SEMI_COLON;
                }
            }

            for (int i = 1; i <= _schema.Columns.Count; i++)
            {
                if (i != _schema.Columns.Count)
                    qry = qry + "$" + i + CharConstants.COMMA;
                else
                    qry = qry + "$" + i + ")'";
            }
            qry = qry + " USING " + _using_clas;
            return qry;
        }

        private string GetExecuteQryU(TableSchema _schema)
        {
            string qry = string.Format("EXECUTE 'UPDATE {0} SET ", _schema.TableName);
            string _using_clas = string.Empty;
            int _counter = 0;
            foreach (ColumnSchema col in _schema.Columns)
            {
                _counter++;
                if (!col.Equals(_schema.Columns.Last()))
                {
                    qry = qry + col.ColumnName + CharConstants.EQUALS + "$" + _counter + CharConstants.COMMA;
                    _using_clas = _using_clas + "_row->>'" + col.ColumnName + "'" + "::" + this.GetVendorDbText(col.EbDbType) + CharConstants.COMMA;
                }
                else
                {
                    qry = qry + col.ColumnName + CharConstants.EQUALS + "$" + _counter + " WHERE id=$" + _counter + ";'";
                    _using_clas = _using_clas + "_row->>'" + col.ColumnName + "'" + "::" + this.GetVendorDbText(col.EbDbType) + CharConstants.SEMI_COLON;
                }
            }
            qry = qry + " USING " + _using_clas;
            return qry;
        }

        private string GetVendorDbText(int type)
        {
            return ConnectionFactory.DataDB.VendorDbTypes.GetVendorDbText((EbDbTypes)type);
        }

        private string GetFuncNameByRegex()
        {
            string _funcname = string.Empty;
            Regex r = new Regex(@"(\w+)(\s+|)\(.*?\)");
            if (!string.IsNullOrEmpty(this.Sql))
                _funcname = r.Match(this.Sql.Replace("\n", "")).Groups[1].Value;
            return _funcname;
        }
    }

    [ProtoBuf.ProtoContract]
    public class EbJsFunction : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string JsCode { get; set; }
    }

    [ProtoBuf.ProtoContract]
    public class EbJsValidator : EbObject
    {
        [ProtoBuf.ProtoMember(1)]
        public string JsCode { get; set; }
    }
}
