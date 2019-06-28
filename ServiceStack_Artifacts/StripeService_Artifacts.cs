﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ServiceStack;
using ExpressBase.Common.Stripe;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
    public class CreateCustomerRequest : IEbTenentRequest, IReturn<CreateCustomerResponse>
    {
        public string EmailId { get; set; }

        public string TokenId { get; set; }

        public int UserId { get; set; }

        public string SolnId { get; set; }
    }

    public class CreateCustomerResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public  string  CustomerId { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GetCustomerRequest : EbServiceStackAuthRequest, IReturn<GetCustomerResponse>
    {
        public string CustId { get; set; }
    }

    public class GetCustomerResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Address { get; set; }

        [DataMember(Order = 3)]
        public string City { get; set; }

        [DataMember(Order = 4)]
        public string State { get; set; }

        [DataMember(Order = 5)]
        public string Country { get; set; }

        [DataMember(Order = 6)]
        public string Zip { get; set; }

        [DataMember(Order = 7)]
        public string Email { get; set; }

        [DataMember(Order = 8)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GetCardRequest : EbServiceStackAuthRequest, IReturn<GetCardResponse>
    {
        public string CustId { get; set; }
    }

    public class GetCardResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public string Last4 { get; set; }

        [DataMember(Order = 2)]
        public long ExpMonth { get; set; }

        [DataMember(Order = 3)]
        public long ExpYear { get; set; }

        [DataMember(Order = 4)]
        public ResponseStatus ResponseStatus { get; set; }
    }


    public class CheckCustomerSubscribedRequest : IEbTenentRequest, IReturn<CheckCustomerSubscribedResponse>
    {
        public int UserId { get; set; }

        public string SolnId { get; set; }
    }

    public class CheckCustomerSubscribedResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public string Plan { get; set; }

        [DataMember(Order = 2)]
        public int Users { get; set; }

        [DataMember(Order = 3)]
        public string CustId { get; set; }

        [DataMember(Order = 4)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CheckCustomerRequest : EbServiceStackAuthRequest, IReturn<CheckCustomerResponse>
    {
        public string EmailId { get; set; }

        public string TokenId { get; set; }

        public string SolutionId { get; set; }
    }

    public class CheckCustomerResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class UpdateCardRequest : EbServiceStackAuthRequest, IReturn<UpdateCardResponse>
    {
        public string CustId { get; set; }

        public string CardId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Zip { get; set; }
    }

    public class UpdateCardResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreateChargeRequest : EbServiceStackAuthRequest, IReturn<CreateChargeResponse>
    {
        public string CustId { get; set; }
    }

    public class CreateChargeResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreateCharge2Request : EbServiceStackAuthRequest, IReturn<CreateCharge2Response>
    {
        public string CustId { get; set; }

        public string Total { get; set; }
    }

    public class CreateCharge2Response : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreatePlanRequest : EbServiceStackAuthRequest, IReturn<CreatePlanResponse>
    {
        public string Total { get; set; }

        public int Interval { get; set; }

        public int Interval_count { get; set; }
    }

    public class CreatePlanResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public string PlanId { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GetPlansRequest : EbServiceStackAuthRequest, IReturn<GetPlansResponse>
    {
    }

    public class GetPlansResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public Eb_StripePlansList Plans { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreateCouponRequest : EbServiceStackAuthRequest, IReturn<CreateCouponResponse>
    {
        public int Duration { get; set; }

        public int PercentageOff { get; set; }

        public int DurationInMonth { get; set; }

        public int RedeemBy { get; set; }

        public int MaxRedeem { get; set; }
    }

    public class CreateCouponResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public string CouponId { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreateSubscriptionRequest : EbServiceStackAuthRequest, IReturn<CreateSubscriptionResponse>
    {
        public string CustId { get; set; }

        public string PlanId { get; set; }

        public string CoupId { get; set; }

        public string SolutionId { get; set; }

        public long? Total { get; set; }
    }

    public class CreateSubscriptionResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public DateTime PeriodStart { get; set; }

        [DataMember(Order = 2)]
        public DateTime PeriodEnd { get; set; }

        [DataMember(Order = 3)]
        public DateTime Created { get; set; }

        [DataMember(Order = 4)]
        public long? Amount { get; set; }

        [DataMember(Order = 5)]
        public string UseageType { get; set; }

        [DataMember(Order = 6)]
        public string BillingScheme { get; set; }

        [DataMember(Order = 7)]
        public long Quantity { get; set; }

        [DataMember(Order = 8)]
        public string Plan { get; set; }

        [DataMember(Order = 9)]
        public ResponseStatus ResponseStatus { get; set; }
    }

  
    public class UpgradeSubscriptionRequest : EbServiceStackAuthRequest, IReturn<UpgradeSubscriptionResponse>
    {
        public string PlanId { get; set; }

        public int Total { get; set; }

        public string SolutionId { get; set; }
    }

    public class UpgradeSubscriptionResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public DateTime PeriodStart { get; set; }

        [DataMember(Order = 2)]
        public DateTime PeriodEnd { get; set; }

        [DataMember(Order = 3)]
        public DateTime Created { get; set; }

        [DataMember(Order = 4)]
        public long? Amount { get; set; }

        [DataMember(Order = 5)]
        public string UseageType { get; set; }

        [DataMember(Order = 6)]
        public string BillingScheme { get; set; }

        [DataMember(Order = 7)]
        public long Quantity { get; set; }

        [DataMember(Order = 8)]
        public string Plan { get; set; }

        [DataMember(Order = 9)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class CreateInvoiceRequest : EbServiceStackAuthRequest, IReturn<CreateInvoiceResponse>
    {
        public string CustId { get; set; }

        public string Total { get; set; }
    }

    public class CreateInvoiceResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class StripewebhookRequest : EbServiceStackAuthRequest, IReturn<StripewebhookResponse>
    {
        public string Json { get; set; }
    }

    public class StripewebhookResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GetCustomerInvoiceRequest : EbServiceStackAuthRequest, IReturn<GetCustomerInvoiceResponse>
    {
        public string CustId { get; set; }
    }

    public class GetCustomerInvoiceResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public Eb_StripeInvoiceList Invoices { get; set; }

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class GetCustomerUpcomingInvoiceRequest : EbServiceStackAuthRequest, IReturn<GetCustomerUpcomingInvoiceResponse>
    {
        public string CustId { get; set; }
    }

    public class GetCustomerUpcomingInvoiceResponse : IEbSSResponse
    {
        [DataMember(Order = 1)]
        public Eb_StripeUpcomingInvoiceList Invoice {get;set;}

        [DataMember(Order = 2)]
        public ResponseStatus ResponseStatus { get; set; }
    }
}