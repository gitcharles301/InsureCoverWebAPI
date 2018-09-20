using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InsuranceIssueApp.Model;
using InsuranceIssueApp.Repository;
using System.Collections.Generic;

namespace InsureCoverWebAPI.Tests
{
    [TestClass]
    public class InsuranceWebAPITestClass
    {
        IPolicyRepository repository = null;
        IUserRepository userRepository = null;
        IEnquiryRepository enquiryRepository = null;
        public InsuranceWebAPITestClass()
        {
            repository = new PolicyRepository();
            userRepository = new UserRepository();
            enquiryRepository = new EnquiryRepository();
        }

        [TestMethod]
        public void TestGetTop5PolicyInQueue()
        {
            bool result = false;
            List<InsurrerDetail> detail = userRepository.GetBasicDetail("1");
            if (detail.Count > 0)
                result = true;
            Assert.AreEqual(true, result);
        }
        [TestMethod]
        public void AddBasicDetail()
        {
            var detail = new InsurrerDetail();
            detail.FirstName = "Charles";
            detail.MiddleName = detail.MiddleName == null ? "" : detail.MiddleName;
            detail.LastName = "J";
            detail.TempPolicyNo = 0;
            detail.CreatedBy = 1;
            detail.CreatedDate = DateTime.Now;
            detail.ModifyDate = DateTime.Now;
            detail.PolicyStatus = 1;
            long result = repository.AddBasicDetail(detail);
        }

        [TestMethod]
        public void TestGetAgentList()
        {
            bool result = false;
            List<NameValueType> list = repository.BindDropDownList("AgentList");
            if (list.Count > 0)
                result = true;
            Assert.AreEqual(true, result);
        }

    }
}
