using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using InsuranceIssueApp.Model;
using InsuranceIssueApp.Repository;
using System.Globalization;
using System.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web.Http.Cors;

namespace InsureCoverWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InsuranceAPIController : ApiController
    {
        // GET api/<controller>
        IPolicyRepository repository = null;
        IUserRepository userRepository = null;
        IEnquiryRepository enquiryRepository = null;
        public InsuranceAPIController()
        {
            repository = new PolicyRepository();
            userRepository = new UserRepository();
            enquiryRepository = new EnquiryRepository();
        }


        //[HttpGet]
        //public IEnumerable<string> GetDetail()
        //{
        //    return new string[] { "value1", "value2" };
        //}


        // POST api/<controller>
        [HttpPost]
        public string CreateBasicDetail([FromBody]InsurrerDetail detail)
        {
            long result = 0;
            try
            {
                detail.MiddleName = detail.MiddleName == null ? "" : detail.MiddleName;
                detail.TempPolicyNo = 0;
                detail.CreatedBy = detail.AgentId;
                detail.CreatedDate = DateTime.Now;
                detail.ModifyDate = DateTime.Now;
                detail.PolicyStatus = 1;
                result = repository.AddBasicDetail(detail);
            }
            catch (Exception dbEx)
            {
                return Convert.ToString(result);
            }
            return Convert.ToString(result);
        }

        [HttpPost]
        public long CreatePersonalDetail([FromBody]PersonalDetail detail)
        {
            long result = 0;
            try
            {
                result = repository.AddPersonalDetail(detail);
            }
            catch (Exception dbEx)
            {
                return 0;
            }
            return result;
        }


        [HttpPost]
        public long CreateNomineeDetail([FromBody]NomineeDetail detail)
        {
            long result = 0;
            try
            {
                detail.NomineeMiddleName = detail.NomineeMiddleName == null ? "" : detail.NomineeMiddleName;
                result = repository.AddNomineeDetail(detail);
            }
            catch (Exception dbEx)
            {
                return result;
            }
            return result;
        }

        [HttpPost]
        public long CreateMedicalDetail([FromBody]MedicalDetail detail)
        {
            long result = 0;
            try
            {
                result = repository.AddMedicalDetail(detail);
            }
            catch (Exception dbEx)
            {
                return result;
            }
            return result;
        }

        [HttpPost]
        public long CreatePolicyDetail([FromBody]PolicyDetail detail)
        {
            long result = 0;
            try
            {
                result = repository.AddPolicyDetail(detail);
            }
            catch (Exception dbEx)
            {
                return result;
            }
            return result;
        }
        [HttpGet]
        public List<InsurrerDetail> GetTop5PolicyInQueue([FromUri]string loginid)
        {
            return userRepository.GetBasicDetail(loginid);
        }


        [HttpPost]
        public long AddUnwriterReviewDetail([FromBody]UnderWriterReview detail)
        {
            long result = 0;
            try
            {
                result = repository.AddUnwriterReviewDetail(detail);
                PolicyDetailViewModel policydetail = repository.GetPolicyAllDetail(detail.TempPolicyNo);
                SendApproveMail(policydetail);
            }
            catch (Exception dbEx)
            {
                return result;
            }
            return result;
        }

        [HttpPost]
        public long InsertCallbackDetail([FromBody]EnquiryCallbackDetail detail)
        {
            long result = 0;
            try
            {
                var date = DateTime.ParseExact(detail.CallbackTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                detail.CallBackDate = date;
                result = enquiryRepository.InsertCallbackDetail(detail);
            }
            catch (Exception dbEx)
            {
                return result;
            }
            return result;
        }

        [HttpPost]
        public List<AgentSalesReportResult> RepAgentSalesResult([FromUri]DateTime fromDate, DateTime toDate, int agentid)
        {
            List<AgentSalesReportResult> list = null;
            try
            {
                list = repository.RepAgentSalesResult(fromDate, toDate, agentid);
                return list;
            }
            catch (Exception ex)
            {
                return list;
            }
        }

        [HttpPost]
        public DashboardCount GetUserDashboardDetail([FromUri]int userid)
        {
            DashboardCount count = userRepository.GetDashboardCount(userid);
            return count;
        }

        [HttpPost]
        public List<AgentCommissionResult> GenerateCommission([FromUri]DateTime fromDate, DateTime toDate, int userid, string reportflag)
        {
            List<AgentCommissionResult> detail = repository.GenerateCommission(fromDate, toDate, userid, reportflag);
            return detail;
        }

        [HttpPost]
        public List<NameValueType> GetAgentList()
        {
            List<NameValueType> list = repository.BindDropDownList("AgentList");
            return list;
        }

        private bool SendApproveMail(PolicyDetailViewModel detail)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"].ToString();
            var client = new SendGridClient(apiKey);
            var bodymessage = @"<html><head><title> Policy Detail </title> <style> table {    border-collapse: collapse; } table, th, td {    border: 1px solid black;
                        }</style>
                    </head><body><table >" +
                "<thead class='thead-dark'><tr><td>Policy No</td><td>Customer Name</td><td>Date Of Birth</td><td>Phone No</td><td>Sum Assured</td><td>Policy Created Date</td><td>PremiumAmount</td></tr></thead>" +
                "<tr><td>" + detail.PolicyNo + "</td><td>" + detail.FirstName + " " + detail.LastName + "</td><td>" + detail.DateofBirth + "</td><td>" + detail.MobileNo + "</td><td>" + detail.SumAssured + "</td><td>" + detail.CreatedDate + "</td><td>" + detail.PremiumAmount + "</td></tr>" +
                "</table>" +
                "<br/> THIS IS AN AUTOMATED MESSAGE - PLEASE DO NOT REPLY DIRECTLY TO THIS EMAIL." +
                "</body></html>";
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("charlesjosh.j@gmail.com", "Charles"),
                Subject = "Policy No : " + detail.PolicyNo + " has been Approved.",
                // PlainTextContent = "Hello, Email!",
                HtmlContent = bodymessage
            };
            msg.AddTo(new EmailAddress("charles.josephamalraj@mindtree.com", "Insurance Admin"));
            var response = client.SendEmailAsync(msg);
            return true;
        }

        [HttpGet]
        public List<NameValueType> AgentDetail()
        {
            List<NameValueType> list = repository.BindDropDownList("AgentList");
            return list;
        }

        [HttpPost]
        public List<Enquiry> EnquiryList(string fromdate, string todate)
        {
            List<Enquiry> listEnquiies = null;
            DateTime dtStartDate = DateTime.ParseExact(fromdate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime dtEndDate = DateTime.ParseExact(todate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
           // string keyname = "Enquiry_" + fromdate + todate + (int)Session["UserId"];           
            listEnquiies = enquiryRepository.GetAllEnquires(dtStartDate, dtEndDate, 1);             
            return listEnquiies;
        }
    }
}
