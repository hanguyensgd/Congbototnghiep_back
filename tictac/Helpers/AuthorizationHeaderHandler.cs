using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using tictac.Helper;

namespace tictac.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Data;
    using System.Data.SqlClient;
 

    /// <summary>   
    /// Authorization for web API class.   
    /// </summary>   
    public class AuthorizationHeaderHandler : DelegatingHandler
    {
       
        //region Send method.
        /// <summary>   
        /// Send method.   
        /// </summary>   
        /// <param name="request">Request parameter</param>   
        /// <param name="cancellationToken">Cancellation token parameter</param>   
        /// <returns>Return HTTP response.</returns>   
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Initialization.   
            IEnumerable<string> apiKeyHeaderValues = null;
            //AuthenticationHeaderValue authorization = request.Headers.Authorization;
            //string userName = null;
            //string password = null;
            // Verification.   
            if (request.Headers.TryGetValues("HEADER_TOKEN", out apiKeyHeaderValues) /*&& !string.IsNullOrEmpty(authorization.Parameter)*/)
            {
                var apiKeyHeaderValue = apiKeyHeaderValues.First();
                // Get the auth token   
                //string authToken = authorization.Parameter;
                // Decode the token from BASE64   
                //string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                // Extract username and password from decoded token   
                //userName = decodedToken.Substring(0, decodedToken.IndexOf(":"));
                //password = decodedToken.Substring(decodedToken.IndexOf(":") + 1);
                // Verification.  
                String verifyResult = SessionVerify(apiKeyHeaderValue);
                if (!String.IsNullOrEmpty(verifyResult)/*apiKeyHeaderValue.Equals("HEADER_TOKEN")*/ /*&& userName.Equals("USERNAME_VALUE") && password.Equals("PASSWORD_VALUE")*/)
                {

                    string[] roles = new string[1];               
                    roles[0] = verifyResult;            
                    // Setting   
                    var identity = new GenericIdentity(apiKeyHeaderValue);
                    SetPrincipal(new GenericPrincipal(identity, roles));
                }
            }
            // Info.   
            return base.SendAsync(request, cancellationToken);
        }  
     
        private static void SetPrincipal(IPrincipal principal)
        {
            // setting.   
            Thread.CurrentPrincipal = principal;
            // Verification.   
            if (HttpContext.Current != null)
            {
                // Setting.   
                HttpContext.Current.User = principal;

            }
        }

        public String SessionVerify(String Token)
        {      
            SQL_function sql_function = new SQL_function();
            String strQuery = @"SELECT [UserName] FROM [DuLieuTotNghiep].[dbo].[Session]
            WHERE ID = @ID AND DATEDIFF(HOUR, GETDATE(), [TimeStampt]) < 3 AND isActive = 1 ";

            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar).Value = Token;

            String[,] array = sql_function.xml_deserialize(sql_function.GetData(cmd, "sqlconnString"));

            if(array.GetLength(0) == 0)
            {
                return null;
            }
            else
            {
                return array[0,0].ToString();
            }
        }

    }
}