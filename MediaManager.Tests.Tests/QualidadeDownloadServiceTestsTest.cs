// <copyright file="QualidadeDownloadServiceTestsTest.cs">Copyright ©  2015</copyright>

using System;
using MediaManager.Tests.Services;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaManager.Tests.Services.Tests
{
    [TestClass]
    [PexClass(typeof(QualidadeDownloadServiceTests))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class QualidadeDownloadServiceTestsTest
    {

        [PexMethod(MaxBranches = 20000)]
        public void AdicionarTest([PexAssumeUnderTest]QualidadeDownloadServiceTests target)
        {
            target.AdicionarTest();
            // TODO: add assertions to method QualidadeDownloadServiceTestsTest.AdicionarTest(QualidadeDownloadServiceTests)
        }
    }
}
