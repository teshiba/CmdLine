using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace CmdLineProcess.Tests
{
    [TestClass]
    public class CmdLineTests
    {
        private const string MsBuildPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe";
        private readonly string MsBuildVersion = "16.5.0.12403";
        private readonly ProcessStartInfo expectedPSI = new ProcessStartInfo(){
            FileName = MsBuildPath,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = @".\",
        };

        [TestMethod]
        public void CmdLineTest()
        {
            // Arrange
            BindingFlags BindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
            
            static void onReceive(string arg)
            { }

            static void onExit(int arg)
            { }

            // Act
            var testClass = new CmdLine(
                expectedPSI.FileName,
                expectedPSI.WorkingDirectory,
                onReceive,
                onExit
                );

            // Assert
            var receiveAction = (Action<string>)testClass.GetType().GetField("onReceive", BindingAttr).GetValue(testClass);
            var exitAction = (Action<int>)testClass.GetType().GetField("onExit", BindingAttr).GetValue(testClass);

            Assert.AreEqual(true, testClass.EnableRaisingEvents);
            Assert.AreEqual(expectedPSI.FileName, testClass.StartInfo.FileName);
            Assert.AreEqual(expectedPSI.CreateNoWindow, testClass.StartInfo.CreateNoWindow);
            Assert.AreEqual(expectedPSI.UseShellExecute, testClass.StartInfo.UseShellExecute);
            Assert.AreEqual(expectedPSI.RedirectStandardOutput, testClass.StartInfo.RedirectStandardOutput);
            Assert.AreEqual(expectedPSI.RedirectStandardError, testClass.StartInfo.RedirectStandardError);
            Assert.AreEqual(expectedPSI.WorkingDirectory, testClass.StartInfo.WorkingDirectory);
            Assert.AreEqual(onExit, exitAction);
            Assert.AreEqual(onReceive, receiveAction);
        }

        [TestMethod()]
        public void StartTestWithArg()
        {
            // Arrange
            var outstr = string.Empty;
            var retcode = 0;

            void onReceive(string arg)
            {
                outstr += arg + Environment.NewLine;
            }

            void onExit(int arg)
            {
                retcode = arg;
            }

            // Act
            var testClass = new CmdLine(
                expectedPSI.FileName,
                expectedPSI.WorkingDirectory,
                onReceive,
                onExit
                );

            // Act
            testClass.Start(@"-ver");
            testClass.WaitForExit();
            
            // Assert
            Assert.AreEqual(retcode, 0);
            Assert.IsTrue(outstr.Contains(MsBuildVersion));
        }

        [TestMethod()]
        public void StartTestArg()
        {
            // Arrange
            var retcode = 0;

            // Act
            var testClass = new CmdLine(
                expectedPSI.FileName,
                expectedPSI.WorkingDirectory,
                new Action<string>((arg) => { }),
                new Action<int>((arg) => { })
                );

            // Act
            testClass.Start();
            testClass.WaitForExit();

            // Assert
            Assert.AreEqual(retcode, 0);
        }

        [TestMethod()]
        public void StartTestHandlerNull()
        {
            // Arrange
            var retcode = 0;

            // Act
            var testClass = new CmdLine(expectedPSI.FileName, expectedPSI.WorkingDirectory, null, null);

            // Act
            testClass.Start();
            testClass.WaitForExit();

            // Assert
            Assert.AreEqual(retcode, 0);
        }

        [TestMethod()]
        public async Task StartAsyncTestAsync()
        {
            // Arrange
            var outstr = string.Empty;
            var retcode = 0;

            void onReceive(string arg)
            {
                outstr += arg + Environment.NewLine;
            }

            void onExit(int arg)
            {
                retcode = arg;
            }

            // Act
            var testClass = new CmdLine(
                expectedPSI.FileName,
                expectedPSI.WorkingDirectory,
                onReceive,
                onExit
                );

            // Act
            var actualRetCode = await testClass.StartAsync(@"-ver");

            // Assert
            Assert.AreEqual(retcode, actualRetCode);
            Assert.IsTrue(outstr.Contains(MsBuildVersion));
        }
    }
}