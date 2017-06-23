﻿using ExpressBase.Common;
using ExpressBase.Security;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using ServiceStack.Web;
using System.IO;
using ExpressBase.Data;
using ServiceStack.Logging;
using System.Runtime.Serialization;
using ExpressBase.Objects.ServiceStack_Artifacts;
using System.Globalization;

namespace ExpressBase.Objects.ServiceStack_Artifacts
{
    [DataContract]
    public class MyAuthenticateResponse : AuthenticateResponse
    {
        [DataMember(Order = 1)]
        public User User { get; set; }
    }

    [DataContract]
    public class CustomUserSession : AuthUserSession
    {
        [DataMember(Order = 1)]
        public string CId { get; set; }

        [DataMember(Order = 2)]
        public int Uid { get; set; }

        [DataMember(Order = 3)]
        public User User { get; set; }

        [DataMember(Order = 4)]
        public string WhichConsole { get; set; }

        public override bool IsAuthorized(string provider)
        {
            return true;
        }

        private static string CreateGravatarUrl(string email, int size = 64)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var md5HadhBytes = md5.ComputeHash(email.ToUtf8Bytes());

            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < md5HadhBytes.Length; i++)
                sb.Append(md5HadhBytes[i].ToString("x2"));

            string gravatarUrl = "http://www.gravatar.com/avatar/{0}?d=mm&s={1}".Fmt(sb, size);
            return gravatarUrl;
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);

            //Populate all matching fields from this session to your own custom User table
            var user = session.ConvertTo<User>();
            user.Id = int.Parse(session.UserAuthId);
            user.Profileimg = !session.Email.IsNullOrEmpty()
                ? CreateGravatarUrl(session.Email, 64)
                : null;

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    user.UserName = authToken.DisplayName;
                    user.FirstName = authToken.FirstName;
                    user.LastName = authToken.LastName;
                    user.Email = authToken.Email;
                    //session.bea
                }
                //else if (authToken.Provider == TwitterAuthProvider.Name)
                //{
                //    user.TwitterName = user.DisplayName = authToken.UserName;
                //}
                //else if (authToken.Provider == YahooOpenIdOAuthProvider.Name)
                //{
                //    user.YahooUserId = authToken.UserId;
                //    user.YahooFullName = authToken.FullName;
                //    user.YahooEmail = authToken.Email;
                //}
            }

            //var userAuthRepo = authService.TryResolve<IAuthRepository>();
            //if (AppHost.AppConfig.AdminUserNames.Contains(session.UserAuthName)
            //    && !session.HasRole(RoleNames.Admin, userAuthRepo))
            //{
            //    var userAuth = userAuthRepo.GetUserAuth(session, tokens);
            //    userAuthRepo.AssignRoles(userAuth, roles: new[] { RoleNames.Admin });
            //}

            //Resolve the DbFactory from the IOC and persist the user info
            //using (var db = authService.TryResolve<IDbConnectionFactory>().Open())
            //{
            //    db.Save(user);
            //}
        }

        public class MyCredentialsAuthProvider: CredentialsAuthProvider
        {
            public MyCredentialsAuthProvider(IAppSettings settings) : base(settings) { }

            public override bool TryAuthenticate(IServiceBase authService, string UserName, string password)
            {
                User _authUser = null;
                ILog log = LogManager.GetLogger(GetType());
                var request = authService.Request.Dto as Authenticate;


                var cid = request.Meta.ContainsKey("cid") ?  request.Meta["cid"] : string.Empty;
                var socialId = request.Meta.ContainsKey("socialId") ? request.Meta["socialId"] : string.Empty;                      

                EbBaseService bservice = new EbBaseService();

                if (request.Meta.ContainsKey("signup_tok"))
                {
                    cid = "expressbase";
                    var _InfraDb = authService.TryResolve<DatabaseFactory>().InfraDB as IDatabase;
                    _authUser = User.GetInfraVerifiedUser(_InfraDb, UserName, request.Meta["signup_tok"]);
                }
                else
                {
                    if (cid == "expressbase")
                    {
                        var _InfraDb = authService.TryResolve<DatabaseFactory>().InfraDB as IDatabase;
                        _authUser = (string.IsNullOrEmpty(socialId)) ? User.GetInfraUser(_InfraDb, UserName, password) : User.GetInfraUserViaSocial(_InfraDb, UserName, socialId);
                        log.Info("#Eb reached 1");
                    }
                    else
                    {
                        bservice.ClientID = cid;
                        _authUser = User.GetDetails(bservice.DatabaseFactory, UserName, password);
                        log.Info("#Eb reached 2");
                    }
                }

                if (_authUser != null)
                {
                    CustomUserSession session = authService.GetSession(false) as CustomUserSession;

                    session.Company =cid;
                    session.UserAuthId = _authUser.Id.ToString();
                    session.UserName = UserName;
                    session.IsAuthenticated = true;
                    session.User = _authUser;
                }

                return (_authUser != null);
            }

            public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
            {
                AuthenticateResponse authResponse = base.Authenticate(authService, session, request) as AuthenticateResponse;

                var _customUserSession = authService.GetSession() as CustomUserSession;
                _customUserSession.WhichConsole = request.Meta.ContainsKey("wc") ? request.Meta["wc"] : string.Empty;

                if (!string.IsNullOrEmpty(authResponse.SessionId) && _customUserSession != null)
                {
                    return new MyAuthenticateResponse
                    {
                        UserId = _customUserSession.UserAuthId,
                        UserName = _customUserSession.UserName,
                        User = _customUserSession.User,
                    };
                }

                return authResponse;
            }

            public override object Logout(IServiceBase service, Authenticate request)
            {
                return base.Logout(service, request);
            }
        }
    }
}
