﻿using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.Structures;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
	[DataContract]
	public class GetManageLeadRequest : IReturn<GetManageLeadResponse>, IEbSSRequest
	{
		[DataMember(Order = 1)]
		public string AccId { get; set; }

		[DataMember(Order = 2)]
		public int RequestMode { get; set; }

		[DataMember(Order = 3)]
		public string TenantAccountId { get; set; }

		public int UserId { get; set; }
	}

	[DataContract]
	public class GetManageLeadResponse : IEbSSResponse
	{
		[DataMember(Order = 1)]
		public string AccId { get; set; }

		[DataMember(Order = 2)]
		public Dictionary<string, string> CustomerDataDict { get; set; }

		[DataMember(Order = 3)]
		public Dictionary<int, string> CostCenterDict { get; set; }

		[DataMember(Order = 3)]
		public List<FeedbackEntry> FeedbackList { get; set; }

		[DataMember(Order = 3)]
		public List<GraftEntry> GraftList { get; set; }

		[DataMember(Order = 3)]
		public List<BillingEntry> BillingList { get; set; }

		[DataMember(Order = 4)]
		public ResponseStatus ResponseStatus { get; set; }

	}

	[DataContract]
	public class SaveCustomerRequest : IReturn<SaveCustomerResponse>, IEbSSRequest
	{
		[DataMember(Order = 1)]
		public string CustomerData { get; set; }

		[DataMember(Order = 2)]
		public int RequestMode { get; set; }

		[DataMember(Order = 3)]
		public string TenantAccountId { get; set; }

		public int UserId { get; set; }
	}

	[DataContract]
	public class SaveCustomerResponse : IEbSSResponse
	{
		[DataMember(Order = 1)]
		public int Status { get; set; }

		[DataMember(Order = 3)]
		public ResponseStatus ResponseStatus { get; set; }

	}

	[DataContract]
	public class SaveCustomerFollowupRequest : IReturn<SaveCustomerFollowupResponse>, IEbSSRequest
	{
		[DataMember(Order = 1)]
		public string Data { get; set; }

		[DataMember(Order = 2)]
		public int RequestMode { get; set; }

		[DataMember(Order = 3)]
		public string TenantAccountId { get; set; }

		public int UserId { get; set; }
	}

	[DataContract]
	public class SaveCustomerFollowupResponse : IEbSSResponse
	{
		[DataMember(Order = 1)]
		public int Status { get; set; }

		[DataMember(Order = 3)]
		public ResponseStatus ResponseStatus { get; set; }

	}

	[DataContract]
	public class SaveCustomerGraftRequest : IReturn<SaveCustomerGraftResponse>, IEbSSRequest
	{
		[DataMember(Order = 1)]
		public string Data { get; set; }

		[DataMember(Order = 2)]
		public int RequestMode { get; set; }

		[DataMember(Order = 3)]
		public string TenantAccountId { get; set; }

		public int UserId { get; set; }
	}

	[DataContract]
	public class SaveCustomerGraftResponse : IEbSSResponse
	{
		[DataMember(Order = 1)]
		public int Status { get; set; }

		[DataMember(Order = 3)]
		public ResponseStatus ResponseStatus { get; set; }

	}

	[DataContract]
	public class SaveCustomerPaymentRequest : IReturn<SaveCustomerPaymentResponse>, IEbSSRequest
	{
		[DataMember(Order = 1)]
		public string Data { get; set; }

		[DataMember(Order = 2)]
		public int RequestMode { get; set; }

		[DataMember(Order = 3)]
		public string TenantAccountId { get; set; }

		public int UserId { get; set; }
	}

	[DataContract]
	public class SaveCustomerPaymentResponse : IEbSSResponse
	{
		[DataMember(Order = 1)]
		public int Status { get; set; }

		[DataMember(Order = 3)]
		public ResponseStatus ResponseStatus { get; set; }

	}

	[DataContract]
	public class KeyValueType_Field
	{
		[DataMember(Order = 1)]
		public string Key { get; set; }

		[DataMember(Order = 2)]
		public object Value { get; set; }
		
		[DataMember(Order = 3)]
		public EbDbTypes Type { get; set; }
	}

	[DataContract]
	public class FeedbackEntry
	{
		[DataMember(Order = 1)]
		public string Id { get; set; }

		[DataMember(Order = 2)]
		public string Date { get; set; }

		[DataMember(Order = 3)]
		public string Status { get; set; }

		[DataMember(Order = 4)]
		public string Followup_Date { get; set; }
		
		[DataMember(Order = 5)]
		public string Comments { get; set; }
	}

	[DataContract]
	public class GraftEntry
	{
		[DataMember(Order = 1)]
		public string Id { get; set; }

		[DataMember(Order = 2)]
		public string NoOfGrafts { get; set; }

		[DataMember(Order = 3)]
		public string TotalRate { get; set; }

		[DataMember(Order = 4)]
		public string ConsultingDoctor { get; set; }

		[DataMember(Order = 5)]
		public string ProbableMonth { get; set; }
	}

	[DataContract]
	public class BillingEntry
	{
		[DataMember(Order = 6)]
		public string Id { get; set; }

		[DataMember(Order = 1)]
		public string Date { get; set; }

		[DataMember(Order = 2)]
		public string Total { get; set; }

		[DataMember(Order = 3)]
		public string Balance { get; set; }

		[DataMember(Order = 4)]
		public string CashPaid { get; set; }

		[DataMember(Order = 5)]
		public string Narration { get; set; }
	}

	public class SurgeryEntry
	{
		[DataMember(Order = 3)]
		public string Id { get; set; }

		[DataMember(Order = 1)]
		public string Date { get; set; }

		[DataMember(Order = 2)]
		public string Branch { get; set; }

	}
}
